using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.Book
{
    public class CallerMonitor
    {
        private List<Call> _waitingCalls = new List<Call>();
        private List<Call> _processingCalls = new List<Call>();
        private Timer _limitTimer;
        private TimeSpan _timerDueTime;

        public CallerMonitor(int limit, TimeSpan timeLimit)
        {
            _limit = limit;
            _timeLimit = timeLimit;
            _limitTimer = new Timer(OnOverTime, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        #region TimeLimit
        private TimeSpan _timeLimit;
        /// <summary>
        /// Get or set <see cref="TimeLimit"/>
        /// </summary>
        public TimeSpan TimeLimit
        {
            get { return _timeLimit; }
        }
        #endregion

        #region Limit
        private int _limit;
        /// <summary>
        /// Get or set <see cref="Limit"/>
        /// </summary>
        public int Limit
        {
            get { return _limit; }
        }
        #endregion

        #region WaitingCount
        private int _waitingCount;
        /// <summary>
        /// Get or set <see cref="WaitingCount"/>
        /// </summary>
        public int WaitingCount
        {
            get { return Volatile.Read(ref _waitingCount); }
        }
        #endregion

        #region ProcessingCount
        private int _processingCount;
        /// <summary>
        /// Get or set <see cref="ProcessingCount"/>
        /// </summary>
        public int ProcessingCount
        {
            get { return Volatile.Read(ref _processingCount); }
        }
        #endregion

        public void Wait(string callerFile, string callerFunc, TimeSpan timeLimit)
        {
            if (timeLimit != Timeout.InfiniteTimeSpan && timeLimit.Ticks <= 0)
                throw new ArgumentOutOfRangeException("timeLimit", $"timeLimit can not less or equal than 0: value={timeLimit}");

            Call c = new Call(callerFile, callerFunc, timeLimit);
        }

        public void Process()
        { }

        public void Leave()
        { }

        private void SetLimitTimer(TimeSpan ts)
        {
            if (ts == Timeout.InfiniteTimeSpan) return;

            if (ts < _timerDueTime)
                _limitTimer.Change(ts, Timeout.InfiniteTimeSpan);
        }

        private void OnOverTime(object state)
        {
            TimeSpan nextDueTime = Timeout.InfiniteTimeSpan;
            foreach (var c in _processingCalls)
            {
                if (c.TimeLimit != Timeout.InfiniteTimeSpan && c.TimeLimit < nextDueTime)
                    nextDueTime = c.TimeLimit;
            }

            _timerDueTime = TimeSpan.MaxValue;
            if (nextDueTime != Timeout.InfiniteTimeSpan)
                SetLimitTimer(nextDueTime);
        }
    }
}
