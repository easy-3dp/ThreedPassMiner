using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace ThreedPassMiner
{
    internal static class Metadata
    {
        readonly static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        static bool hasValue;
        static byte[] difficultyBytes = new byte[32];
        static byte[] pre_hash = new byte[32];
        static byte[] best_hash = new byte[32];
        static byte[] pool_public = new byte[32];
        static string pool_id = string.Empty;
        static BigInteger difficulty;

        public static void Update(ReadOnlySpan<byte> bytes)
        {
            locker.EnterWriteLock();
          
            if (Args.isSolo)
            {
                bytes[ 0..32].CopyTo(difficultyBytes);
                bytes[32..64].CopyTo(pre_hash);
                bytes[64..96].CopyTo(best_hash);
                hasValue = difficultyBytes[0] != 0 || pre_hash[0] != 0 || best_hash[0] != 0;
            }
            else
            {
                bytes[ 0..32].CopyTo(pre_hash);
                bytes[32..64].CopyTo(best_hash);
                NetInfo.node_difficulty_2 = new BigInteger(bytes[64..96], true).ToString();
                bytes[96..128].CopyTo(difficultyBytes);
                bytes[128..160].CopyTo(pool_public);
                pool_id = Encoding.ASCII.GetString(bytes[160..209]);
                hasValue = difficultyBytes[0] != 0 || pre_hash[0] != 0 || best_hash[0] != 0 || pool_public[0] != 0;
            }

            difficulty = new BigInteger(difficultyBytes, true);

            if (hasValue)
            {
                NetInfo.node_difficulty = difficulty.ToString();
                NetInfo.node_pre_hash = Hex.Encode(pre_hash);
                NetInfo.node_best_hash = Hex.Encode(best_hash);
            }
            else
            {
                NetInfo.node_difficulty = null;
                NetInfo.node_pre_hash = null;
                NetInfo.node_best_hash = null;
            }

            locker.ExitWriteLock();
        }

        public static bool GetValues([NotNullWhen(true)] out BigInteger? difficulty, [NotNullWhen(true)] out byte[]? difficultyBytes, [NotNullWhen(true)] out byte[]? preBytes, [NotNullWhen(true)] out byte[]? bestBytes, [NotNullWhen(true)] out byte[]? publicBytes, [NotNullWhen(true)] out string? poolId)
        {
            try
            {
                locker.EnterReadLock();

                if (hasValue)
                {
                    difficulty = Metadata.difficulty;
                    difficultyBytes = Metadata.difficultyBytes;
                    preBytes = pre_hash;
                    bestBytes = best_hash;
                    publicBytes = pool_public;
                    poolId = pool_id;
                    return true;
                }
                else
                {
                    difficulty = null;
                    difficultyBytes = null;
                    preBytes = null;
                    bestBytes = null;
                    publicBytes = null;
                    poolId = null;
                    return false;
                }
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public static void Close()
        {
            locker.EnterWriteLock();
            hasValue = false;
            Array.Clear(difficultyBytes);
            Array.Clear(pre_hash);
            Array.Clear(best_hash);
            Array.Clear(pool_public);
            difficulty = 0;
            locker.ExitWriteLock();
        }

    }
}
