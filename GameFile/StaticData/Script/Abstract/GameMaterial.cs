public abstract class GameMaterial
{
    public abstract string strName {get;}                   // 名称
    public abstract string strGroundTexturePath {get;}      // 方块纹理路径
    public abstract string strFloorTexturePath {get;}       // 地板纹理路径
    public abstract MaterialType Type {get;}                // 类型
    public abstract bool isFlammable {get;}                 // 是否易燃
    public abstract bool isFlooraMaterial {get;}            // 是否可以用做地板材料
    public abstract float fHardness {get;}                  // 硬度
    public abstract int intMaxWaterCapacity {get;}          // 最大容量（水）
    public abstract int intMaxGasCapacity {get;}            // 最大含气量（气体）
    public abstract int intPenetrationRateWater {get;}      // 渗透率（水）
    public abstract int intWaterCapacity {get; set;}        // 实际含水率
    public abstract int intGasCapacity {get; set;}          // 实际含气率
}
