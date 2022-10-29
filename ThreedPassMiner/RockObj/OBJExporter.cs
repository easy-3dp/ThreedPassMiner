namespace ThreedPassMiner.RockObj
{
    internal static class OBJExporter
    {
        internal unsafe static string Parse(double* positions, uint* indices, double* normals)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("o\\n");

            for (int i = 0; i < 1806; i += 3)
            {
                sb.AppendFormat("v {0} {1} {2}\\n",
                    positions[i + 0].ToString("F2"), //精度指定しないとfloat精度の全体を吐かないので劣化してしまう。10進8桁必要
                    positions[i + 1].ToString("F2"),
                    positions[i + 2].ToString("F2"));
            }

            for (int i = 0; i < 1806; i += 3)
            {
                sb.AppendFormat("vn {0} {1} {2}\\n",
                    normals[i + 0].ToString("F4"),
                    normals[i + 1].ToString("F4"),
                    normals[i + 2].ToString("F4"));
            }

            for (var i = 0; i < 3600; i += 3)
            {
                // 1 based index.
                sb.AppendFormat("f {0}//{0} {1}//{1} {2}//{2}\\n",
                    indices[i + 0] + 1,
                    indices[i + 1] + 1,
                    indices[i + 2] + 1
                    );
            }

            return sb.ToString();
        }
    }
}
