
namespace MUD_Server
{
    class World
    {
        public int id;
        public Tile[,] tiles;

        public World(int worldId)
        {
            id = worldId;
            LoadWorld(id);
            //Console.WriteLine($"World '{id}' Done!");
        }

        private void LoadWorld(int id)
        {
            // offset from player position = 36 +/-
            //TODO: Load world file data and applie to the world instance
            //Console.Write($"Loading... ");
            int width = 1;
            int height = 1;
            tiles = new Tile[width, height];
        }
    }
}
