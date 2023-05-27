using Org.BouncyCastle.Crypto.Digests;

namespace ThreedPassMiner
{
    internal class Compute
    {
        public byte[] difficulty; //32
        public byte[] pre_hash;   //32
        public byte[] poscan_hash;//32

        readonly byte[] encode = new byte[96];
        readonly Sha3Digest sha3 = new Sha3Digest(256);

        public void Seal(byte[] decode)
        {
            Array.Copy(difficulty , 0, encode,  0, 32);
            Array.Copy(pre_hash   , 0, encode, 32, 32);
            Array.Copy(poscan_hash, 0, encode, 64, 32);

            sha3.BlockUpdate(encode, 0, encode.Length);
            sha3.DoFinal(decode);
        }

    }
}
