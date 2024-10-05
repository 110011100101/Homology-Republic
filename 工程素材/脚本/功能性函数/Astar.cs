using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

public partial class Astar
{
    public Node2D relativeNode;

    Vector3[] directions = new Vector3[]
    {
        new Vector3(-1, 0, 0), // 左
        new Vector3(1, 0, 0),  // 右
        new Vector3(0, 1, 0), // 前
        new Vector3(0, -1, 0)   // 后
    };  

    public void initstar(Node2D relativeNode)
    {
        this.relativeNode = relativeNode;
    }

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

    /// <summary>
    /// AStar寻路
    /// <para><b>注意:</b>寻路算法只能求出单层的路径, 参数中的 Vector3 包含Z坐标是用于定位block, 如需跨层移动, 可以手动将任务划分为多段路程, 反复调用此方法, 此时的 <paramref name="end"/> 应当为最近的下层入口</para>
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="context"></param>
    /// <returns>路径清单</returns>
    public List<Vector3> AStar(Vector3 start, Vector3 end, Dictionary<Vector3, 上下文> context)
    {
        test(start, end, context);
        return getPath(closeDictionary, start, end);
    }

    private async Task test(Vector3 start, Vector3 end, Dictionary<Vector3, 上下文> context)
    {
        Vector3 step; // 当前走到的位置

        // init
        step = start;

        openDictionary.Add(step, toVector4(start, 0 + (int)(Mathf.Abs(start.X - end.X) + Mathf.Abs(start.Y - end.Y))));
        Sprite2D point = new Sprite2D();
        point.Name = "text";
        relativeNode.GetChild(0).AddChild(point);
        point.Texture = GD.Load<Texture2D>("res://工程素材/美术素材包/Tile/WoodFloor_Flower.png");

        await Task.Run(() =>{
            while (step != end)
            {
                relativeNode.CallDeferred("UpdatePosition", point, step);
                GD.Print("------------------------------");
                // 打印相关信息
                GD.Print("当前位置: " + step);
                GD.Print("当前位置的父节点: " + getFatherStep(openDictionary[step]));
                GD.Print("当前位置的总代价: " + openDictionary[step].W);
                GD.Print("目标位置:" + end + "\n");
                Thread.Sleep(500);

                foreach (Vector3 direction in directions)
                {
                    Vector3 targetStep = step + direction; // 上下左右遍历

                    // 如果没有被遍历过,又存在,还能走
                    if (context.ContainsKey(targetStep) && !closeDictionary.ContainsKey(targetStep) && isStepWalkable(targetStep, context))
                    {
                        GD.Print("下一个遍历的位置" + targetStep);
                        int H = (int)(Mathf.Abs(targetStep.X - end.X) + Mathf.Abs(targetStep.Y - end.Y)); // 启发代价
                        int G = (int)(Mathf.Abs(targetStep.X - start.X) + Mathf.Abs(targetStep.Y - start.Y)); // 沉默成本
                        int F = G + H; // 总代价

                        GD.Print($"{F}(F) = {G}(沉默成本G) + {H}(启发代价H)");
                        GD.Print($"{H}(H) = (int)({Mathf.Abs(targetStep.X - end.X)} + {Mathf.Abs(targetStep.Y - end.Y)})");
                        GD.Print($"{G}(G) = (int)({Mathf.Abs(targetStep.X - start.X)} + {Mathf.Abs(targetStep.Y - start.Y)}");
                        GD.Print("\n");
                        // 如果这个节点没有在openDictionary中, 添加进去
                        if(!openDictionary.ContainsKey(targetStep))
                            openDictionary.Add(targetStep, toVector4(step ,F));
                    }
                    else if (!closeDictionary.ContainsKey(targetStep)) // 否则就添加到closeDictionary
                    {
                        closeDictionary.Add(targetStep, step);
                    }
                }
            
                closeDictionary.Add(step, getFatherStep(openDictionary[step]));
                openDictionary.Remove(step);

                if (openDictionary.Count > 0)
                    step = openDictionary.Aggregate((l, r) => l.Value.W < r.Value.W ? l : r).Key;
            }
        });

        closeDictionary.Add(step, getFatherStep(openDictionary[step]));
        point.QueueFree();
    }

    public bool isStepWalkable(Vector3 targetStep, Dictionary<Vector3, 上下文> context)
    {
        if (context[targetStep].Objects.Count > 0)
            return false;
        return true;
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
