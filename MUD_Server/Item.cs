
namespace MUD_Server
{
    class Item
    {
        public int id;
        public string name;
        public byte count;

        public Item(int itemId, string itemName, byte stackCount = 1)
        {
            id = itemId;
            name = itemName;
            count = stackCount;
        }
    }
}
