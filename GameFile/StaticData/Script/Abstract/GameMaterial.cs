public abstract class GameMaterial
{
    public abstract EnumMaterial Material { get; }         // 材质
    public abstract EnumMaterialType Type {get;}         // 类型
    public abstract bool isFlammable {get;}              // 是否易燃
    public abstract bool isFlooraMaterial {get;}         // 是否可以用做地板材料
    public abstract bool isGroundMaterial {get;}         // 是否可以用做地面材料
    public abstract float Hardness {get;}                // 硬度
    public abstract int MaxWaterCapacity {get;}          // 最大容量（水）
    public abstract int MaxGasCapacity {get;}            // 最大含气量（气体）
    public abstract int WaterPenetrationRate {get;}      // 渗透率（水）
    public abstract int WaterCapacity {get; set;}        // 实际含水率
    public abstract int GasCapacity {get; set;}          // 实际含气率
}
