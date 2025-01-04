public class air : GameMaterial
{
    // 必须实现的属性
    public override string Name => "air";
    public override string GroundTexturePath => null;
    public override string FloorTexturePath => null;
    public override MaterialType Type => MaterialType.Gas;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => false;
    public override float Hardness => 0f;

    // 重写一个含水量属性替换原有的含水量，达到改变含水量上限的目的
    protected int dynamicMaxWaterCapacity; // 动态最大含水量
    public override int MaxWaterCapacity
    {
        get
        {
            return dynamicMaxWaterCapacity;
        }
    }
    public override int MaxGasCapacity => 8;
    public override int WaterPenetrationRate => 0;
    public override int WaterCapacity 
    {
        get
        {
            return WaterCapacity;
        } 
        set
        {
            WaterCapacity = value;
        }
    }
    public override int GasCapacity // 这里让最大含水量和此值保持一致
    {
        get
        {
            return GasCapacity;
        }
        set
        {
            GasCapacity = value;
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
    public override string Name => "erth";
    public override string GroundTexturePath => TexturePath.earthGroundTexturePath;
    public override string FloorTexturePath => TexturePath.earthFloorTexturePath;
    public override MaterialType Type => MaterialType.Solid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => true;
    public override float Hardness => 3f;
    public override int MaxWaterCapacity => 5;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 3;
    public override int WaterCapacity{get{return WaterCapacity;} set{WaterCapacity = value;}}
    public override int GasCapacity{get{return GasCapacity;} set{GasCapacity = value;}}
}

public class water : GameMaterial
{
    // 必须实现的属性
    public override string Name => "water";
    public override string GroundTexturePath => TexturePath.waterGroundTexturePath;
    public override string FloorTexturePath => null;
    public override MaterialType Type => MaterialType.Liquid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => false;
    public override float Hardness => 0f;
    public override int MaxWaterCapacity => 5;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 0;
    public override int WaterCapacity{get{return WaterCapacity;} set{WaterCapacity = value;}}
    public override int GasCapacity{get{return GasCapacity;} set{GasCapacity = value;}}
}

public class grass : GameMaterial
{
    // 必须实现的属性
    public override string Name => "grass";
    public override string GroundTexturePath => TexturePath.grassGroundTexturePath;
    public override string FloorTexturePath => TexturePath.grassFloorTexturePath;
    public override MaterialType Type => MaterialType.Solid;
    public override bool isFlammable => true;
    public override bool isFlooraMaterial => true;
    public override float Hardness => 5f;
    public override int MaxWaterCapacity => 0;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 300;
    public override int WaterCapacity{get{return WaterCapacity;} set{WaterCapacity = value;}}
    public override int GasCapacity{get{return GasCapacity;} set{GasCapacity = value;}}
}

public class wood : GameMaterial
{
    // 必须实现的属性
    public override string Name => "wood";
    public override string GroundTexturePath => TexturePath.woodGroundTexturePath;
    public override string FloorTexturePath => TexturePath.woodFloorTexturePath;
    public override MaterialType Type => MaterialType.Solid;
    public override bool isFlammable => true;
    public override bool isFlooraMaterial => true;
    public override float Hardness => 10f;
    public override int MaxWaterCapacity => 0;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 500;
    public override int WaterCapacity{get{return WaterCapacity;} set{WaterCapacity = value;}}
    public override int GasCapacity{get{return GasCapacity;} set{GasCapacity = value;}}
}

public class stone : GameMaterial
{
    // 必须实现的属性
    public override string Name => "stone";
    public override string GroundTexturePath => TexturePath.stoneGroundTexturePath;
    public override string FloorTexturePath => TexturePath.stoneFloorTexturePath;
    public override MaterialType Type => MaterialType.Solid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => true;
    public override float Hardness => 13f;
    public override int MaxWaterCapacity => 0;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 850;
    public override int WaterCapacity{get{return WaterCapacity;} set{WaterCapacity = value;}}
    public override int GasCapacity{get{return GasCapacity;} set{GasCapacity = value;}}
}

public class sand : GameMaterial
{
    // 必须实现的属性
    public override string Name => "sand";
    public override string GroundTexturePath => TexturePath.sandGroundTexturePath;
    public override string FloorTexturePath => null;
    public override MaterialType Type => MaterialType.Solid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => true;
    public override float Hardness => 2f;
    public override int MaxWaterCapacity => 10;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 2;
    public override int WaterCapacity{get{return WaterCapacity;} set{WaterCapacity = value;}}
    public override int GasCapacity{get{return GasCapacity;} set{GasCapacity = value;}}
}
