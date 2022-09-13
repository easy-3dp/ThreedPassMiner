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
