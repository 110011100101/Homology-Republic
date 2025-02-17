using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace MapGeneration
{
    public interface IMapGenerator : IHeightMapGenerator, IBaseMapGenerator, IUpdateMap;

    public interface IBaseMapGenerator
    {
        // return a array of cell's position.
        Array<Vector2I> GenerateBaseMap(int mapSize);
    }

    public interface IHeightMapGenerator
    {
        // return a dictionary of cell's height.
        Dictionary<Vector2I, int> GenerateHeightMap(Array<Vector2I> cells);
    }

    public interface IUpdateMap
    {
        // return a task. It will give you a chance to use await.
        Task UpdateMap(Dictionary<Vector2I, int> heightMap);
    }
}