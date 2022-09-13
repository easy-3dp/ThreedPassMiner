using ThreeJs4Net.Math;

namespace ThreedPassMiner.RockObj
{
    internal static class Scrape
    {
        public static List<uint>[] GetNeighbours(List<double[]> positions, List<uint[]> cells)
        {
            /*
             adjacentVertices[i] contains a set containing all the indices of the neighbours of the vertex with
             index i.
             A set is used because it makes the algorithm more convenient.
             */
            var adjacentVertices = new List<uint>[positions.Count];

            // go through all faces.
            for (int iCell = 0; iCell < cells.Count; ++iCell)
            {
                uint[] cellPositions = cells[iCell];

                Func<int, int> wrap = (i) =>
                {
                    if (i < 0)
                    {
                        return cellPositions.Length + i;
                    }
                    else
                    {
                        return i % cellPositions.Length;
                    }
                };

                // go through all the points of the face.
                for (int iPosition = 0; iPosition < cellPositions.Length; ++iPosition)
                {
                    // the neighbours of this points are the previous and next points(in the array)
                    var cur = cellPositions[wrap(iPosition + 0)];
                    var prev = cellPositions[wrap(iPosition - 1)];
                    var next = cellPositions[wrap(iPosition + 1)];

                    // create set on the fly if necessary.
                    if (adjacentVertices[cur] == null)
                    {
                        adjacentVertices[cur] = new List<uint>();
                    }

                    // add adjacent vertices.
                    adjacentVertices[cur].Add(prev);
                    adjacentVertices[cur].Add(next);
                }
            }

            return adjacentVertices;
        }

        /*
        Projects the point `p` onto the plane defined by the normal `n` and the point `r0`
         */
        public static Vector3 Project(Vector3 n, Vector3 r0, Vector3 p)
        {
            // For an explanation of the math, see http://math.stackexchange.com/a/100766

            var t = n.Dot(Vector3.SubtractVectors(r0, p)) / n.Dot(n);

            var projectedP = p;
            projectedP = projectedP.AddScaledVector(n, t);

            return projectedP;
        }

        // scrape at vertex with index `positionIndex`.
        public static void Main(
            int positionIndex,
            List<double[]> positions,
            List<uint[]> cells,
            List<double[]> normals,
            List<uint>[] adjacentVertices,
            float strength, float radius)
        {

            bool[] traversed = new bool[positions.Count];
            for (int i = 0; i < positions.Count; ++i)
            {
                traversed[i] = false;
            }

            var centerPosition = positions[positionIndex];

            // to scrape, we simply project all vertices that are close to `centerPosition`
            // onto a plane. The equation of this plane is given by dot(n, r-r0) = 0,
            // where n is the plane normal, r0 is a point on the plane(in our case we set this to be the projected center),
            // and r is some arbitrary point on the plane.
            var n = new Vector3(
                (float)normals[positionIndex][0],
                (float)normals[positionIndex][1],
                (float)normals[positionIndex][2]
            );

            var r0 = new Vector3(
                (float)centerPosition[0],
                (float)centerPosition[1],
                (float)centerPosition[2]
            );


            r0 = r0.AddScaledVector(n, -strength);

            var stack = new Stack<int>();
            stack.Push(positionIndex);

            /*
             We use a simple flood-fill algorithm to make sure that we scrape all vertices around the center.
             This will be fast, since all vertices have knowledge about their neighbours.
             */
            while (stack.Count > 0)
            {
                var topIndex = stack.Pop();

                if (traversed[topIndex]) continue; // already traversed; look at next element in stack.
                traversed[topIndex] = true;

                var topPosition = positions[topIndex];
                // project onto plane.
                var p = new double[] {
                    topPosition[0],
                    topPosition[1],
                    topPosition[2],
                };

                Vector3 projectedP = Project(
                    new Vector3((float)n[0], (float)n[1], (float)n[2]),
                    new Vector3((float)r0[0], (float)r0[1], (float)r0[2]),
                    new Vector3((float)p[0], (float)p[1], (float)p[2])
                    );

                if (projectedP.DistanceToSquared(r0) < radius)
                {
                    positions[topIndex] = new double[] { projectedP[0], projectedP[1], projectedP[2] };
                    normals[topIndex] = new double[] { n[0], n[1], n[2] };
                }
                else
                {
                    continue;
                }

                var neighbourIndices = adjacentVertices[topIndex];
                for (int i = 0; i < neighbourIndices.Count; ++i)
                {
                    stack.Push((int)neighbourIndices[i]);
                }
            }
        }
    }
}
