using System.Collections.Generic;
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

        static void Main(string[] args)
        {
#if DEBUG
            //args = new string[] { "--test", "--difficulty", "1", "--threads", "1" };
            args = new string[] { "--node-rpc-host", "192.168.31.129", "--node-rpc-port", "9933", "--threads", "1", "--main" };
            //args = new string[] { "--node-rpc-port", "8000", "--threads", "12" , "--id" , "9999" };
            //args = new string[] { "--node-rpc-port", "8000", "--threads", "1" };
            //args = new string[] { "--test", "--difficulty", "100000", "--threads", "16" };
#endif

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Console.Title = "3DP主网 多线程版 V1.1.2";
            ArgsParser(args);

            string? str_RockObjParams = null;
            try { str_RockObjParams = File.ReadAllText("RockObjParams.json"); } catch { }
            if (str_RockObjParams != null)
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<ThreedPassMiner.RockObj.RockObjParams>(str_RockObjParams);
                if (obj != null) RockSpawn.rockObjParams = obj;
                else throw new Exception("RockObjParams.json 解析失败");
            }

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
                    case "--node-rpc-port": Args.node_rpc_port = args[++i]; break;
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
                }
            }
        }

        static void Print()
        {
            int[,] nums = new int[8, 3];
            (nums[0, 0], nums[0, 1], nums[0, 2]) = Statistics.GetRecord(DateTime.Now.AddHours  (- 1));
            (nums[1, 0], nums[1, 1], nums[1, 2]) = Statistics.GetRecord(DateTime.Now.AddHours  (- 4));
            (nums[2, 0], nums[2, 1], nums[2, 2]) = Statistics.GetRecord(DateTime.Now.AddHours  (-12));
            (nums[3, 0], nums[3, 1], nums[3, 2]) = Statistics.GetRecord(DateTime.Now.AddDays   (- 1));
            (nums[4, 0], nums[4, 1], nums[4, 2]) = Statistics.GetRecord(DateTime.Now.AddDays   (- 2));
            (nums[5, 0], nums[5, 1], nums[5, 2]) = Statistics.GetRecord(DateTime.Now.AddDays   (- 7));
            (nums[6, 0], nums[6, 1], nums[6, 2]) = Statistics.GetRecord(DateTime.Now.AddDays   (-15));
            (nums[7, 0], nums[7, 1], nums[7, 2]) = Statistics.GetRecord(DateTime.Now.AddDays   (-30));


            string[,] strnums = new string[8, 3];
            for (int i = 0; i < 8; i++)
            {
                strnums[i, 0] = nums[i, 0].ToString();
                strnums[i, 1] = nums[i, 1].ToString();
                strnums[i, 2] = nums[i, 2].ToString();
            }

            int max1=0, max2=0, max3 = 0;
            for (int i = 0; i < 8; i++)
            {
                max1 = Math.Max(max1, strnums[i, 0].Length);
                max2 = Math.Max(max2, strnums[i, 1].Length);
                max3 = Math.Max(max3, strnums[i, 2].Length);
            }

            string[] str3 = {
                Statistics.GetHashAverage(TimeSpan.FromHours  ( 1)).ToString("0.00"),
                Statistics.GetHashAverage(TimeSpan.FromHours  ( 4)).ToString("0.00"),
                Statistics.GetHashAverage(TimeSpan.FromHours  (12)).ToString("0.00"),
                Statistics.GetHashAverage(TimeSpan.FromDays   ( 1)).ToString("0.00"),
                Statistics.GetHashAverage(TimeSpan.FromDays   ( 2)).ToString("0.00"),
                Statistics.GetHashAverage(TimeSpan.FromDays   ( 7)).ToString("0.00"),
                Statistics.GetHashAverage(TimeSpan.FromDays   (15)).ToString("0.00"),
                Statistics.GetHashAverage(TimeSpan.FromDays   (30)).ToString("0.00"),
            };
            int max4 = 0;
            for (int i = 0; i < str3.Length; i++)
            {
                max4 = Math.Max(max4, str3[i].Length);
            }

            string str = $@"节点：{(NetInfo.nodeinfo == null ? "无连接" :NetInfo.nodeinfo )}
      Difficulty: {NetInfo.node_difficulty}
      Pre_hash  : {NetInfo.node_pre_hash  }
      Best_hash : {NetInfo.node_best_hash }

运行时间：{DateTime.Now-startedDT}

算力：{Statistics.GetHashPerSec()}H/s        1分钟平均：{Statistics.GetHashAverage(TimeSpan.FromMinutes(1)):0.00}H/s

最近： 1时  提交：{strnums[0, 0].PadLeft(max1)} / Empty：{strnums[0, 2].PadLeft(max3)} / 总计：{strnums[0, 1].PadLeft(max2)} / 平均：{str3[0].PadLeft(max4)}H/s
       4时  提交：{strnums[1, 0].PadLeft(max1)} / Empty：{strnums[1, 2].PadLeft(max3)} / 总计：{strnums[1, 1].PadLeft(max2)} / 平均：{str3[1].PadLeft(max4)}H/s
      12时  提交：{strnums[2, 0].PadLeft(max1)} / Empty：{strnums[2, 2].PadLeft(max3)} / 总计：{strnums[2, 1].PadLeft(max2)} / 平均：{str3[2].PadLeft(max4)}H/s
       1天  提交：{strnums[3, 0].PadLeft(max1)} / Empty：{strnums[3, 2].PadLeft(max3)} / 总计：{strnums[3, 1].PadLeft(max2)} / 平均：{str3[3].PadLeft(max4)}H/s
       2天  提交：{strnums[4, 0].PadLeft(max1)} / Empty：{strnums[4, 2].PadLeft(max3)} / 总计：{strnums[4, 1].PadLeft(max2)} / 平均：{str3[4].PadLeft(max4)}H/s
       7天  提交：{strnums[5, 0].PadLeft(max1)} / Empty：{strnums[5, 2].PadLeft(max3)} / 总计：{strnums[5, 1].PadLeft(max2)} / 平均：{str3[5].PadLeft(max4)}H/s
      15天  提交：{strnums[6, 0].PadLeft(max1)} / Empty：{strnums[6, 2].PadLeft(max3)} / 总计：{strnums[6, 1].PadLeft(max2)} / 平均：{str3[6].PadLeft(max4)}H/s
      30天  提交：{strnums[7, 0].PadLeft(max1)} / Empty：{strnums[7, 2].PadLeft(max3)} / 总计：{strnums[7, 1].PadLeft(max2)} / 平均：{str3[7].PadLeft(max4)}H/s

节点返回消息：{NetInfo.push_echo}
";

            Console.Clear();
            Console.WriteLine(str);
        }

    }
}