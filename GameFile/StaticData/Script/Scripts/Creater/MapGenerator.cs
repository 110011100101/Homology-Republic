using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using MapGeneration;

public partial class MapGenerator : AbstractMapGenerator
{
    private FastNoiseLite noise;

    public MapGenerator()
    {
        noise = new FastNoiseLite();
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        noise.Frequency = 0.05f; // 控制噪声的频率
    }

    public override Array<Vector2I> GenerateBaseMap(int mapSize)
    {
        Array<Vector2I> cells = new Array<Vector2I>();

        for (int i = 0; i < mapSize; i++)
        for (int j = 0; j < mapSize; j++)
        {
            cells.Add(new Vector2I(i, j));
        }

        return cells;
    }

    public override Dictionary<Vector2I, int> GenerateHeightMap(Array<Vector2I> cells)
    {
        Dictionary<Vector2I, int> heightMap = new Dictionary<Vector2I, int>();

        foreach (var cell in cells)
        {
            float noiseValue = noise.GetNoise2D(cell.X, cell.Y);
            int height = (int)((noiseValue + 1) * 50); // 将噪声值映射到0-100的范围
            heightMap[cell] = height;
        }

        return heightMap;
    }

    public override Task UpdateMap(Dictionary<Vector2I, int> heightMap)
    {
        throw new NotImplementedException();
    }
}