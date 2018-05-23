using CLRTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading
{
    /// <summary>
    /// This class implements a reader/writer lock that never blocks any threads.
    /// To use, await the result of AccessAsync and, after manipulating shared state,
    /// call Release.
    /// </summary>
    public sealed class AsyncOneManyLockEx
    {
        #region TaskItem
        private class WaitItem
        {
            public WaitItem(TaskCompletionSource<Boolean> tcs, string receiver)
            {
                TCS = tcs;
                Receiver = receiver;
            }

            /// <summary>
            /// The TCS of task
            /// </summary>
            public TaskCompletionSource<Boolean> TCS { get; private set; }
            /// <summary>
            /// The receiver of task
            /// </summary>
            public string Receiver { get; private set; }
        }
        #endregion

        #region const
        public const int WaitLimitInfinite = -1;
        #endregion

        #region Lock code
        private SpinLock m_lock = new SpinLock(true);   // Don't use readonly with a SpinLock
        private void Lock() { Boolean taken = false; m_lock.Enter(ref taken); }
        private void Unlock() { m_lock.Exit(); }
        #endregion

        #region Lock state and helper methods
        private Int32 m_state = 0;
        private Boolean IsFree { get { return m_state == 0; } }
        private Boolean IsOwnedByWriter { get { return m_state == -1; } }
        private Boolean IsOwnedByReaders { get { return m_state > 0; } }
        private Int32 AddReaders(Int32 count) { return m_state += count; }
        private Int32 SubtractReader() { return --m_state; }
        private void MakeWriter() { m_state = -1; }
        private void MakeFree() { m_state = 0; }
        #endregion

        #region limit
        private int _waitLimit = -1;
        private Boolean IsReachLimit()
        {
            return _waitLimit != WaitLimitInfinite &&
                m_WaitingWriters.Count + m_WaitingReaders.Count >= _waitLimit;
        }
        #endregion

        // For the no-contention case to improve performance and reduce memory consumption
        private readonly Task<Boolean> m_noContentionAccessGranter;

        // Each waiting writers wakes up via their own TaskCompletionSource queued here
        private readonly List<WaitItem> m_WaitingWriters = new List<WaitItem>();

        // Each waiting readers wakes up via their own TaskCompletionSource queued here
        private readonly List<WaitItem> m_WaitingReaders = new List<WaitItem>();

        /// <summary>
        /// The writer or readers that being processing
        /// </summary>
        private readonly List<WaitItem> m_ProcessingItems = new List<WaitItem>();

        /// <summary>Constructs an AsyncOneManyLock object.</summary>
        /// <param name="waitLimit">The wait tasks limit, -1</param>
        public AsyncOneManyLockEx(int waitLimit)
        {
            if (waitLimit != WaitLimitInfinite && waitLimit <= 0)
                throw new ArgumentOutOfRangeException("waitLimit", waitLimit, "waitLimit value can be -1 or bigger than 0");

            _waitLimit = waitLimit;
            m_noContentionAccessGranter = Task.FromResult<Boolean>(true);
        }

        /// <summary>
        /// Asynchronously requests access to the state protected by this AsyncOneManyLock.
        /// </summary>
        /// <param name="exclusive">Specifies whether you want exclusive (write) access or shared (read) access.</param>
        /// <param name="timeout">The timeout of the wait task, use Timeout.InfiniteTimeSpan to indicate no timeout</param>
        /// <param name="ct">Use to cancel wait</param>
        /// <param name="receiver">The receiver of wait task, use to add in Exception.Message when timeout or reach the limit, this will help for debug</param>
        /// <returns>A Task to await, return true: wait success, false: reach the wait limit</returns>
        public Task WaitAsync(Boolean exclusive, TimeSpan timeout, CancellationToken ct, string receiver)
        {
            if (timeout != Timeout.InfiniteTimeSpan && timeout.Ticks < 0)
                throw new ArgumentOutOfRangeException("timeout", timeout, "timeout can be Timeout.InfiniteTimeSpan or bigger than 0");

            TaskCompletionSource<Boolean> accressGranter = new TaskCompletionSource<Boolean>(); // Assume no contention
            WaitItem wi = new WaitItem(accressGranter, receiver);

            Lock();
            if (IsReachLimit())
            {
                // Wait task reach the limit, return false
                accressGranter.SetException(new LockWaitLimitException(_waitLimit, GetProcessingReceivers()));
                Unlock();
                return accressGranter.Task;
            }

            if (exclusive)
            {
                if (IsFree)
                {
                    MakeWriter();  // No contention
                    m_ProcessingItems.Add(wi);
                }
                else
                {
                    // Contention: Queue new writer task & return it so writer waits
                    m_WaitingWriters.Add(wi);

                    if (ct != CancellationToken.None)
                        ct.Register(() => CancelWriterWaiting(wi));

                    if (timeout != Timeout.InfiniteTimeSpan)
                        Task.Delay(timeout, ct).ContinueWith(t => OnWriterWaitingTimeout(t, wi));
                }
            }
            else
            {
                if (IsFree || (IsOwnedByReaders && m_WaitingWriters.Count == 0))
                {
                    AddReaders(1); // No contention
                    m_ProcessingItems.Add(wi);
                }
                else
                {
                    // Contention: Queue new reader task & return it so reader waits
                    m_WaitingReaders.Add(wi);

                    if (ct != CancellationToken.None)
                        ct.Register(() => CancelReaderWaiting(wi));

                    if (timeout != Timeout.InfiniteTimeSpan)
                        Task.Delay(timeout, ct).ContinueWith(t => OnReaderWaitingTimeout(t, wi));
                }
            }
            Unlock();

            return accressGranter.Task;
        }

        /// <summary>
        /// Releases the AsyncOneManyLock allowing other code to acquire it
        /// </summary>
        public void Release()
        {
            List<WaitItem> accessGranters = new List<WaitItem>();   // Assume no code is released

            Lock();
            if (IsOwnedByWriter) MakeFree(); // The writer left
            else SubtractReader();           // A reader left

            if (IsFree)
            {
                // If free, wake 1 waiting writer or all waiting readers
                if (m_WaitingWriters.Count > 0)
                {
                    MakeWriter();
                    accessGranters.Add(m_WaitingWriters[0]);
                    m_WaitingWriters.RemoveAt(0);
                }
                else if (m_WaitingReaders.Count > 0)
                {
                    AddReaders(m_WaitingReaders.Count);
                    accessGranters.AddRange(m_WaitingReaders);

                    // Clear all reader task for future readers that need to wait
                    m_WaitingReaders.Clear();
                }
            }
            Unlock();

            // Wake the writer/reader outside the lock to reduce
            // chance of contention improving performance
            foreach (var ag in accessGranters)
                ag.TCS.SetResult(true);
        }

        private string GetProcessingReceivers()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m_ProcessingItems.Count && i < 10; i++)
                sb.Append(m_ProcessingItems[i].Receiver ?? "?").Append(',');

            if (m_ProcessingItems.Count > 10) sb.Append("..."); // Add "..." to indicate too more
            else if (m_ProcessingItems.Count > 0) sb.Remove(sb.Length - 1, 1); // Remove last ','

            sb.Insert(0, "Processing receivers:");

            return sb.ToString();
        }

        private void CancelWriterWaiting(WaitItem wi)
        {
            bool isCancel = false;
            Lock();
            if (m_WaitingWriters.Contains(wi))
            {
                isCancel = true;
                m_WaitingWriters.Remove(wi);
            }
            Unlock();

            if (isCancel) wi.TCS.SetCanceled();
        }

        private void OnWriterWaitingTimeout(Task delayTask, WaitItem wi)
        {
            if (delayTask.IsCanceled || delayTask.IsFaulted) return;

            bool isTimeout = false;
            Lock();
            if (m_WaitingWriters.Contains(wi))
            {
                isTimeout = true;
                m_WaitingWriters.Remove(wi);
            }
            Unlock();

            if (isTimeout) wi.TCS.SetException(new TimeoutException(GetProcessingReceivers()));
        }

        private void CancelReaderWaiting(WaitItem wi)
        {
            bool isCancel = false;
            Lock();
            if (m_WaitingReaders.Contains(wi))
            {
                isCancel = true;
                m_WaitingReaders.Remove(wi);
            }
            Unlock();

            if (isCancel) wi.TCS.SetCanceled();
        }

        private void OnReaderWaitingTimeout(Task delayTask, WaitItem wi)
        {
            if (delayTask.IsCanceled || delayTask.IsFaulted) return;

            bool isTimeout = false;
            Lock();
            if (m_WaitingReaders.Contains(wi))
            {
                isTimeout = true;
                m_WaitingReaders.Remove(wi);
            }
            Unlock();

            if (isTimeout) wi.TCS.SetException(new TimeoutException(GetProcessingReceivers()));
        }
    }
}
