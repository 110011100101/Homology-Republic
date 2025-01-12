using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using RoseIsland.Library.CalculationTool.CoordinateConverter;

public static partial class Astar
{
    private static Dictionary<Vector3, Vector3> _OpenDictionary; // <节点, 父节点>
    private static Dictionary<Vector3, Vector3> _CloseDictionary; // <节点, 父节点>
    private static Dictionary<Vector3, Vector3> _StepLink; // <节点, 父节点>
    private static Dictionary<Vector3, Vector3> _CostDictionary; // <节点, Vector3(F, G, H)>
    private static List<Vector3> _path;
    private static List<Vector3> _FourDirections = new List<Vector3>(){
        new Vector3(1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, -1, 0)
    };

    /// <summary>
    /// A*寻路
    /// <para>
    /// <b>注意:</b>只能传入矩阵坐标!
    /// </para>
    /// </summary>
    /// <param name="start">起始坐标</param>
    /// <param name="end">终止坐标</param>
    /// <param name="information">上下文库</param>
    /// <returns></returns>
    public static List<Vector2> AstarAlgorithm(Vector3 start, Vector3 end, Dictionary<Vector3, 上下文> information)
    {
        Vector3 Step = start;
        Vector3 ChildStep;
        _OpenDictionary = new Dictionary<Vector3, Vector3>() { { start, start } };
        _CloseDictionary = new Dictionary<Vector3, Vector3>();
        _CostDictionary = new Dictionary<Vector3, Vector3>() { { start, new Vector3((int)(Math.Abs(Step.X - end.X) + Math.Abs(Step.Y - end.Y)), 0, (int)(Math.Abs(Step.X - end.X) + Math.Abs(Step.Y - end.Y))) } };
        _StepLink = new Dictionary<Vector3, Vector3>() { { start, start } };
        _path = new List<Vector3>();

        while (Step != end)
        {
            GD.Print($"----------<{Step}>----------\n");
            GD.Print("start: " + start);
            GD.Print("end: " + end + "\n");
            foreach (Vector3 Direction in _FourDirections)
            {
                ChildStep = Step + Direction;

                GD.Print(">>> ChildStep: " + ChildStep); // 当前遍历的节点
                if (isStepExist(information, ChildStep))
                {
                    GD.Print("· isWalkable: " + isWalkable(information, ChildStep) + "\n");
                    if (isWalkable(information, ChildStep) && !_CloseDictionary.ContainsKey(ChildStep))
                    {
                        DictionaryAdd<Vector3, Vector3>(_OpenDictionary, ChildStep, Step);
                    }
                    else
                    {
                        DictionaryAdd<Vector3, Vector3>(_CloseDictionary, ChildStep, Step);
                    }
                    
                    DictionaryAdd<Vector3, Vector3>(_StepLink, ChildStep, Step);
                    CountCost(_CostDictionary, _StepLink, ChildStep, end);
                }
                else
                {
                    // FIXME: 这里未来放报错什么的
                    GD.Print($"这里不存在{ChildStep}");
                }

            }
            DictionaryAdd<Vector3, Vector3>(_CloseDictionary, Step, _OpenDictionary[Step]);
            _OpenDictionary.Remove(Step);
            Step = _OpenDictionary.Aggregate((l, r) => _CostDictionary[l.Key].X < _CostDictionary[r.Key].X ? l : r).Key;
        }
        return GetPath(_StepLink, start, end);
    }

    private static bool isWalkable(Dictionary<Vector3, 上下文> Information, Vector3 Stap)
    {
        bool isGround = Information[Stap].GroundMaterial != null;
        // bool isObjects = Information[Stap].Items.Count() > 0;

        return isGround;
    }

    private static bool isStepExist(Dictionary<Vector3, 上下文> Information, Vector3 Stap)
    {
        return Information.ContainsKey(Stap);
    }

    private static void CountCost(Dictionary<Vector3, Vector3> CostDic, Dictionary<Vector3, Vector3> LinkDic, Vector3 Step, Vector3 end)
    {
        int G = (int)CostDic[LinkDic[Step]].Y + 1; // 沉默成本
        int H = (int)(Math.Abs(Step.X - end.X) + Math.Abs(Step.Y - end.Y));
        int F = G + H;

        GD.Print($"Position{Step} --> F:{F}, G:{G}, H:{H}\n");
        DictionaryAdd<Vector3, Vector3>(CostDic, Step, new Vector3(F, G, H));
    }

    private static List<Vector2> GetPath(Dictionary<Vector3, Vector3> CloseDic, Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3> { end };
        Vector3 step = end;

        while (step != start)
        {
            path.Add(CloseDic[step]);
            step = CloseDic[step];
        }

        List<Vector2> outPath = new List<Vector2>();
        foreach (Vector3 item in path)
        {
            outPath.Add(CoordinateConverter.ToRealPosition(item));
        }
        outPath.Reverse();
        return outPath;
    }

    private static void DictionaryAdd<T, U>(Dictionary<T, U> dic, T key, U value)
    {
        if (!dic.ContainsKey(key))
        {
            dic.Add(key, value);
        }
        else
        {
            // FIXME: 这里未来放报错什么的
        }
    }
}
