using System;
using System.Numerics;
using ThreeJs4Net.Core;

namespace ThreedPassMiner.RockObj
{
    internal class Rock
    {
        //struct Values
        //{
        //    public ulong seed;
        //    public double noiseScale;
        //    public double noiseStrength;
        //    public double scrapeCount;
        //    public double scrapeMinDist;
        //    public double scrapeStrength;
        //    public double scrapeRadius;
        //    public double scale_0;
        //    public double scale_1;
        //    public double scale_2;
        //}

        List<uint>[]? adjacentVertices = null;
        Random random;
        public BufferGeometry geometry;

        public Rock(RockObj rockObj)
        {
            random = new Random(rockObj.seed);

            (List<double[]> positions, List<uint[]> indexes, List<double[]> normals) = Sphere.CreatSphere(stacks: 25, slices: 25);

            if (adjacentVertices == null)
            {
                // OPTIMIZATION: we are always using the same sphere as base for the rock,
                // so we only need to compute the adjacent positions once.
                adjacentVertices = Scrape.GetNeighbours(positions, indexes);
            }

            var scrapeIndices = new List<int>();

            for (int i = 0; i < rockObj.scrapeCount; ++i)
            {
                int attempts = 0;

                // find random position which is not too close to the other positions.
                while (true)
                {
                    int randIndex = (int)Math.Floor(positions.Count * random.NextDouble());
                    var p = new Vector3((float)positions[randIndex][0], (float)positions[randIndex][1], (float)positions[randIndex][2]);

                    bool tooClose = false;
                    // check that it is not too close to the other vertices.
                    for (int j = 0; j < scrapeIndices.Count; ++j)
                    {
                        var _ = positions[scrapeIndices[j]];
                        var q = new Vector3((float)_[0], (float)_[1], (float)_[2]);

                        if (Vector3.Distance(p, q) < rockObj.scrapeMinDist)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    ++attempts;

                    // if we have done too many attempts, we let it pass regardless.
                    // otherwise, we risk an endless loop.
                    if (tooClose && attempts < 100)
                    {
                        continue;
                    }
                    else
                    {
                        scrapeIndices.Add(randIndex);
                        break;
                    }
                }
            }

            // now we scrape at all the selected positions.
            for (int i = 0; i < scrapeIndices.Count; ++i)
            {
                Scrape.Main(scrapeIndices[i], positions, indexes, normals, adjacentVertices, (float)rockObj.scrapeStrength, (float)rockObj.scrapeRadius);
            }

            /**
             * Finally, we apply a Perlin noise to slightly distort the mesh and then scale the mesh.
             */
            var perlin = new Perlin(random.Next());
            for (int i = 0; i < positions.Count; ++i)
            {
                var p = positions[i];

                var noise = rockObj.meshNoiseStrength * perlin.Noise(rockObj.meshNoiseScale * p[0], rockObj.meshNoiseScale * p[1], rockObj.meshNoiseScale * p[2]);

                positions[i][0] += noise;
                positions[i][1] += noise;
                positions[i][2] += noise;

                positions[i][0] *= rockObj.scale[0];
                positions[i][1] *= rockObj.scale[1];
                positions[i][2] *= rockObj.scale[2];

                positions[i][0] = Math.Round((positions[i][0] + double.Epsilon) * 100) / 100;
                positions[i][1] = Math.Round((positions[i][1] + double.Epsilon) * 100) / 100;
                positions[i][2] = Math.Round((positions[i][2] + double.Epsilon) * 100) / 100;
            }

           

            var geometry = new BufferGeometry();
            geometry.SetIndex(new BufferAttribute<uint>(Flat(indexes), 3));
            geometry.SetAttribute("position", new BufferAttribute<float>(Flat(positions), 3));
            geometry.ComputeVertexNormals();

            this.geometry = geometry;
        }

        T[] Flat<T>(List<T[]> date)
        {
            T[] flat = new T[date.Count * 3];
            for (int i = 0; i < date.Count; i++)
            {
                flat[i*3+0] = date[i][0];
                flat[i*3+1] = date[i][1];
                flat[i*3+2] = date[i][2];
            }
            return flat;
        }

        float[] Flat(List<double[]> date)
        {
            float[] flat = new float[date.Count * 3];
            for (int i = 0; i < date.Count; i++)
            {
                flat[i * 3 + 0] = (float)date[i][0];
                flat[i * 3 + 1] = (float)date[i][1];
                flat[i * 3 + 2] = (float)date[i][2];
            }
            return flat;
        }

    }
}
