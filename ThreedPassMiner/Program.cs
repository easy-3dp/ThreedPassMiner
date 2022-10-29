using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using ThreedPassMiner.RockObj;

namespace ThreedPassMiner
{
    internal class Program
    {
        static DateTime startedDT;
        static string kernel = String.Empty;

        static void Main(string[] args)
        {
#if DEBUG
            //args = new string[] { "--test", "--difficulty", "1", "--threads", "1" };
            //args = new string[] { "--node-rpc-host", "192.168.31.129", "--node-rpc-port", "9933", "--threads", "1" };
            //args = new string[] { "--node-rpc-port", "8000", "--threads", "12" , "--id" , "9999" };
            //args = new string[] { "--node-rpc-port", "8000", "--threads", "1" };
            //args = new string[] { "--test", "--difficulty", "100000", "--threads", "16" };
            args = new string[] { "--node-rpc-host", "43.136.176.220", "--node-rpc-port", "1111", "--threads", "1" };
#endif

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Console.Title = "easy 3dp";
            ArgsParser(args);

            string? str_RockObjParams = null;
            try { str_RockObjParams = File.ReadAllText("RockObjParams.json"); } catch { }
            if (str_RockObjParams != null)
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<ThreedPassMiner.RockObj.RockObjParams>(str_RockObjParams);
                if (obj != null) RockSpawn.rockObjParams = obj;
                else throw new Exception("RockObjParams.json 解析失败");
            }

            IntPtr ptr = Marshal.AllocHGlobal(128);
            int len = get_version(ptr, 0);
            unsafe { kernel = Encoding.UTF8.GetString((byte*)ptr, len); }
            Marshal.FreeHGlobal(ptr);

            NodeServer.Start();
            Service.Start();

            startedDT = DateTime.Now;
            while (true)
            {
                Thread.Sleep(Args.refresh_interval);
                Print();
                Statistics.ClearToolong();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.LogError(e.ExceptionObject as Exception);
        }

        static void ArgsParser(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].Trim())
                {
                    default: throw new Exception("不支持的参数"+ args[i]);
                    case "--node-rpc-host": Args.node_rpc_host = args[++i]; break;
                    case "--node-rpc-port": Args.node_rpc_port = (int)uint.Parse(args[++i]); break;
                    case "--test"         : Args.test          = true; break;
                    case "--refresh-interval": Args.refresh_interval = (int)uint.Parse(args[++i]); break;
                    case "--threads"      : Args.threads = uint.Parse(args[++i]); break;
                    case "--difficulty":
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
                    case "--restart-secs":  Args.restartSecs = (int)uint.Parse(args[++i]); break;
                    case "--restart-hours": Args.restartSecs = (int)uint.Parse(args[++i]) * 60 * 60; break;
                    case "--dont-track": Args.dontTrack = true; break;
                }
            }
        }

        static void Print()
        {
            string str = $@"easy 3dp v1.1.3-Kernel-{kernel}    Running:{(DateTime.Now - startedDT):hh':'mm':'ss}

Server     : {NetInfo.nodeinfo ?? "not connected"}   Ping {NetInfo.ping}ms
Difficulty : {NetInfo.node_difficulty}
Pre_hash   : {NetInfo.node_pre_hash}
Best_hash  : {NetInfo.node_best_hash}
Speed      : {TGMK(Statistics.GetTotalRecord(DateTime.Now.AddSeconds(-1)))}h/s
";
            Console.Clear();
            Console.WriteLine(str);
        }

        static string TGMK(double value)
        {
            if (value > 1_000_000_000_000) return (value / 1_000_000_000_000).ToString("0.00") + "T";
            else if (value > 1_000_000_000) return (value / 1_000_000_000).ToString("0.000") + "G";
            else if (value > 1_000_000) return (value / 1_000_000).ToString("0.000") + "M";
            else if (value > 1_000) return (value / 1_000).ToString("0.000") + "K";
            else return value.ToString("0.000");
        }


        [DllImport("pass3d", CallingConvention = CallingConvention.Cdecl)]
        static extern int get_version(IntPtr str, int function);

    }
}