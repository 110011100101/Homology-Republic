public class Air : GameMaterial
{
    public override EnumMaterial Material => EnumMaterial.Air;
    public override EnumMaterialType Type => EnumMaterialType.Gas;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => false;
    public override bool isGroundMaterial => true;
    public override float Hardness => 0f;
    public override int MaxWaterCapacity => dynamicMaxWaterCapacity;    // 最大含水量
    public override int MaxGasCapacity => 8;                // 最大含气量
    public override int WaterPenetrationRate => 0;          // 渗水率
    public override int WaterCapacity { get; set; }         // 实际的含水量
    public override int GasCapacity                         // 有多少气就有多少水
    {
        get => GasCapacity;
        set
        {
            GasCapacity = value;
            dynamicMaxWaterCapacity = value;
        }
    }

    // 重写一个含水量属性替换原有的含水量，达到改变含水量上限的目的
    // 每次访问的时候会访问动态含水量,这样既符合了抽象类的定义又可以随便更改含水量了, 定义实际上就是只能访问
    protected int dynamicMaxWaterCapacity;              // 动态最大含水量

    // 扩展包:风
    private int windSpeed; // 风速
    private Quadrivial windDirection; // 风向
    private int windTrans = 8; // 化风阈值(达到这个值则为风)

    // 公开扩展包
    // 这样公开就不会被继承
    public int WindSpeed { get => windSpeed; set => windSpeed = value; }
    public Quadrivial WindDirection { get => windDirection; set => windDirection = value; }
    public int WindTrans => windTrans; // 只读
}

public class Soil : GameMaterial
{
    public override EnumMaterial Material => EnumMaterial.Soil;
    public override EnumMaterialType Type => EnumMaterialType.Solid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => true;
    public override bool isGroundMaterial => true;
    public override float Hardness => 3f;
    public override int MaxWaterCapacity => 5;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 3;
    public override int WaterCapacity { get; set; }
    public override int GasCapacity { get; set; }
}

public class Water : GameMaterial
{
    public override EnumMaterial Material => EnumMaterial.Water;
    public override EnumMaterialType Type => EnumMaterialType.Liquid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => true;
    public override bool isGroundMaterial => true;
    public override float Hardness => 0f;
    public override int MaxWaterCapacity => 5;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 0;
    public override int WaterCapacity { get; set; }
    public override int GasCapacity { get; set; }
}

public class Grass : GameMaterial
{
    public override EnumMaterial Material => EnumMaterial.Grass;
    public override EnumMaterialType Type => EnumMaterialType.Solid;
    public override bool isFlammable => true;
    public override bool isFlooraMaterial => true;
    public override bool isGroundMaterial => false;
    public override float Hardness => 5f;
    public override int MaxWaterCapacity => 0;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 300;
    public override int WaterCapacity { get; set; }
    public override int GasCapacity { get; set; }
}

public class Wood : GameMaterial
{
    public override EnumMaterial Material => EnumMaterial.Wood;
    public override EnumMaterialType Type => EnumMaterialType.Solid;
    public override bool isFlammable => true;
    public override bool isFlooraMaterial => true;
    public override bool isGroundMaterial => true;
    public override float Hardness => 10f;
    public override int MaxWaterCapacity => 0;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 500;
    public override int WaterCapacity { get; set; }
    public override int GasCapacity { get; set; }
}

public class Stone : GameMaterial
{
    public override EnumMaterial Material => EnumMaterial.Stone;
    public override EnumMaterialType Type => EnumMaterialType.Solid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => true;
    public override bool isGroundMaterial => true;
    public override float Hardness => 13f;
    public override int MaxWaterCapacity => 0;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 850;
    public override int WaterCapacity { get; set; }
    public override int GasCapacity { get; set; }
}

public class Sand : GameMaterial
{
    public override EnumMaterial Material => EnumMaterial.Sand;
    public override EnumMaterialType Type => EnumMaterialType.Solid;
    public override bool isFlammable => false;
    public override bool isFlooraMaterial => false;
    public override bool isGroundMaterial => true;
    public override float Hardness => 2f;
    public override int MaxWaterCapacity => 10;
    public override int MaxGasCapacity => 0;
    public override int WaterPenetrationRate => 2;
    public override int WaterCapacity { get; set; }
    public override int GasCapacity { get; set; }
}
