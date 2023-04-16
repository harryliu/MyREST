namespace MyREST
{
    public class AppState
    {
        private long runningRequests = 0;
        private long completedRequests = 0;
        private long failedRequests = 0;
        private long succeededRequests = 0;
        public DateTime startTime { get; set; }

        public void markNewRequest()
        {
            Interlocked.Increment(ref runningRequests);
        }

        public void markRequestCompleted(bool isFailed)
        {
            Interlocked.Decrement(ref runningRequests);
            Interlocked.Increment(ref completedRequests);
            if (isFailed)
            {
                Interlocked.Increment(ref failedRequests);
            }
            else
            {
                Interlocked.Increment(ref succeededRequests);
            }
        }

        public long getCompletedRequests()
        {
            return this.completedRequests;
        }

        public long getRunningRequests()
        {
            return this.runningRequests;
        }

        public long getFailedRequests()
        {
            return this.failedRequests;
        }

        public long getSucceededRequests()
        {
            return this.succeededRequests;
        }
    }
}