using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace MapGeneration
{
    public abstract partial class AbstractMapGenerator : Node2D, IMapGenerator
    {
        public Data data;

        public override void _Draw()
        {
            data = GetNode<Data>("/root/Data");
        }

        public void Main()
        {
            Array<Vector2I> cells = GenerateBaseMap(data.MapSize);
            Dictionary<Vector2I, int> heightMap = GenerateHeightMap(cells);
            UpdateMap(heightMap);
        }

        public abstract Array<Vector2I> GenerateBaseMap(int mapSize);          
        public abstract Dictionary<Vector2I, int> GenerateHeightMap(Array<Vector2I> cells);
        public abstract Task UpdateMap(Dictionary<Vector2I, int> heightMap);
    }
}