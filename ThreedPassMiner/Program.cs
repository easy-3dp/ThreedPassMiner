using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace ThreedPassMiner
{
    internal class Program
    {
        static DateTime startedDT;
        static string kernel = String.Empty;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Console.Title = "easy 3dp";
            ArgsParser(args);

            IntPtr ptr = Marshal.AllocHGlobal(128);
            int len = get_version(ptr);
            unsafe { kernel = Encoding.UTF8.GetString((byte*)ptr, len); }
            len = get_algorithm(ptr);
            unsafe { Args.algorithm = Encoding.UTF8.GetString((byte*)ptr, len); }
            Marshal.FreeHGlobal(ptr);

            NodeServer.Start();
            Service.Start();
            Statistics.Run();

            startedDT = DateTime.Now;
            while (true)
            {
                Thread.Sleep(Args.refresh_interval);
                Print();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.LogError(e.ExceptionObject as Exception);
        }

        static void ArgsParser(string[] args)
        {
            int solo = 0;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].Trim())
                {
                    default: throw new Exception("不支持的参数"+ args[i]);
                    case "--node-rpc-host": Args.node_rpc_host = args[++i]; break;
                    case "--node-rpc-port": Args.node_rpc_port = (int)uint.Parse(args[++i]); break;
                    case "--refresh-interval": Args.refresh_interval = (int)uint.Parse(args[++i]); break;
                    case "--threads"      : Args.threads = ushort.Parse(args[++i]); break;
                    case "--test":
                        var str = args[++i];
                        if (str.Contains(","))
                        {
                            str = str.Trim().TrimStart('[').TrimEnd(']');
                            var array = str.Split(",", StringSplitOptions.RemoveEmptyEntries);
                            if(array.Length !=32)
                                throw new Exception("数组长度要求32位");
                            Args.difficultyBytes = new byte[32];
                            for (int j = 0; j < 32; j++)
                                Args.difficultyBytes[j] = byte.Parse(array[j]);
                        }
                        else
                        {
                            Args.difficultyBytes = new byte[32]; 
                            var tmp = BigInteger.Parse(str).ToByteArray(true);
                            Array.Copy(tmp, Args.difficultyBytes, tmp.Length);
                        }
                        Args.test = true;
                        break;
                    case "--member-id": Args.member_id = args[++i]; ++solo; break;
                    case "--key": var keystr = args[++i]; if(keystr.StartsWith("0x")) Hex.Decode(keystr[2..], Args.key); else Hex.Decode(keystr, Args.key); ++solo; break;
                }
            }

            Args.isSolo = solo == 0;
            if (Args.test) Args.isSolo = true;
        }

        static void Print()
        {
            string str = $@"easy 3dp v1.2.0 kernel-{kernel}({Args.algorithm})    Running:{(DateTime.Now - startedDT):hh':'mm':'ss}

Server     : {NetInfo.nodeinfo ?? "not connected"}   {(Args.isSolo?"SOLO":"POOL")}   Ping {NetInfo.ping}ms
Difficulty : {NetInfo.node_difficulty}{(Args.isSolo?"": $"/{NetInfo.node_difficulty_2}")}
Pre_hash   : {NetInfo.node_pre_hash}
Best_hash  : {NetInfo.node_best_hash}
Model Speed: {TGMK(Statistics.GetRecordTotal())} models/s
Valid Speed: {TGMK(Statistics.GetRecordTotalNotEmpty())} models/s
Found      : {Statistics.GetRecordFound()} models
";
            Console.Clear();
            Console.WriteLine(str);
        }

        static string TGMK(double value)
        {
            return (value / 1_000).ToString("0.000") + "K";
        }


        [DllImport("pass3d", CallingConvention = CallingConvention.Cdecl)]
        static extern int get_version(IntPtr str);

        [DllImport("pass3d", CallingConvention = CallingConvention.Cdecl)]
        static extern int get_algorithm(IntPtr str);

    }
}