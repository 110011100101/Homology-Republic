using System.Collections.ObjectModel;

public class cloud : air
{
    private string texturePackName;

    public cloud(string TexturePackName) : base(TexturePackName)
    {
        texturePackName = TexturePackName;
    }
    
    public int deltaGrayLevel; // 灰度等级

    public override string Name => "cloud";
}

public class ice : water
{
    private string texturePackName;

    public ice(string TexturePackName) : base(TexturePackName)
    {
        texturePackName = TexturePackName;
    }

    public override string Name => "ice";
    public int melitingSpeed_basic; // 基础融化速度
    public int meltingSpeed; // 实际的融化速度
}

public class snow : water
{
    private string texturePackName;

    public snow(string TexturePackName) : base(TexturePackName)
    {
        texturePackName = TexturePackName;
    }

    public override string Name => "snow";
}

public class TestTile : GameMaterial
{
    public override string Name => "TestTile";
    public override string GroundTexturePath => TexturePath.testGroundTexturePath;
    public override string FloorTexturePath => TexturePath.testFloorTexturePath;
    public override MaterialType Type => MaterialType.Solid; // 类型
    public override bool isFlammable => false; // 是否易燃
    public override bool isFlooraMaterial => false; // 是否可以用做地板材料
    public override float Hardness => float.MaxValue; // 硬度(巨硬)
    public override int MaxWaterCapacity => 0; // 最大容量（水）
    public override int MaxGasCapacity => 0; // 最大含气量（气体）
    public override int WaterPenetrationRate => 0; // 渗透率（水）
    public override int WaterCapacity {get; set;} // 实际含水率
    public override int GasCapacity {get; set;} // 实际含气率
}
