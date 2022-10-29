namespace ThreedPassMiner
{
    internal static class Statistics
    {
        static readonly List<DateTime> successRecords = new List<DateTime>();
        static readonly List<DateTime> emptyRecords = new List<DateTime>();
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

        public static void AddEmptyRecord()
        {
            lock (locker)
            {
                var now = DateTime.Now;
                totalRecords.Add(now);
                emptyRecords.Add(now);
            }
        }

        public static int GetTotalRecord(DateTime dateTime)
        {
            lock (locker)
            {
                int total = totalRecords.Count(x => x > dateTime);
                return total;
            }
        }

        public static (int success, int total, int error) GetAllRecord(DateTime dateTime)
        {
            lock (locker)
            {
                int total   = totalRecords  .Count(x => x > dateTime);
                int success = successRecords.Count(x => x > dateTime);
                int error   = emptyRecords  .Count(x => x > dateTime);
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
                if (totalRecords.Count > 0)
                {
                    var dateTime = DateTime.Now.Subtract(time);
                    var dateTime2 = DateTime.Now.Subtract(totalRecords[0]);
                    var sec = Math.Min(dateTime2.TotalSeconds, time.TotalSeconds);
                    return totalRecords.Count(x => x > dateTime) / sec;
                }
                return 0;
            }
        }

        public static void ClearToolong()
        {
            lock (locker)
            {
                var dateTime = DateTime.Now.AddHours(1);
                totalRecords.RemoveAll(x => x > dateTime);
                successRecords.RemoveAll(x => x > dateTime);
            }
        }

    }
}
