using Godot;
using System;

public static partial class 坐标转换器
{
    // 矩阵坐标转实际坐标
    // 导入一个三维的矩阵坐标，返回一个二维的参考系坐标
    public static Vector2 ToRealPosition(Vector3 matrixPosition)
    {
        float realX = (float)(64 * matrixPosition.X);
        float realY = (float)(64 * matrixPosition.Y);

        return new Vector2(realX, realY);
    }

    // 高差计算
    public static int CalculateHeight(float baseHight, float thisHight)
    {
        return (int)(baseHight - thisHight);
    }
}
