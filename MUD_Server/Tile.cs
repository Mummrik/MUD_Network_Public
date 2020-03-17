using System.Collections.Generic;
using System.Numerics;

namespace MUD_Server
{
    class Tile
    {
        Vector3 position;
        List<Item> itemList;

        public Tile(Vector3 tilePosition)
        {
            position = tilePosition;
            itemList = new List<Item>();
        }
    }
}
