namespace MyREST
{
    [ToString]
    public class AppState
    {
        private long _runningRequests = 0;
        private long _completedRequests = 0;
        private long _failedRequests = 0;
        private long _succeededRequests = 0;

        public long runningRequests
        { get { return _runningRequests; } }

        public long completedRequests
        { get { return _completedRequests; } }

        public long failedRequests
        { get { return _failedRequests; } }

        public long succeededRequests
        { get { return _succeededRequests; } }

        public DateTime startTime { get; set; }

        public void markNewRequest()
        {
            Interlocked.Increment(ref _runningRequests);
        }

        public void markRequestCompleted(bool isFailed)
        {
            Interlocked.Decrement(ref _runningRequests);
            Interlocked.Increment(ref _completedRequests);
            if (isFailed)
            {
                Interlocked.Increment(ref _failedRequests);
            }
            else
            {
                Interlocked.Increment(ref _succeededRequests);
            }
        }

        public long getCompletedRequests()
        {
            return this._completedRequests;
        }

        public long getRunningRequests()
        {
            return this._runningRequests;
        }

        public long getFailedRequests()
        {
            return this._failedRequests;
        }

        public long getSucceededRequests()
        {
            return this._succeededRequests;
        }
    }

    [ToString]
    public class DbState
    {
        public string name { get; set; }
        public string status { get; set; }
    }

    [ToString]
    public class StateQueryResult
    {
        public AppState appState { get; set; }
        public List<DbState> DbStates { get; set; }

        public string message { get; set; } = "";
    }
}