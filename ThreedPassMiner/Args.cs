using System.Numerics;

namespace ThreedPassMiner
{
    internal class Args
    {
        public static string node_rpc_host = "127.0.0.1"; // proxy -> node
        public static int node_rpc_port = 9933;      // proxy -> node

        public static bool test = false;
        public static byte[]? difficultyBytes = null;

        public static int refresh_interval = 1000;
        public static uint threads = (uint)Environment.ProcessorCount;

        public static int? restartSecs = null;

        public static bool dontTrack = false;
    }
}
