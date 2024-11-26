public class cloud : air
{
    public int deltaGrayLevel; // 灰度等级

    public override string strName => "cloud";
}

public class ice : water
{
    public override string strName => "ice";
    public int melitingSpeed_basic; // 基础融化速度
    public int meltingSpeed; // 实际的融化速度
}

public class snow : water
{
    public override string strName => "snow";
}

public class TestTile : GameMaterial
{
    public override string strName => "TestTile";
    public override string strGroundTexturePath => TexturePath.testGroundTexturePath;
    public override string strFloorTexturePath => TexturePath.testFloorTexturePath;
    public override MaterialType Type => MaterialType.Solid; // 类型
    public override bool isFlammable => false; // 是否易燃
    public override bool isFlooraMaterial => false; // 是否可以用做地板材料
    public override float fHardness => float.MaxValue; // 硬度(巨硬)
    public override int intMaxWaterCapacity => 0; // 最大容量（水）
    public override int intMaxGasCapacity => 0; // 最大含气量（气体）
    public override int intPenetrationRateWater => 0; // 渗透率（水）
    public override int intWaterCapacity {get; set;} // 实际含水率
    public override int intGasCapacity {get; set;} // 实际含气率
}
