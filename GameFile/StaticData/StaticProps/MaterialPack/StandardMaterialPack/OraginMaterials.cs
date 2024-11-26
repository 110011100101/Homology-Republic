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
    public override string strName => "erth";
    public override string strGroundTexturePath => TexturePath.earthGroundTexturePath;
    public override string strFloorTexturePath => TexturePath.earthFloorTexturePath;
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
    public override string strGroundTexturePath => TexturePath.waterGroundTexturePath;
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
    public override string strGroundTexturePath => TexturePath.grassGroundTexturePath;
    public override string strFloorTexturePath => TexturePath.grassFloorTexturePath;
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
    public override string strGroundTexturePath => TexturePath.woodGroundTexturePath;
    public override string strFloorTexturePath => TexturePath.woodFloorTexturePath;
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
    public override string strGroundTexturePath => TexturePath.stoneGroundTexturePath;
    public override string strFloorTexturePath => TexturePath.stoneFloorTexturePath;
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
    public override string strGroundTexturePath => TexturePath.sandGroundTexturePath;
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
