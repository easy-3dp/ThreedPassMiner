using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Org.BouncyCastle.Utilities;

namespace ThreedPassMiner
{
    internal static class Service
    {
        [DllImport("pass3d", CallingConvention = CallingConvention.Cdecl)]
        static extern int p3d_process(byte[] pre, IntPtr hash, IntPtr str, ref int len);

        [DllImport("pass3d", CallingConvention = CallingConvention.Cdecl)]
        static extern int sign(byte[] message, int message_len, byte[] pub_key, byte[] hash, byte[] key, byte[] out_encrypted, ref int out_encrypted_len, byte[] out_sign);

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
            IntPtr objstr = Marshal.AllocHGlobal(65535);
            int len = 0;
            Compute compute = new Compute();
            DoubleHash dh = new DoubleHash(); dh.obj_hash = new byte[32];
            byte[] poscan_hash = new byte[32];
            byte[] workBytes = new byte[32];

            byte[] encryptedBytes = new byte[1048576];
            byte[] signBytes = new byte[64];
            int encryptedLen = 0;

            while (true)
            {
                if (Metadata.GetValues(out BigInteger? difficulty, out byte[]? difficultyBytes, out byte[]? pre_hash, out byte[]? best_hash, out byte[]? pub_key, out string? pool_id))
                {
                    if (0 == p3d_process(best_hash, hash, objstr, ref len))
                    {
                        Statistics.AddRecord(true);
                    }
                    else
                    {
                        string hashes;
                        unsafe { hashes = Encoding.ASCII.GetString((byte*)hash, 64); }

                        dh.pre_hash = pre_hash;
                        Hex.Decode(hashes, dh.obj_hash);
                        dh.CalcHash(poscan_hash);

                        compute.difficulty = difficultyBytes;
                        compute.pre_hash = pre_hash;
                        compute.poscan_hash = poscan_hash;
                        compute.Seal(workBytes);
                        var work = new BigInteger(workBytes, true, true);

                        Statistics.AddRecord(false);

                        if (HashMeetsDifficulty(work, difficulty.Value))
                        {
                            Statistics.AddRecordFound();

                            if (!Args.test)
                            {
                                if (Args.isSolo)
                                {
                                    string pre_obj;
                                    unsafe { pre_obj = Encoding.ASCII.GetString((byte*)objstr, len); }
                                    string str = $"{{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"poscan_pushMiningObject\",\"params\":[\"{pre_obj.Replace("\n", "\\n")}\", \"{hashes}\"]}}";
                                    NodeServer.Push(str);
                                }
                                else
                                {

                                    StringBuilder sb = new StringBuilder();
                                    unsafe
                                    {
                                        for (int i = 0; i < len; i++)
                                        {
                                            sb.Append(((byte*)objstr)[i]).Append(',');
                                        }
                                        sb.Remove(sb.Length - 1, 1);
                                    }

                                    string message = $"{{\"pool_id\":\"{pool_id}\",\"member_id\":\"{Args.member_id}\",\"pre_hash\":\"0x{Hex.Encode(pre_hash)}\",\"parent_hash\":\"0x{Hex.Encode(best_hash)}\",\"algo\":\"{Args.algorithm}\",\"dfclty\":\"0x{Hex.Encode(difficulty.Value.ToByteArray(true, true)).TrimStart('0')}\",\"hash\":\"0x{hashes}\",\"obj_id\":1,\"obj\":[{sb.ToString()}]}}";

                                    int result = sign(Encoding.ASCII.GetBytes(message), message.Length, pub_key, dh.obj_hash, Args.key, encryptedBytes, ref encryptedLen, signBytes);
                                    if (result == -1)
                                    {
                                        Console.WriteLine("invalid private key !!!");
                                        Environment.Exit(0);
                                    }

                                    sb.Clear();
                                    for (int i = 0; i < encryptedLen; i++)
                                    {
                                        sb.Append(encryptedBytes[i]).Append(',');
                                    }
                                    sb.Remove(sb.Length - 1, 1);

                                    var payload = sb.ToString();

                                    sb.Clear();
                                    for (int i = 0; i < signBytes.Length; i++)
                                    {
                                        sb.AppendFormat("{0:x2}", signBytes[i]);
                                    }

                                    string str = $"{{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"poscan_pushMiningObjectToPool\",\"params\":[[{payload}], \"{Args.member_id}\", \"{sb}\"]}}";

                                    NodeServer.Push(str);
                                }
                            }
                        }

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
