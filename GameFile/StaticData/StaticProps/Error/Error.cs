using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

public static class KTError
{
    public static void PrintError(string error_code, Node error_node, string error_script)
    {
        GD.Print("----------------------------------报错----------------------------------");
        GD.Print($"Error: {KTErrorNameDic[error_code]}");
        GD.Print($"{error_code}: {KTErrorExplainDic[error_code]}");
        GD.Print($"\n发生错误的节点: {error_node.GetPath}");
        GD.Print($"路径: {error_node.GetPath()}");
        GD.Print($"发生错误的脚本: {error_script}");
        GD.Print("------------------------------------------------------------------------");
    }

    public static Dictionary<string, string> KTErrorNameDic = new()
    {
        {"KT0001", TimeOutError.ErrorName},
    };

    public static Dictionary<string, string> KTErrorExplainDic = new()
    {
        {"KT0001", TimeOutError.ErrorExplain},
    };

    public static class TimeOutError
    {
        public const string ErrorName = "TimeOutError(超时错误)";
        public const string ErrorExplain = "此错误表示函数执行超时";
    }
}