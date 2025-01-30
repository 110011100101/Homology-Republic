using System;
using System.Collections.Generic;
using Godot;

public static class TexturePath
{
    public static string GetTexturePath(string packName, EnumMaterial textureName, EnumTextureType textureType)
    {
        GD.Print($"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/{textureName}_{textureType}.png");
        return $"res://GameFile/StaticData/GameAssets/Texture/{packName}/tiles/{textureName}_{textureType}.png";
    } 
    // 测试瓦片
    public static string testGroundTexturePath => $"res://制作素材包/美术素材/场景美术/地图/Act_pack/png/TestTile.png";
    public static string testFloorTexturePath => $"res://制作素材包/美术素材/场景美术/地图/Act_pack/png/TestTile.png";

    public static List<string> GetAllPath(string packName)
    {
        List<string> paths = new List<string>();

         // 遍历 EnumTexture 枚举
        foreach (EnumMaterial texture in Enum.GetValues(typeof(EnumMaterial)))
        {
            GetTexturePath(packName, texture, EnumTextureType.Ground);
            GetTexturePath(packName, texture, EnumTextureType.Floor);
        }
        
        return paths;
    }
}