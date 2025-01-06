public class air : GameMaterial
{
    string texturePackName;
    public air(){} 

    public air(string TexturePackName)
    {
        texturePackName = TexturePackName;
    }

    // 必须实现的属性
    public override string Name => "air";
    public override string GroundTexturePath => TexturePath.GetAirGroundTexturePath(texturePackName);
    public override string FloorTexturePath => TexturePath.GetAirFloorTexturePath(texturePackName);
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
    private string texturePackName;
    public earth(string TexturePackName)
    {
        texturePackName = TexturePackName;
    }

    public override string Name => "erth";
    public override string GroundTexturePath => TexturePath.GetEarthGroundTexturePath(texturePackName);
    public override string FloorTexturePath => TexturePath.GetEarthFloorTexturePath(texturePackName);
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
    private string texturePackName;
    public water(){}
    public water(string TexturePackName)
    {
        texturePackName = TexturePackName;
    }

    // 必须实现的属性
    public override string Name => "water";
    public override string GroundTexturePath => TexturePath.GetWaterGroundTexturePath(texturePackName);
    public override string FloorTexturePath => TexturePath.GetWaterFloorTexturePath(texturePackName);
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
    private string texturePackName;
    public grass(string TexturePackName)
    {
        texturePackName = TexturePackName;
    }

    // 必须实现的属性
    public override string Name => "grass";
    public override string GroundTexturePath => TexturePath.GetGrassGroundTexturePath(texturePackName);
    public override string FloorTexturePath => TexturePath.GetGrassFloorTexturePath(texturePackName);
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
    private string texturePackName;
    public wood(string TexturePackName)
    {
        texturePackName = TexturePackName;
    }

    // 必须实现的属性
    public override string Name => "wood";
    public override string GroundTexturePath => TexturePath.GetWoodGroundTexturePath(texturePackName);
    public override string FloorTexturePath => TexturePath.GetWoodFloorTexturePath(texturePackName);
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
    string texturePackName;
    public stone(string TexturePackName)
    {
        texturePackName = TexturePackName;
    }

    // 必须实现的属性
    public override string Name => "stone";
    public override string GroundTexturePath => TexturePath.GetStoneGroundTexturePath(texturePackName);
    public override string FloorTexturePath => TexturePath.GetStoneFloorTexturePath(texturePackName);
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
    private string texturePackName;
    public sand(string TexturePackName)
    {
        texturePackName = TexturePackName;
    }
    // 必须实现的属性
    public override string Name => "sand";
    public override string GroundTexturePath => TexturePath.GetSandGroundTexturePath(texturePackName);
    public override string FloorTexturePath => TexturePath.GetSandFloorTexturePath(texturePackName);
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
