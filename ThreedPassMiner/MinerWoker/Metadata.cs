using System.Numerics;

namespace ThreedPassMiner
{
    internal static class Metadata
    {
        public class Meta
        {
            byte[]? difficultyBytes;
            byte[]? pre_hash;
            byte[]? best_hash;
            BigInteger? difficulty;

            readonly object locker = new object();

            public void Update(byte[]? bytes_difficulty, byte[]? bytes_pre_hash, byte[]? bytes_best_hash)
            {
                lock (locker)
                {
                    difficultyBytes = bytes_difficulty;
                    pre_hash = bytes_pre_hash;
                    best_hash = bytes_best_hash;
                    difficulty = new BigInteger(difficultyBytes, true);

                    if (bytes_difficulty != null)
                    {
                        NetInfo.node_difficulty = difficulty.ToString();
                        NetInfo.node_pre_hash   = Hex.Encode(bytes_pre_hash);
                        NetInfo.node_best_hash  = Hex.Encode(bytes_best_hash);
                    }
                    else
                    {
                        NetInfo.node_difficulty = null;
                        NetInfo.node_pre_hash   = null;
                        NetInfo.node_best_hash  = null;
                    }
                }
            }

            public (BigInteger? difficulty, byte[]? difficultyBytes, byte[]? pre_hash, byte[]? best_hash) GetValues()
            {
                lock (locker)
                {
                    return (difficulty, difficultyBytes, pre_hash, best_hash);
                }
            }
        }
        public static Meta Local { get; } = new();
    }
}
