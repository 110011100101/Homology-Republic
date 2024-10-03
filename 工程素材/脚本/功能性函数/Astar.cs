using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Astar : Node
{

    Vector3[] directions = new Vector3[]
    {
        new Vector3(-1, 0, 0), // 左
        new Vector3(1, 0, 0),  // 右
        new Vector3(0, 0, -1), // 前
        new Vector3(0, 0, 1)   // 后
    };  

    /// <summary>
    /// 记录可以走的路径
    /// <para>&lt;位置 : Vector4(父节点, 总代价)&gt;</para>
    /// </summary>
    private Dictionary<Vector3, Vector4> openDictionary = new Dictionary<Vector3, Vector4>();
    /// <summary>
    /// 记录已经走过的位置
    /// <para>&lt;位置 : 父位置&gt;</para>
    /// </summary>
    private Dictionary<Vector3, Vector3> closeDictionary = new Dictionary<Vector3, Vector3>();

    public List<Vector3> AStar(Vector3 start, Vector3 end, int Level, Dictionary<Vector3, 上下文> context)
    {
        Vector3 step; // 当前走到的位置

        // init
        step = start;

        openDictionary.Add(step, toVector4(start, 0));

        while (step != end)
        {
            foreach (Vector3 direction in directions)
            {
                Vector3 targetStep = step + direction;

                if (isStepExist(targetStep,context) && !closeDictionary.ContainsKey(targetStep) && isStepWalkable(targetStep, context))
                {
                    
                    int H = (int)(Mathf.Abs(targetStep.X - end.X) + Mathf.Abs(targetStep.Y - end.Y)); // 启发代价
                    int G = (int)openDictionary[step].W - H + 1; // 沉默成本
                    int F = G + H; // 总代价

                    openDictionary.Add(targetStep, toVector4(step ,F));
                }
                else
                {
                    closeDictionary.Add(targetStep, step);
                }
            }
            
            if (openDictionary.Count != 0)
            {
                openDictionary.Remove(step);
                closeDictionary.Add(step, getFatherStep(openDictionary[step]));
                step = openDictionary.Max().Key;
            }
            else
            {
                return null;
            }
        }

        // 输出路径
        return getPath(closeDictionary, start, end);
    }

    public bool isStepWalkable(Vector3 targetStep, Dictionary<Vector3, 上下文> context)
    {
        if (context[targetStep].Objects.Count != 0)
        {
            return false;
        }
        
        return true;
    }

    public bool isStepExist(Vector3 targetStep, Dictionary<Vector3, 上下文> context)
    {
        return context.ContainsKey(targetStep);
    }
    
    public Vector4 toVector4(Vector3 vector3, int value)
    {
        return new Vector4(vector3.X, vector3.Y, vector3.Z, value);
    }

    public Vector3 getFatherStep(Vector4 vector4)
    {
        return new Vector3(vector4.X, vector4.Y, vector4.Z);
    }

    public List<Vector3> getPath(Dictionary<Vector3, Vector3> closeDictionary, Vector3 start, Vector3 end)
    {
        Vector3 father = closeDictionary[end];
        List<Vector3> path = new List<Vector3> { end };

        while (father != start)
        {
            path.Add(father);
            father = closeDictionary[father];
        }
        
        path.Add(start);
        path.Reverse();
        return path;
    }
}
