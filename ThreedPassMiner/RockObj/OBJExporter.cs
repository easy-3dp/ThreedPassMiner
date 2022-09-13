using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ThreeJs4Net.Core;
using ThreeJs4Net.Scenes;

namespace ThreedPassMiner.RockObj
{
    internal static class OBJExporter
    {
        internal static string Parse(BufferGeometry geometry)
        {
            var positions = (geometry.Attributes["position"] as BufferAttribute<float>).Array;
            var normals = (geometry.Attributes["normal"] as BufferAttribute<float>).Array;
            var indices = geometry.Index.Array;

            var sb = new System.Text.StringBuilder();
            sb.Append("o\n");

            for (int i = 0; i < positions.Length; i+=3)
            {
                sb.AppendFormat("v {0} {1} {2}\n",
                    positions[i+0].ToString("F2"), //精度指定しないとfloat精度の全体を吐かないので劣化してしまう。10進8桁必要
                    positions[i+1].ToString("F2"),
                    positions[i+2].ToString("F2"));
            }

            for (int i = 0; i < normals.Length; i+=3)
            {
                sb.AppendFormat("vn {0} {1} {2}\n",
                    normals[i+0].ToString(),
                    normals[i+1].ToString(),
                    normals[i+2].ToString());
            }

            for (var i = 0; i < indices.Length; i += 3)
            {
                // 1 based index.
                sb.AppendFormat("f {0}//{0} {1}//{1} {2}//{2}\n",
                    indices[i + 0] + 1,
                    indices[i + 1] + 1,
                    indices[i + 2] + 1
                    );
            }

            return sb.ToString();
        }
    }
}
