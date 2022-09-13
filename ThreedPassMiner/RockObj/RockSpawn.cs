namespace ThreedPassMiner.RockObj
{
    internal static class RockSpawn
    {
        internal readonly static Random random = new Random();
        internal static RockObjParams rockObjParams = new RockObjParams();

        public static string Spawn()
        {
            return CreateObjFile(CreatRock());
        }

        public static Rock CreatRock() {
            var rock_obj = new RockObj();
            rock_obj.VaryMesh(rockObjParams);
            return new Rock(rock_obj);
        }

        public static string CreateObjFile(Rock rock)
        {
            return OBJExporter.Parse(rock.geometry);
        }
    }
}
