﻿using System;
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
    public sealed class AsyncOneManyLock
    {
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

        // For the no-contention case to improve performance and reduce memory consumption
        private readonly Task m_noContentionAccessGranter;

        // Each waiting writers wakes up via their own TaskCompletionSource queued here
        private readonly Queue<TaskCompletionSource<Object>> m_qWaitingWriters =
           new Queue<TaskCompletionSource<Object>>();

        // All waiting readers wake up by signaling a single TaskCompletionSource
        private TaskCompletionSource<Object> m_waitingReadersSignal =
           new TaskCompletionSource<Object>();
        private Int32 m_numWaitingReaders = 0;

        /// <summary>Constructs an AsyncOneManyLock object.</summary>
        public AsyncOneManyLock()
        {
            m_noContentionAccessGranter = Task.FromResult<Object>(null);
        }

        /// <summary>
        /// Asynchronously requests access to the state protected by this AsyncOneManyLock.
        /// </summary>
        /// <param name="mode">Specifies whether you want exclusive (write) access or shared (read) access.</param>
        /// <returns>A Task to await.</returns>
        public Task WaitAsync(Boolean exclusive)
        {
            Task accressGranter = m_noContentionAccessGranter; // Assume no contention

            Lock();
            if (exclusive)
            {
                if (IsFree)
                {
                    MakeWriter();  // No contention
                }
                else
                {
                    // Contention: Queue new writer task & return it so writer waits
                    var tcs = new TaskCompletionSource<Object>();
                    m_qWaitingWriters.Enqueue(tcs);
                    accressGranter = tcs.Task;
                }
            }
            else
            {
                if (IsFree || (IsOwnedByReaders && m_qWaitingWriters.Count == 0))
                {
                    AddReaders(1); // No contention
                }
                else
                { // Contention
                  // Contention: Increment waiting readers & return reader task so reader waits
                    m_numWaitingReaders++;
                    accressGranter = m_waitingReadersSignal.Task.ContinueWith(t => t.Result);
                }
            }
            Unlock();

            return accressGranter;
        }

        /// <summary>
        /// Releases the AsyncOneManyLock allowing other code to acquire it
        /// </summary>
        public void Release()
        {
            TaskCompletionSource<Object> accessGranter = null;   // Assume no code is released

            Lock();
            if (IsOwnedByWriter) MakeFree(); // The writer left
            else SubtractReader();           // A reader left

            if (IsFree)
            {
                // If free, wake 1 waiting writer or all waiting readers
                if (m_qWaitingWriters.Count > 0)
                {
                    MakeWriter();
                    accessGranter = m_qWaitingWriters.Dequeue();
                }
                else if (m_numWaitingReaders > 0)
                {
                    AddReaders(m_numWaitingReaders);
                    m_numWaitingReaders = 0;
                    accessGranter = m_waitingReadersSignal;

                    // Create a new TCS for future readers that need to wait
                    m_waitingReadersSignal = new TaskCompletionSource<Object>();
                }
            }
            Unlock();

            // Wake the writer/reader outside the lock to reduce
            // chance of contention improving performance
            if (accessGranter != null) accessGranter.SetResult(null);
        }
    }
}
