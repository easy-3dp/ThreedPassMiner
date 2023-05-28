namespace ThreedPassMiner
{
    internal class Args
    {
        public static string node_rpc_host = "127.0.0.1"; // proxy -> node
        public static int node_rpc_port = 9933;      // proxy -> node

        public static bool test = false;
        public static byte[]? difficultyBytes = null;

        public static int refresh_interval = 1000;
        public static ushort threads = (ushort)Environment.ProcessorCount;

        public static bool isSolo = false;
        public static string member_id = string.Empty;
        public static byte[] key = new byte[32];

        public static string algorithm = string.Empty;
    }
}
