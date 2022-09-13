using SHA3.Net;

namespace ThreedPassMiner
{
    internal class Compute
    {
        public byte[] difficulty; //32
        public byte[] pre_hash;   //32
        public byte[] poscan_hash;//32

        readonly byte[] encode = new byte[96];
        readonly Sha3 sha3 = Sha3.Sha3256();

        public byte[] Encode()
        {
            Array.Copy(difficulty , 0, encode,  0, 32);
            Array.Copy(pre_hash   , 0, encode, 32, 32);
            Array.Copy(poscan_hash, 0, encode, 64, 32);
            return encode;
        }

        public byte[] Seal()
        {
            return sha3.ComputeHash(Encode());
        }

    }
}
