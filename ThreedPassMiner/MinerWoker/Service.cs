using System.Collections.Concurrent;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace ThreedPassMiner
{
    internal static class Service
    {
        [DllImport("pass3d", CallingConvention = CallingConvention.Cdecl)]
        static extern int gethash(string data, byte[] best, IntPtr ptr_hash);

        readonly static BigInteger max_u256;
        readonly static ConcurrentBag<DoubleHash> poolDoubleHash = new ConcurrentBag<DoubleHash>();
        readonly static ConcurrentBag<Compute> poolCompute = new ConcurrentBag<Compute>();
        //readonly static ConcurrentBag<byte[]> poolBuffer = new ConcurrentBag<byte[]>();
        readonly static ConcurrentBag<IntPtr> poolPass3dBuffer = new ConcurrentBag<IntPtr>();

        static Service()
        {
            Span<byte> bytes = new byte[32]; bytes.Fill(0xFF);
            max_u256 = new BigInteger(bytes, true);
        }

        public static void Start()
        {
            for (int i = 0; i < Args.threads; i++)
            {
                new Thread(Loop) { IsBackground = true }.Start();
            }
        }

        static void Loop()
        {
            while (true)
            {
                string pre_obj = RockObj.RockSpawn.Spawn();

                BigInteger? difficulty;
                byte[]? difficultyBytes;
                byte[]? pre_hash;
                byte[]? best_hash;
                (difficulty, difficultyBytes, pre_hash, best_hash) = Metadata.Local.GetValues();

                if (difficulty != null && difficultyBytes != null && pre_hash != null && best_hash != null)
                {
                    IntPtr p3dBuffer;
                    if (!poolPass3dBuffer.TryTake(out p3dBuffer))
                        p3dBuffer = Marshal.AllocHGlobal(640);

                    var hashResult = -1;
                    try { hashResult = gethash(pre_obj, best_hash, p3dBuffer); } catch { }

                    if (hashResult == 0)
                    {
                        Statistics.AddErrorRecord();
                    }
                    else if (hashResult > 0)
                    {
                        byte[] hashBytes = new byte[hashResult * 64];
                        Marshal.Copy(p3dBuffer, hashBytes, 0, hashBytes.Length);
                        string hashes = Encoding.ASCII.GetString(hashBytes);

                        //byte[]? obj_hash;
                        //if (!poolBuffer.TryTake(out obj_hash))
                        //    obj_hash = new byte[32];
                        //Hex.Decode(hashes[0..64], obj_hash);

                        DoubleHash? dh;
                        if (!poolDoubleHash.TryTake(out dh))
                            dh = new DoubleHash();
                        dh.pre_hash = pre_hash;
                        dh.obj_hash = Hex.Decode(hashes[..64]);
                        var poscan_hash = dh.CalcHash();
                        poolDoubleHash.Add(dh);

                        Compute? compute;
                        if (!poolCompute.TryTake(out compute))
                            compute = new Compute();
                        compute.difficulty = difficultyBytes;
                        compute.pre_hash = pre_hash;
                        compute.poscan_hash = poscan_hash;
                        var work = new BigInteger(compute.Seal(), true, true);
                        poolCompute.Add(compute);

                        //poolBuffer.Add(obj_hash);

                        bool? success = null;

                        if (HashMeetsDifficulty(work, difficulty.Value))
                        {
                            if (Args.test)
                            {
                                success = true;
                            }
                            else
                            {
                                string str = $"{{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"poscan_pushMiningObject\",\"params\":[1,\"{pre_obj.Replace("\n", "\\n")}\",\"{Hex.Encode(poscan_hash)}\",\"{hashes}\"]}}";
                                using (var web = new WebClient())
                                {
                                    web.Headers["Content-Type"] = "application/json; charset=utf-8";
                                    try {
                                        var s = web.UploadString($"http://{Args.node_rpc_host}:{Args.node_rpc_port}", str.ToString());
                                            if (!s.Contains("error"))
                                                success = true;
                                            NetInfo.push_echo = $"[{DateTime.Now}] {s}";
                                    }
                                    catch (Exception e) { 
                                        Log.LogError(e);
                                            success = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            success = false;
                        }

                        if(success!=null)
                        Statistics.AddRecord(success.Value);
                    }

                    poolPass3dBuffer.Add(p3dBuffer);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        //static string? GetObjHashes(string filename)
        //{
        //    Process process = new Process();
        //    process.StartInfo.UseShellExecute = false;
        //    process.StartInfo.CreateNoWindow = true;
        //    process.StartInfo.RedirectStandardOutput = true;
        //    process.StartInfo.FileName = "pass3d.exe";
        //    process.StartInfo.Arguments = $"--algo grid2d --grid 8 --sect 66 --infile \"{filename}\"";
        //    process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        //    process.Start();

        //    var task = process.StandardOutput.ReadToEndAsync();
        //    var result = task.Wait(TimeSpan.FromSeconds(1));
        //    if (result && task.Status == TaskStatus.RanToCompletion)
        //    {
        //        var str = task.Result;
        //        var arr = str.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        //        if (arr.Length == 11)
        //            return arr[1].Trim('"');
        //    }
        //    return null;
        //}

        static bool HashMeetsDifficulty(BigInteger hash, BigInteger difficulty) // byte[32] byte[32]
        {
            var overflowing_mul = BigInteger.Multiply(hash, difficulty);
            bool overflowed = max_u256 < overflowing_mul;
            return !overflowed;
        }



    }
}
