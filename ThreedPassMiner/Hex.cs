using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreedPassMiner
{
    internal static class Hex
    {
        public static void Decode(string str, byte[] result)
        {
            for (int i = 0; i < str.Length; i += 2)
            {
                result[i >> 1] = byte.Parse(str[i..(i + 2)], System.Globalization.NumberStyles.HexNumber);
            }
        }

        public static byte[] Decode(ReadOnlySpan<char> chars)
        {
            byte[] bytes = new byte[chars.Length >> 1];
            for (int i = 0; i < chars.Length; i += 2)
            {
                bytes[i >> 1] = byte.Parse(chars[i..(i+2)], System.Globalization.NumberStyles.HexNumber);
            }
            return bytes;
        }

        public static string Encode(Span<byte> bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i ++)
                sb.Append(bytes[i].ToString("x2"));
            return sb.ToString();
        }
    }
}
