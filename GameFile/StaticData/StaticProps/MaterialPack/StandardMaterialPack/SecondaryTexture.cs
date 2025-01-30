using System.Collections.ObjectModel;

public class Cloud : Air
{
    public new EnumMaterial Material = EnumMaterial.Cloud;

    public int deltaGrayLevel; // 灰度等级

}

public class Ice : Water
{
    public new EnumMaterial Material = EnumMaterial.Ice;

    public int melitingSpeed_basic; // 基础融化速度
    public int meltingSpeed; // 实际的融化速度
}

public class Snow : Water
{
    public new EnumMaterial Material = EnumMaterial.Snow;
}
