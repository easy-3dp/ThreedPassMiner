using SHA3.Net;

namespace ThreedPassMiner
{
    internal class DoubleHash
    {
        public byte[] pre_hash; //32
        public byte[] obj_hash; //32

        readonly byte[] encode = new byte[64];
        readonly Sha3 sha3 = Sha3.Sha3256();

        public byte[] CalcHash()
        {
            Array.Copy(pre_hash, 0, encode,  0, 32);
            Array.Copy(obj_hash, 0, encode, 32, 32);
            return sha3.ComputeHash(encode);
        }
    }
}
