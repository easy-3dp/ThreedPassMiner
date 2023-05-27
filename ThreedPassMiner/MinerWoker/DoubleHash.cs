using Org.BouncyCastle.Crypto.Digests;

namespace ThreedPassMiner
{
    internal class DoubleHash
    {
        public byte[] pre_hash; //32
        public byte[] obj_hash; //32

        readonly byte[] encode = new byte[64];
        readonly Sha3Digest sha3 = new Sha3Digest(256);

        public void CalcHash(byte[] decode)
        {
            Array.Copy(pre_hash, 0, encode,  0, 32);
            Array.Copy(obj_hash, 0, encode, 32, 32);

            sha3.BlockUpdate(encode, 0, encode.Length);
            sha3.DoFinal(decode);
        }
    }
}
