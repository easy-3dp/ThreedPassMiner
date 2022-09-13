namespace ThreedPassMiner
{
    internal static class Statistics
    {
        static readonly List<DateTime> successRecords = new List<DateTime>();
        static readonly List<DateTime> errorRecords = new List<DateTime>();
        static readonly List<DateTime> totalRecords   = new List<DateTime>();
        static readonly object locker = new object();

        public static void AddRecord(bool success)
        {
            lock (locker)
            {
                var now = DateTime.Now;
                totalRecords.Add(now);
                if (success) successRecords.Add(now);
            }
        }

        public static void AddErrorRecord()
        {
            lock (locker)
            {
                var now = DateTime.Now;
                totalRecords.Add(now);
                errorRecords.Add(now);
            }
        }

        public static (int success, int total, int error) GetRecord(DateTime dateTime)
        {
            lock (locker)
            {
                int total = totalRecords.Count(x => x > dateTime);
                int success = successRecords.Count(x => x > dateTime);
                int error = errorRecords.Count(x => x > dateTime);
                return (success, total, error);
            }
        }

        public static int GetHashPerSec()
        {
            lock (locker)
            {
                var dateTime = DateTime.Now.AddSeconds(-1);
                return totalRecords.Count(x => x > dateTime);
            }
        }

        public static double GetHashAverage(TimeSpan time)
        {
            lock (locker)
            {
                var dateTime = DateTime.Now.Subtract(time);
                return totalRecords.Count(x => x > dateTime) / time.TotalSeconds;
            }
        }

        public static void ClearToolong()
        {
            lock (locker)
            {
                var dateTime = DateTime.Now.AddDays(30);
                totalRecords.RemoveAll(x => x > dateTime);
                successRecords.RemoveAll(x => x > dateTime);
            }
        }

    }
}
