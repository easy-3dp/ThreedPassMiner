using System.Collections.Concurrent;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using ThreedPassMiner.RockObj;

namespace ThreedPassMiner
{
    internal static class Service
    {
        [DllImport("pass3d", CallingConvention = CallingConvention.Cdecl)]
        static extern bool p3d_process(RockObj.RockObj rock_obj_params, byte[] pre, bool fast, IntPtr hash, IntPtr poisitons, IntPtr indices, IntPtr normals);

        readonly static BigInteger max_u256;

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
            IntPtr hash = Marshal.AllocHGlobal(64);
            IntPtr poisitons = Marshal.AllocHGlobal(14448);
            IntPtr indices = Marshal.AllocHGlobal(14400);
            IntPtr normals = Marshal.AllocHGlobal(14448);
            Compute compute = new Compute();
            DoubleHash dh = new DoubleHash(); dh.obj_hash = new byte[32];
            RockObj.RockObj rock = new RockObj.RockObj();

            while (true)
            {
                int id;
                BigInteger? difficulty;
                byte[]? difficultyBytes;
                byte[]? pre_hash;
                byte[]? best_hash;
                (difficulty, difficultyBytes, pre_hash, best_hash) = Metadata.Local.GetValues();

                if (difficulty != null && difficultyBytes != null && pre_hash != null && best_hash != null)
                {
                    rock.VaryMesh(RockSpawn.rockObjParams);

                    var hashResult = p3d_process(rock, best_hash[0..4], false, hash, poisitons, indices, normals);

                    if (!hashResult)
                    {
                        Statistics.AddEmptyRecord();
                    }
                    else
                    {
                        string hashes;
                        unsafe { hashes = Encoding.ASCII.GetString((byte*)hash, 64); }

                        dh.pre_hash = pre_hash;
                        Hex.Decode(hashes, dh.obj_hash);
                        var poscan_hash = dh.CalcHash();

                        compute.difficulty = difficultyBytes;
                        compute.pre_hash = pre_hash;
                        compute.poscan_hash = poscan_hash;
                        var work = new BigInteger(compute.Seal(), true, true);

                        bool? success = null;

                        if (HashMeetsDifficulty(work, difficulty.Value))
                        {
                            if (Args.test)
                            {
                                success = true;
                            }
                            else
                            {
                                string pre_obj;
                                unsafe { pre_obj = OBJExporter.Parse((double*)poisitons, (uint*)indices, (double*)normals); }
                                string str = $"{{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"poscan_pushMiningObject\",\"params\":[1,\"{pre_obj}\",\"{Hex.Encode(poscan_hash)}\",\"{hashes}\"]}}";
                                NodeServer.Push(str);
                                success = true;
                            }
                        }
                        else
                        {
                            success = false;
                        }

                        if (success != null)
                            Statistics.AddRecord(success.Value);
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        static bool HashMeetsDifficulty(BigInteger hash, BigInteger difficulty) // byte[32] byte[32]
        {
            var overflowing_mul = BigInteger.Multiply(hash, difficulty);
            bool overflowed = max_u256 < overflowing_mul;
            return !overflowed;
        }

    }
}
