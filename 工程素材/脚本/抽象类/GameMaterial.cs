public abstract class GameMaterial
{
    public abstract string strName {get;} // 名称
    public abstract string strGroundTexturePath {get;} // 方块纹理路径
    public abstract string strFloorTexturePath {get;} // 地板纹理路径
    public abstract MaterialType Type {get;} // 类型
    public abstract bool isFlammable {get;} // 是否易燃
    public abstract bool isFlooraMaterial {get;} // 是否可以用做地板材料
    public abstract float fHardness {get;} // 硬度
    public abstract int intMaxWaterCapacity {get;} // 最大容量（水）
    public abstract int intMaxGasCapacity {get;} // 最大含气量（气体）
    public abstract int intPenetrationRateWater {get;} // 渗透率（水）
    public abstract int intWaterCapacity {get; set;} // 实际含水率
    public abstract int intGasCapacity {get; set;} // 实际含气率
}

//
// 原始材质
//
public class air : GameMaterial
{
    // 必须实现的属性
    public override string strName => "air";
    public override string strGroundTexturePath => null;
    public override string strFloorTexturePath => null;
    public override MaterialType Type => MaterialType.Gas;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => false;
    public override float fHardness => 0f;

    // 重写一个含水量属性替换原有的含水量，达到改变含水量上限的目的
    protected int dynamicMaxWaterCapacity; // 动态最大含水量
    public override int intMaxWaterCapacity
    {
        get
        {
            return dynamicMaxWaterCapacity;
        }
    }
    public override int intMaxGasCapacity => 8;
    public override int intPenetrationRateWater => 0;
    public override int intWaterCapacity 
    {
        get
        {
            return intWaterCapacity;
        } 
        set
        {
            intWaterCapacity = value;
        }
    }
    public override int intGasCapacity // 这里让最大含水量和此值保持一致
    {
        get
        {
            return intGasCapacity;
        }
        set
        {
            intGasCapacity = value;
            dynamicMaxWaterCapacity = value;
        }
    }

    //
    // 扩展包
    //
    // 源
    // source: Wind
    private int windSpeed; // 风速
    private Quadrivial windDirection; // 风向
    private int windTrans = 8; // 化风阈值(达到这个值则为风)

    // 公开扩展包
    public int WindSpeed {get{return windSpeed;} set{windSpeed = value;}}
    public Quadrivial WindDirection {get{return windDirection;} set{windDirection = value;}}
    public int WindTrans {get{return windTrans;}} // 只读
}

public class earth : GameMaterial
{
    // 必须实现的属性
    public override string strName => "erth";
    public override string strGroundTexturePath => 图片路径.earthGroundTexturePath;
    public override string strFloorTexturePath => 图片路径.earthFloorTexturePath;
    public override MaterialType Type => MaterialType.Solid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => true;
    public override float fHardness => 3f;
    public override int intMaxWaterCapacity => 5;
    public override int intMaxGasCapacity => 0;
    public override int intPenetrationRateWater => 3;
    public override int intWaterCapacity{get{return intWaterCapacity;} set{intWaterCapacity = value;}}
    public override int intGasCapacity{get{return intGasCapacity;} set{intGasCapacity = value;}}
}

public class water : GameMaterial
{
    // 必须实现的属性
    public override string strName => "water";
    public override string strGroundTexturePath => 图片路径.waterGroundTexturePath;
    public override string strFloorTexturePath => null;
    public override MaterialType Type => MaterialType.Liquid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => false;
    public override float fHardness => 0f;
    public override int intMaxWaterCapacity => 5;
    public override int intMaxGasCapacity => 0;
    public override int intPenetrationRateWater => 0;
    public override int intWaterCapacity{get{return intWaterCapacity;} set{intWaterCapacity = value;}}
    public override int intGasCapacity{get{return intGasCapacity;} set{intGasCapacity = value;}}
}

public class grass : GameMaterial
{
    // 必须实现的属性
    public override string strName => "grass";
    public override string strGroundTexturePath => 图片路径.grassGroundTexturePath;
    public override string strFloorTexturePath => 图片路径.grassFloorTexturePath;
    public override MaterialType Type => MaterialType.Solid;
    public override bool isFlammable => true;
    public override bool isFlooraMaterial => true;
    public override float fHardness => 5f;
    public override int intMaxWaterCapacity => 0;
    public override int intMaxGasCapacity => 0;
    public override int intPenetrationRateWater => 300;
    public override int intWaterCapacity{get{return intWaterCapacity;} set{intWaterCapacity = value;}}
    public override int intGasCapacity{get{return intGasCapacity;} set{intGasCapacity = value;}}
}

public class wood : GameMaterial
{
    // 必须实现的属性
    public override string strName => "wood";
    public override string strGroundTexturePath => 图片路径.woodGroundTexturePath;
    public override string strFloorTexturePath => 图片路径.woodFloorTexturePath;
    public override MaterialType Type => MaterialType.Solid;
    public override bool isFlammable => true;
    public override bool isFlooraMaterial => true;
    public override float fHardness => 10f;
    public override int intMaxWaterCapacity => 0;
    public override int intMaxGasCapacity => 0;
    public override int intPenetrationRateWater => 500;
    public override int intWaterCapacity{get{return intWaterCapacity;} set{intWaterCapacity = value;}}
    public override int intGasCapacity{get{return intGasCapacity;} set{intGasCapacity = value;}}
}

public class stone : GameMaterial
{
    // 必须实现的属性
    public override string strName => "stone";
    public override string strGroundTexturePath => 图片路径.stoneGroundTexturePath;
    public override string strFloorTexturePath => 图片路径.stoneFloorTexturePath;
    public override MaterialType Type => MaterialType.Solid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => true;
    public override float fHardness => 13f;
    public override int intMaxWaterCapacity => 0;
    public override int intMaxGasCapacity => 0;
    public override int intPenetrationRateWater => 850;
    public override int intWaterCapacity{get{return intWaterCapacity;} set{intWaterCapacity = value;}}
    public override int intGasCapacity{get{return intGasCapacity;} set{intGasCapacity = value;}}
}

public class sand : GameMaterial
{
    // 必须实现的属性
    public override string strName => "sand";
    public override string strGroundTexturePath => 图片路径.sandGroundTexturePath;
    public override string strFloorTexturePath => null;
    public override MaterialType Type => MaterialType.Solid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => true;
    public override float fHardness => 2f;
    public override int intMaxWaterCapacity => 10;
    public override int intMaxGasCapacity => 0;
    public override int intPenetrationRateWater => 2;
    public override int intWaterCapacity{get{return intWaterCapacity;} set{intWaterCapacity = value;}}
    public override int intGasCapacity{get{return intGasCapacity;} set{intGasCapacity = value;}}
}

//
// 次级材质
//
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
    public override string strGroundTexturePath => 图片路径.testGroundTexturePath;
    public override string strFloorTexturePath => 图片路径.testFloorTexturePath;
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