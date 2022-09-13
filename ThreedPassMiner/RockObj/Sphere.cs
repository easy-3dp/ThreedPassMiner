using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ThreedPassMiner.RockObj
{
    internal static class Sphere
    {
        public static (List<double[]> vertices, List<uint[]> indexes, List<double[]> normals) CreatSphere(double radius = 1.0, uint stacks = 32, uint slices = 32)
        {
            List<double[]> vertices = new List<double[]>();
            List<uint[]> indexes = new List<uint[]>();
            List<double[]> normals = new List<double[]>();

            // keeps track of the index of the next vertex that we create.
            uint index = 0;

            /*
             First of all, we create all the faces that are NOT adjacent to the
             bottom(0,-R,0) and top(0,+R,0) vertices of the sphere.

             (it's easier this way, because for the bottom and top vertices, we need to add triangle faces.
             But for the faces between, we need to add quad faces. )
             */

            // loop through the stacks.
            for (int i = 1; i < stacks; ++i)
            {
                double u = (double)i / stacks;
                double phi = u * Math.PI;

                uint stackBaseIndex = (uint)indexes.Count >> 1;

                // loop through the slices.
                for (int j = 0; j < slices; ++j)
                {
                    double v = (double)j / slices;
                    double theta = v * (Math.PI * 2);

                    var R = radius;
                    // use spherical coordinates to calculate the positions.
                    double x = Math.Cos(theta) * Math.Sin(phi);
                    double y = Math.Cos(phi);
                    double z = Math.Sin(theta) * Math.Sin(phi);

                    vertices.Add(new double[] { R * x, R * y, R * z });
                    normals.Add(new double[] { x, y, z });

                    if (i + 1 != stacks)
                    {
                        // for the last stack, we don't need to add faces.

                        uint i1, i2, i3, i4;

                        if (j + 1 == slices)
                        {
                            // for the last vertex in the slice, we need to wrap around to create the face.
                            i1 = index;
                            i2 = stackBaseIndex;
                            i3 = index + slices;
                            i4 = stackBaseIndex + slices;
                        }
                        else
                        {
                            // use the indices from the current slice, and indices from the next slice, to create the face.
                            i1 = index;
                            i2 = index + 1;
                            i3 = index + slices;
                            i4 = index + slices + 1;
                        }

                        // add quad face
                        indexes.Add(new uint[] { i1, i2, i3 });
                        indexes.Add(new uint[] { i4, i3, i2 });
                    }

                    index++;
                }
            }

            /*
             Next, we finish the sphere by adding the faces that are adjacent to the top and bottom vertices.
             */

            uint topIndex = index++;
            vertices.Add(new double[] { 0.0, radius, 0.0 });
            normals.Add(new double[] { 0, 1, 0 });

            uint bottomIndex = index++;
            vertices.Add(new double[] { 0, -radius, 0 });
            normals.Add(new double[] { 0, -1, 0 });

            for (uint i = 0; i < slices; ++i) {
                uint i1 = topIndex;
                uint i2 = i + 0;
                uint i3 = (i + 1) % slices;
                indexes.Add(new uint[] { i3, i2, i1 });

                i1 = bottomIndex;
                i2 = bottomIndex - 1 - slices + (i + 0);
                i3 = bottomIndex - 1 - slices + ((i + 1) % slices);
                indexes.Add(new uint[] { i1, i2, i3 });
            }

            return (vertices, indexes, normals);
        }
    }
}
