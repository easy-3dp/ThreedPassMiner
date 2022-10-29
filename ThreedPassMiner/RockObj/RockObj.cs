using System.Runtime.InteropServices;

namespace ThreedPassMiner.RockObj
{
    public class RockObjParams
    {
        public double MESH_NOISE_SCALE_MIN = 0.5;
        public double MESH_NOISE_SCALE_MAX = 5.0;
        public double MESH_NOISE_SCALE_VARY = 0.1;

        public double MESH_NOISE_STRENGTH_MIN = 0.0;
        public double MESH_NOISE_STRENGTH_MAX = 0.5;
        public double MESH_NOISE_STRENGTH_VARY = 0.05;

        public int SCRAPE_COUNT_MIN = 0;
        public int SCRAPE_COUNT_MAX = 15;
        public int SCRAPE_COUNT_VARY = 2;

        public double SCRAPE_MIN_DIST_MIN = 0.1;
        public double SCRAPE_MIN_DIST_MAX = 1.0;
        public double SCRAPE_MIN_DIST_VARY = 0.05;

        public double SCRAPE_STRENGTH_MIN = 0.1;
        public double SCRAPE_STRENGTH_MAX = 0.6;
        public double SCRAPE_STRENGTH_VARY = 0.05;

        public double SCRAPE_RADIUS_MIN = 0.1;
        public double SCRAPE_RADIUS_MAX = 0.5;
        public double SCRAPE_RADIUS_VARY = 0.05;

        public double SCALE_MIN = +1.0;
        public double SCALE_MAX = +2.0;
        public double SCALE_VARY = +0.1;

        public double meshNoiseScale = 2.0;
        public double meshNoiseStrength = 0.2;
        public int scrapeCount = 7;
        public double scrapeMinDist = 0.8;
        public double scrapeStrength = 0.2;
        public double scrapeRadius = 0.3;
        public double[] scale = new double[] { 1, 1, 2 };
        public double varyStrength = 1.0;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RockObj
    {
        public double meshNoiseScale = 2.0;
        public double meshNoiseStrength = 0.2;
        public int scrapeCount = 7;
        public double scrapeMinDist = 0.8;
        public double scrapeStrength = 0.2;
        public double scrapeRadius = 0.3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] scale = new double[] { 1, 1, 2 };
        public double varyStrength = 1.0;

        public RockObj()
        {
        }

        double GetRandomValue(double min, double max)
        {
            return Random.Shared.NextDouble() * (max - min) + min;
        }

        public double VaryParameter(double param, double variance, double min, double max)
        {
            param += GetRandomValue( -variance * this.varyStrength, +variance * this.varyStrength);
            if (param > max) param = max;
            if (param < min) param = min;
            return param;
        }
        public int VaryParameter(int param, int variance, int min, int max)
        {
            param += (int)GetRandomValue(-variance * this.varyStrength, +variance * this.varyStrength);
            if (param > max) param = max;
            if (param < min) param = min;
            return param;
        }

        public void VaryMesh(RockObjParams p)
        {
            varyStrength = p.varyStrength;

            meshNoiseScale     = VaryParameter(p.meshNoiseScale,     p.MESH_NOISE_SCALE_VARY,      p.MESH_NOISE_SCALE_MIN,       p.MESH_NOISE_SCALE_MAX);
            meshNoiseStrength  = VaryParameter(p.meshNoiseStrength,  p.MESH_NOISE_STRENGTH_VARY,   p.MESH_NOISE_STRENGTH_MIN,    p.MESH_NOISE_STRENGTH_MAX);
            scrapeCount        = VaryParameter(p.scrapeCount,        p.SCRAPE_COUNT_VARY,          p.SCRAPE_COUNT_MIN,           p.SCRAPE_COUNT_MAX);
            scrapeMinDist      = VaryParameter(p.scrapeMinDist,      p.SCRAPE_MIN_DIST_VARY,       p.SCRAPE_MIN_DIST_MIN,        p.SCRAPE_MIN_DIST_MAX);
            scrapeStrength     = VaryParameter(p.scrapeStrength,     p.SCRAPE_STRENGTH_VARY,       p.SCRAPE_STRENGTH_MIN,        p.SCRAPE_STRENGTH_MAX);
            scrapeRadius       = VaryParameter(p.scrapeRadius,       p.SCRAPE_RADIUS_VARY,         p.SCRAPE_RADIUS_MIN,          p.SCRAPE_RADIUS_MAX);

            scale[0] = VaryParameter(p.scale[0], p.SCALE_VARY, p.SCALE_MIN, p.SCALE_MAX);
            scale[1] = VaryParameter(p.scale[1], p.SCALE_VARY, p.SCALE_MIN, p.SCALE_MAX);
            scale[2] = VaryParameter(p.scale[2], p.SCALE_VARY, p.SCALE_MIN, p.SCALE_MAX);
        }

    }
}
