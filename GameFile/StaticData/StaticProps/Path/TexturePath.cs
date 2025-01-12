public static class TexturePath
{
    // 图片路径
    // 默认
    public static string GetDefaultTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/default.png";

    // 特征点
    public static string GetFeaturePointTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/FeaturePoint.png";

    // 空气
    public static string GetAirGroundTexturePath(string packName) => null;
    public static string GetAirFloorTexturePath(string packName) => null;
    
    // 泥土
    public static string GetEarthGroundTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/earth_ground.png";
    public static string GetEarthFloorTexturePath(string packName) => null;

    // 水
    public static string GetWaterGroundTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/water_ground.png";
    public static string GetWaterFloorTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/water_floor.png";

    // 草
    public static string GetGrassGroundTexturePath(string packName) => null;
    public static string GetGrassFloorTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/grass_floor.png";

    // 木头
    public static string GetWoodGroundTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/wood_ground.png";
    public static string GetWoodFloorTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/wood_floor.png";

    // 石头
    public static string GetStoneGroundTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/stone_ground.png";
    public static string GetStoneFloorTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/stone_floor.png";
    
    // 沙子
    public static string GetSandGroundTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/sand_ground.png";
    public static string GetSandFloorTexturePath(string packName) => $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/sand_floor.png";

    // 测试瓦片
    public static string testGroundTexturePath => $"res://制作素材包/美术素材/场景美术/地图/Act_pack/png/TestTile.png";
    public static string testFloorTexturePath => $"res://制作素材包/美术素材/场景美术/地图/Act_pack/png/TestTile.png";
}