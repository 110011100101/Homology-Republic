using Godot;
using System;

/// <summary>
/// 坐标转换器
/// </summary>
public static partial class CoordinateConverter
{
    /// <summary>
    /// 矩阵坐标 -> 实际坐标
    /// </summary>
    /// <param name="matrixPosition">三维的矩阵坐标</param>
    /// <returns>二维的参考系坐标</returns>
    public static Vector2 ToRealPosition(Vector3 matrixPosition)
    {
        float realX = (float)(64 * matrixPosition.X);
        float realY = (float)(64 * matrixPosition.Y);

        return new Vector2(realX, realY);
    }

    /// <summary>
    /// 矩阵坐标 -> 实际坐标
    /// </summary>
    /// <param name="matrixPosition">二维的矩阵坐标</param>
    /// <returns>二维的参考系坐标</returns>
    public static Vector2 ToRealPosition(Vector2 matrixPosition)
    {
        float realX = (float)(64 * matrixPosition.X);
        float realY = (float)(64 * matrixPosition.Y);
        
        return new Vector2(realX, realY);
    }

    /// <summary>
    /// 实际坐标 -> 矩阵坐标
    /// </summary>
    /// <param name="realPosition">实际坐标</param>
    /// <param name="height">高度坐标</param>
    /// <returns>三维的矩阵坐标</returns>
    public static Vector3 ToMatrixPosition(Vector2 realPosition, float height)
    {
        return new Vector3((int)(realPosition.X / 64), (int)(realPosition.Y / 64), height);
    }

    /// <summary>
    /// 实际坐标 -> 矩阵坐标
    /// </summary>
    /// <param name="realPosition">实际坐标</param>
    /// <returns>二维的矩阵坐标</returns>
    public static Vector2 ToMatrixPosition(Vector2 realPosition)
    {
        return new Vector2((int)(realPosition.X / 64), (int)(realPosition.Y / 64));
    }

    /// <summary>
    /// 高度差计算
    /// </summary>
    /// <param name="baseHight">基准高度</param>
    /// <param name="targetHight">目标高度</param>
    /// <returns></returns>
    public static int CalculateHeight(float baseHight, float targetHight)
    {
        return (int)(baseHight - targetHight);
    }
}
