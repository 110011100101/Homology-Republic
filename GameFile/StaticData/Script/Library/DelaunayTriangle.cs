using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using RoseIsland.Library.CalculationTool.Determinant;
using Vector2 = System.Numerics.Vector2;

// -------------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------------
// 
// DelaunayTriangle.cs
//
// 概述:
// 该文件实现了 Delaunay 三角剖分算法。Delaunay 三角剖分是一种将一组点集转换为三角形网格的方法，
// 使得任意三角形的外接圆不包含其他任何输入点。这种三角剖分在计算机图形学、地理信息系统和有限元分析等领域有广泛应用。
//
// 主要类和方法:
// - vertex: 表示一个二维平面上的点。
// - edge: 表示一条边。
// - face: 表示一个平面。
// - DelaunayTriangle: 静态类，包含实现 Delaunay 三角剖分的核心方法。
//   - Main: 主函数，初始化并执行 Delaunay 三角剖分。
//   - InsertVertex: 插入点到当前的三角剖分中。
//   - FlipEdge: 翻转指定的边以优化三角剖分。
//   - IsFlipNeeded: 判断是否需要翻转指定的边。
//   - CreateSuperTriangle: 创建一个超级三角形作为初始三角剖分的基础。
// 
// -------------------------------------------------------------------------------------------------------
// 
// 关于 DelaunayTriangle 的数据结构:
//      访问逻辑 面 --> 边 --> 点
//                        --> 边 
//      
// 关于优化:
//      优化插入点时检测点在哪个面内:
//          采用 桶 结构, 即每次分割面时, 面将分成三份, 此时对面内的点进行进一步分类进入已经与面形成一一对应关系的桶中
// 
// -------------------------------------------------------------------------------------------------------
// 
// 参考文献:
//      [1] 有一种悲伤叫颓废, & 鹤翔万里. (2023, October 1). 计算几何]随机增量法构造德劳内三角网 [Video]. Bilibili. https://www.bilibili.com/video/BV1Ck4y1z7VT?vd_source=5f2ad51027b889960dcb1af3acd7ac1a
// 
// -------------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------------

namespace RoseIsland.Library.Algorithm.DelaunayTriangle
{
    public class Vertex
    {
        public Vector2 Position;    // 坐标

        public Vertex(Vector2 position)
        {
            Position = position;
        }
    };

    public class Edge
    {
        public Vertex Start;    // 起点(起点是用来访问的,不是用来定义边的)
        public Edge Twin;       // 伴生边(也就是另一个方向的边而已)
        public Edge Out;        // 出边
        public Edge In;         // 入边
        public Face Face;       // 边的属面

        public Edge(Vertex start)
        {
            Start = start;
        }
    };

    public class Face
    {
        public Edge Edge;                   // 属边
        public List<Vertex> Bucket;         // 桶

        public Face(Edge edge)
        {
            Edge = edge;
            Bucket = new List<Vertex>();
        }
    };

    public static class DelaunayTriangle
    {

        private static Vertex superVertexA;
        private static Vertex superVertexB;
        private static Vertex superVertexC;
        private static Node2D TestNode;

        /// <summary>
        /// 主函数
        /// </summary>
        /// <param name="vertexs">点集</param>
        /// <param name="triangleSize">三角形的大小的基准,实际上就是点生成的范围</param>
        public static async Task<Point> Main(Godot.Collections.Array InVectors, int triangleSize, Node2D testNode)
        {
            await Task.Delay(1000);
            List<Vector2> Vectors = new List<Vector2>();
            foreach (Godot.Vector2 godotVector in InVectors)
            {
                Vectors.Add(new Vector2(godotVector.X, godotVector.Y));
            }

            TestNode = testNode;
            bool isFinish = false;
            List<Vertex> vertexs = new List<Vertex>();
            List<Face> faces = new List<Face>();        // 用于记录所有面
            List<Edge> suspects = new List<Edge>();     // 用于记录可疑边

            // 将所有point转化为Vertex
            foreach (Vector2 temp in Vectors)
            {
                if (!vertexs.Contains(new Vertex(temp)))
                {
                    vertexs.Add(new Vertex(temp));
                }
            }

            CreateSuperTriangle(faces, triangleSize); // 创建超级三角形
            
            await DrawFace(faces);
            // Debug_PrintFaces(faces, $"{nameof(Main)}", "检查超级三角形是否创建成功");

            UpdateBucket(vertexs, faces);            // 对于全家桶, 将全部的点分配给全部的面

            while (!isFinish)
            {
                Vertex vertex = null;
                Face face = null;

                // 寻找一个点
                foreach (Face temp in faces)
                {
                    if (temp.Bucket.Count > 0)
                    {
                        vertex = temp.Bucket[0];
                        face = temp;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (vertex != null)
                {
                    GD.Print("\n插入点\t", vertex.Position);
                    InsertVertex(faces, face, vertex, suspects);
                    
                    await DrawFace(faces);
                    // Debug_PrintFaces(faces, $"{nameof(Main)}", $"这是在插入点{vertex.Position}后的检查");

                    // 不断纠正可疑边
                    while (suspects.Count > 0)
                    {
                        Edge suspect = suspects[0]; // 取出一个可疑边

                        GD.Print("检查可疑边\t", suspect.Start.Position, " to ", suspect.Out.Start.Position);
                        
                        GD.Print("IsFlipNeeded\t", IsFlipNeeded(vertex, suspect));

                        if (IsFlipNeeded(vertex, suspect))
                        {
                            await FlipEdge(faces, suspect, suspects);
                            await DrawFace(faces);
                        }

                        suspects.RemoveAt(0);
                    }

                    // 将分配完的点移出桶
                    face.Bucket.Remove(vertex);
                }
                else
                {
                    isFinish = true;
                }
            }
            
            
            Debug_PrintFaces(faces, $"{nameof(Main)}", "这是在剔除超三角形之前的检查,");
            RemoveSuperTriangle(faces);
            Debug_PrintFaces(faces, $"{nameof(Main)}", "这是剔除超三角形之后的检查,");

            return ToPoint(faces);
        }

        /// <summary>
        /// 将一个点插入到当前的三角剖分中。
        /// </summary>
        /// <param name="faces">面表，包含所有面的信息。</param>
        /// <param name="face">待插入点所在的面。</param>
        /// <param name="vertex">待插入的点。</param>
        /// <param name="suspects">可疑边列表，用于记录需要检查翻转的边。</param>
        private static void InsertVertex(List<Face> faces, Face face, Vertex vertex, List<Edge> suspects)
        {
            // 属面转移三边到三个新面
            // 创建新面
            Face faceA = new Face(new Edge(face.Edge.Start));
            Face faceB = new Face(new Edge(face.Edge.Out.Start));
            Face faceC = new Face(new Edge(face.Edge.In.Start));

            // 引入可疑边
            suspects.Add(faceA.Edge);
            suspects.Add(faceB.Edge);
            suspects.Add(faceC.Edge);

            // 针对每个面开始链接
            // 链接逻辑：原边 --> 原 Out 的 startVertex 到 新点 --> 新点到原边 startVertex
            // 更新一波信息
            // FIXME: 这个有问题会导致faces的初始值跟FaceA一样
            UpdateEdgeFromFace(faceA, new Edge(face.Edge.Out.Start), new Edge(vertex));
            UpdateEdgeFromFace(faceB, new Edge(face.Edge.Out.Out.Start), new Edge(vertex));
            UpdateEdgeFromFace(faceC, new Edge(face.Edge.In.Out.Start), new Edge(vertex));
            
            // 更新面表信息
            if (!isFaceIn(faces, faceA))
            {
                faces.Add(faceA);
            }

            if (!isFaceIn(faces, faceB))
            {
                faces.Add(faceB);
                
            }
            
            if (!isFaceIn(faces, faceC))
            {
                faces.Add(faceC);
            }

            // 设置伴生边
            foreach (Face orgin in faces)
            {
                SetTwin(orgin, faces);
            }

            faces.Remove(face);

            // 分桶
            // 创建简单的面表和点集
            List<Vertex> simpleVertexs = new List<Vertex>();
            List<Face> simpleFaces = new List<Face>();

            // 构建更简单的点集合新面表
            simpleVertexs.AddRange(face.Bucket);
            simpleFaces.Add(faceA);
            simpleFaces.Add(faceB);
            simpleFaces.Add(faceC);

            // 更新桶的信息
            UpdateBucket(simpleVertexs, simpleFaces);
        }

        /// <summary>
        /// 翻转指定的边以优化三角剖分。
        /// </summary>
        /// <param name="faces">面表，包含所有面的信息。</param>
        /// <param name="edge">需要翻转的边。</param>
        /// <param name="suspects">可疑边列表，用于记录需要检查翻转的边。</param>
        private static async Task FlipEdge(List<Face> faces, Edge edge, List<Edge> suspects)
        {  
            Face f1 = edge.Face;
            Face f2 = edge.Twin.Face;
            Edge e21 = edge.In;
            Edge e32 = edge.Out;
            Edge e43 = edge.Twin.In;
            Edge e14 = edge.Twin.Out;
            GD.Print(0);
            await DrawFace(faces);

            
            faces.Remove(f1);
            faces.Remove(f2);
            Face f3 = new Face(edge);
            Face f4 = new Face(edge.Twin);
            faces.Add(f3);
            faces.Add(f4);

            GD.Print(1);
            await DrawFace(faces);
            // Debug_PrintFaces(faces, nameof(FlipEdge), "检查进行边翻转操作前的状态");

            edge.Start = new Vertex(e43.Start.Position);
            edge.Twin.Start = new Vertex(e21.Start.Position);

            GD.Print(2);
            await DrawFace(faces);
            // Debug_PrintFaces(faces, nameof(FlipEdge), "如果这个除了更改的主边跟上面不一样,很可能是更改两条边的指向的操作时有问题");
            Debug_PrintFaces(new List<Face>() { f3, f4 }, nameof(FlipEdge), "操作之前");
            
            // f1变成f3,f2变成f4
            UpdateEdgeFromFace(f3, e21, e14, edge.Twin, e21.Twin, e14.Twin);
            UpdateEdgeFromFace(f4, e43, e32, edge, e43.Twin, e32.Twin);

            GD.Print(3);
            Debug_PrintFaces(new List<Face>() { f3, f4 }, nameof(FlipEdge), "这个是检查涉及到的几个面");
            await DrawFace(faces);
            // Debug_PrintFaces(faces, nameof(FlipEdge), "如果这个跟上面不一样,那就是进行顶替操作的时候有问题");

            List<Vertex> simpleVertexs = new List<Vertex>();
            List<Face> simpleFaces = new List<Face>();

            // 构建更简单的点集合新面表
            simpleVertexs.AddRange(f1.Bucket);
            simpleVertexs.AddRange(f2.Bucket);
            simpleFaces.Add(f3);
            simpleFaces.Add(f4);

            // 更新桶的信息
            UpdateBucket(simpleVertexs, simpleFaces);

            // 引入新的可疑边
            suspects.Add(e14);
            suspects.Add(e43);
        }

        /// <summary>
        /// 判断是否需要翻转
        /// </summary>
        /// <param name="baseVertex">基点<para><b>通常为插入点</b></para></param>
        /// <param name="suspect">可疑边</param>
        /// <returns>Bool</returns>
        private static bool IsFlipNeeded(Vertex baseVertex, Edge suspect)
        {
            if (suspect.Twin != null)
            {
                // 获取三角形的三个顶点坐标
                double x0 = suspect.Start.Position.X;
                double y0 = suspect.Start.Position.Y;
                double x1 = baseVertex.Position.X;
                double y1 = baseVertex.Position.Y;
                double x2 = suspect.Out.Start.Position.X;
                double y2 = suspect.Out.Start.Position.Y;

                // 获取需要判断的点坐标
                double px = suspect.Twin.In.Start.Position.X;
                double py = suspect.Twin.In.Start.Position.Y;

                // 计算外接圆的圆心和半径
                double a1 = x1 - x0;
                double b1 = y1 - y0;
                double c1 = (a1 * a1 + b1 * b1) / 2.0;

                double a2 = x2 - x0;
                double b2 = y2 - y0;
                double c2 = (a2 * a2 + b2 * b2) / 2.0;

                double d = a1 * b2 - a2 * b1;
                double Cntrx = x0 + (c1 * b2 - c2 * b1) / d;
                double Cntry = y0 + (a1 * c2 - a2 * c1) / d;

                double Radius_2 = Math.Pow(x0 - Cntrx, 2) + Math.Pow(y0 - Cntry, 2);

                // 计算点 P 到圆心的距离平方
                double Dist_P_Cntr_2 = Math.Pow(px - Cntrx, 2) + Math.Pow(py - Cntry, 2);

                // 判断点 P 是否在圆内或圆上
                return Dist_P_Cntr_2 <= Radius_2;
            }
            return false;
        }

        /// <summary>
        /// 创建超三角形
        /// </summary>
        /// <param name="faces">面表</param>
        /// <param name="triangleSize">三角形基准大小</param>
        private static void CreateSuperTriangle(List<Face> faces, int triangleSize)
        {
            superVertexA = new Vertex(new Vector2(-triangleSize, triangleSize));
            superVertexB = new Vertex(new Vector2(triangleSize / 2, -(3 * triangleSize) / 2));
            superVertexC = new Vertex(new Vector2(2 * triangleSize, triangleSize));

            faces.Add(new Face(new Edge(superVertexA)));

            Edge outEdge = new Edge(superVertexB);
            Edge inEdge = new Edge(superVertexC);

            UpdateEdgeFromFace(faces[0], outEdge, inEdge);
        }

        public static void RemoveSuperTriangle(List<Face> faces)
        {
            for (int i = faces.Count - 1; i >= 0; i--)
            {
                Face face = faces[i];
                if (face.Edge.Out.Start.Position == superVertexA.Position ||
                    face.Edge.Out.Start.Position == superVertexB.Position ||
                    face.Edge.Out.Start.Position == superVertexC.Position||
                    face.Edge.In.Start.Position == superVertexA.Position ||
                    face.Edge.In.Start.Position == superVertexB.Position ||
                    face.Edge.In.Start.Position == superVertexC.Position ||
                    face.Edge.Start.Position == superVertexA.Position ||
                    face.Edge.Start.Position == superVertexB.Position ||
                    face.Edge.Start.Position == superVertexC.Position)
                {
                    // Debug_PrintFaces(new List<Face>{ face }, nameof(RemoveSuperTriangle), "这是检查移除的面是否是真的可以移出");
                    faces.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 此方法会将传入的点集中的点逐个取出匹配面表中的面, 若在面内则加入面的桶中
        /// <para>
        /// PS：你可以尝试把桶传入点集, 可以实现细分桶的效果
        /// </para>
        /// <para>
        /// PS：你也可以创建一个精简的面表,这样减少不必要的计算
        /// </para>
        /// </summary>
        /// <param name="vertexs">点集</param>
        /// <param name="faces">面表</param>
        private static void UpdateBucket(List<Vertex> vertexs, List<Face> faces, bool isRemove = true)
        {
            foreach (Vertex vertex in vertexs)
                foreach (Face face in faces)
                {
                    // 将这三个行列式ABC
                    double[,] A = {
                        {face.Edge.Start.Position.X, face.Edge.Start.Position.Y, 1},
                        {face.Edge.Out.Start.Position.X, face.Edge.Out.Start.Position.Y, 1},
                        {vertex.Position.X, vertex.Position.Y, 1}
                    };
                    double[,] B = {
                        {face.Edge.Out.Start.Position.X, face.Edge.Out.Start.Position.Y, 1},
                        {face.Edge.In.Start.Position.X, face.Edge.In.Start.Position.Y, 1},
                        {vertex.Position.X, vertex.Position.Y, 1}
                    };
                    double[,] C = {
                        {face.Edge.In.Start.Position.X, face.Edge.In.Start.Position.Y, 1},
                        {face.Edge.Start.Position.X, face.Edge.Start.Position.Y, 1},
                        {vertex.Position.X, vertex.Position.Y, 1}
                    };
                    // 放入计算器中计算正负,需要全部同号且不为零
                    if (Determinant.GetDeterminantSign(A) == 1 && Determinant.GetDeterminantSign(B) == 1 && Determinant.GetDeterminantSign(C) == 1)
                    {
                        if (isRemove)
                        {
                            foreach (Face temp in faces)
                            {
                                if (temp.Bucket.Contains(vertex))
                                {
                                    temp.Bucket.Remove(vertex);
                                    break;
                                }
                            }
                        }

                        face.Bucket.Add(vertex);
                        break;
                    }
                }
        }

        /// <summary>
        /// 通过面轮询更新边的信息
        /// <para>
        /// 参数: 主面, 出边, 入边, 主边伴生边, Out伴生边, In伴生边
        /// </para>
        /// </summary>
        /// <param name="face">主体面</param>
        /// <param name="twinMain">主边伴生边</param>
        /// <param name="twinOut">Out伴生边</param>
        /// <param name="twinIn">In伴生边</param>
        /// <param name="out">出边</param>
        /// <param name="in">入边</param>
        private static void UpdateEdgeFromFace(Face face, Edge @out, Edge @in, Edge twinMain = null, Edge twinOut = null, Edge twinIn = null)
        {
            face.Edge.Twin = twinMain;
            face.Edge.Out = @out;
            face.Edge.In = @in;
            face.Edge.Face = face;

            // FaceEdge 的 Out
            @out.Twin = twinOut;
            @out.Out = @in;
            @out.In = face.Edge;
            @out.Face = face;

            // FaceEdge 的 In
            @in.Twin = twinIn;
            @in.Out = face.Edge;
            @in.In = @out;
            @in.Face = face;
        }

        /// <summary>
        /// 此方法将一条边与面表中的面的所有边逐一进行匹配
        /// </summary>
        /// <param name="face">待设置边的面</param>
        /// <param name="faces">面表</param>
        private static void SetTwin(Face face, List<Face> faces)
        {
            Vertex vertexBegin = face.Edge.Out.Start;
            Vertex vertexEnd = face.Edge.Start;

            foreach (Face twinFace in faces)
            {
                // 排除自己
                if (twinFace != face)
                {
                    if (vertexBegin.Equals(twinFace.Edge.Start) && vertexEnd.Equals(twinFace.Edge.Out.Start))
                    {
                        face.Edge.Twin = twinFace.Edge;
                        break;
                    }
                    else if (vertexBegin.Equals(twinFace.Edge.Out.Start) && vertexEnd.Equals(twinFace.Edge.In.Start))
                    {
                        face.Edge.Twin = twinFace.Edge.Out;
                        break;
                    }
                    else if (vertexBegin.Equals(twinFace.Edge.In.Start) && vertexEnd.Equals(twinFace.Edge.Start))
                    {
                        face.Edge.Twin = twinFace.Edge.In;
                        break;
                    }
                    else
                    {
                        face.Edge.Twin = null;
                    }
                }
            }
        }

        private static Point ToPoint(List<Face> faces)
        {
            List<Point> points = new List<Point>();

            // Debug_PrintFaces(faces, $"{nameof(ToPoint)}", "将Vertex转成Point前的检查,如果这个跟下面末尾输出的不一样,很可能是InsertPoint方法有问题");

            foreach (Face face in faces)
            {
                InsertPoint(points, face);
            }

            Debug_PrintFaces(faces, $"{nameof(ToPoint)}", "这是在程序的末尾");

            PrintNet(points[0], new List<Point>());
            
            return points[0];
        }

        private static void InsertPoint(List<Point> points, Face face)
        {
            Vector2 v1 = face.Edge.Start.Position;
            Vector2 v2 = face.Edge.In.Start.Position;
            Vector2 v3 = face.Edge.Out.Start.Position;

            // 先确保每个点都存在
            Point p1 = points.Find(p => p.Position == v1);
            if (p1 == null)
            {
                p1 = new Point(v1);
                points.Add(p1);
            }

            Point p2 = points.Find(p => p.Position == v2);
            if (p2 == null)
            {
                p2 = new Point(v2);
                points.Add(p2);
            }
            
            Point p3 = points.Find(p => p.Position == v3);
            if (p3 == null)
            {
                p3 = new Point(v3);
                points.Add(p3);
            }

            if (p1.Neighbors.Find(p => p.Position == v2) == null)
            {
                p1.Neighbors.Add(p2);
            }

            if (p1.Neighbors.Find(p => p.Position == v3) == null)
            {
                p1.Neighbors.Add(p3);
            }

            if (p2.Neighbors.Find(p => p.Position == v1) == null)
            {
                p2.Neighbors.Add(p1);
            }

            if (p2.Neighbors.Find(p => p.Position == v3) == null)
            {
                p2.Neighbors.Add(p3);
            }

            if (p3.Neighbors.Find(p => p.Position == v1) == null)
            {
                p3.Neighbors.Add(p1);
            }

            if (p3.Neighbors.Find(p => p.Position == v2) == null)
            {
                p3.Neighbors.Add(p2);
            }
        }

        private static void Debug_PrintFaces(List<Face> faces, string where, string information = null)
        {
            GD.Print($"\n{information}\n");
            GD.Print($"共 ({faces.Count}) 个面");
            foreach (Face face in faces)
            {
                GD.Print($"--=--------{where}--------=--");
                GD.Print($"----------{face.Edge.Start.Position}----------");
                GD.Print($"Edge:\t{face.Edge.Start.Position}");
                if (face.Edge.Twin != null)
                GD.Print($"Twin:\t{face.Edge.Twin.Start.Position}");
                else
                GD.Print("Twin:\tnull");
                GD.Print($"Out:\t{face.Edge.Out.Start.Position}");
                GD.Print($"In:\t\t{face.Edge.In.Start.Position}");
                GD.Print($"Face:\t{face.Edge.Face.Edge.Start.Position}");
                GD.Print($"---OUT-->");
                GD.Print($"Edge:\t{face.Edge.Out.Start.Position}");
                if(face.Edge.Out.Twin != null)
                GD.Print($"Twin:\t{face.Edge.Out.Twin.Start.Position}");
                else
                GD.Print("Twin:\tnull");
                GD.Print($"Out:\t{face.Edge.Out.Out.Start.Position}");
                GD.Print($"In:\t\t{face.Edge.Out.In.Start.Position}");
                GD.Print($"Face:\t{face.Edge.Out.Face.Edge.Start.Position}");
                GD.Print($"---IN-->");
                GD.Print($"Edge:\t{face.Edge.In.Start.Position}");
                if (face.Edge.In.Twin != null)
                GD.Print($"Twin:\t{face.Edge.In.Twin.Start.Position}");
                else
                GD.Print("Twin:\tnull");
                GD.Print($"Out:\t{face.Edge.In.Out.Start.Position}");
                GD.Print($"In:\t\t{face.Edge.In.In.Start.Position}");
                GD.Print($"Face:\t{face.Edge.In.Face.Edge.Start.Position}");
                GD.Print($"--=--------{where}--------=--");
            }
        }

        private static void PrintNet(Point point, List<Point> visitedPoints)
        {
            // 首先检查当前点是否已经访问过
            if (visitedPoints.Contains(point))
            {
                return;
            }

            // 标记当前点为已访问
            visitedPoints.Add(point);

            // 打印当前点的信息
            GD.Print($"点: {point.Position}");

            // 打印当前点的邻居点信息
            GD.Print("邻居:");
            foreach (Point neighbor in point.Neighbors)
            {
                GD.Print($"  {neighbor.Position}");
            }

            // 遍历当前点的所有邻居点，并递归调用 PrintNet 函数
            foreach (Point neighbor in point.Neighbors)
            {
                PrintNet(neighbor, visitedPoints);
            }
        }

        public static bool isFaceEqule(Face face1, Face face2)
        {
            List<Vector2> face1Points = new List<Vector2>()
            {
                face1.Edge.Start.Position,
                face1.Edge.Out.Start.Position,
                face1.Edge.In.Start.Position
            };

            List<Vector2> face2Points = new List<Vector2>()
            {
                face2.Edge.Start.Position,
                face2.Edge.Out.Start.Position,
                face2.Edge.In.Start.Position
            };

            // foreach (Vector2 point in face1Points)
            // {
            //     GD.Print(point);
            // }

            // foreach(Vector2 point in face2Points)
            // {
            //     GD.Print(point);
            // }
            
            if (face2Points.Contains(face1Points[0]) && face2Points.Contains(face1Points[1]) && face2Points.Contains(face1Points[2]))
            {
                // GD.Print("isFaceEqule:\ttrue");
                return true;
            }
            else
            {
                // GD.Print("isFaceEqule:\tfalse");
                return false;
            }
        }

        public static bool isFaceIn(List<Face> faces, Face face)
        {
            foreach (Face f in faces)
            {
                if (isFaceEqule(f, face))
                {
                    // GD.Print("isFaceIn:\ttrue");
                    return true;
                }
            }
            // GD.Print("isFaceIn:\tfalse");
            return false;
        }

        private static async Task DrawFace(List<Face> faces)
        {
            // 重置图
            Node2D node2D = new Node2D();
            
            if (TestNode.GetChildCount() > 0)
            {
                TestNode.RemoveChild(TestNode.GetChild(0));
            }

            TestNode.AddChild(node2D);

            // 绘制新图
            foreach (Face face in faces)
            {
                // 取出坐标
                Godot.Vector2 v1 = new Godot.Vector2(face.Edge.Start.Position.X, face.Edge.Start.Position.Y);
                Godot.Vector2 v2 = new Godot.Vector2(face.Edge.Out.Start.Position.X, face.Edge.Out.Start.Position.Y);
                Godot.Vector2 v3 = new Godot.Vector2(face.Edge.In.Start.Position.X, face.Edge.In.Start.Position.Y);

                // 构建三个点
                Sprite2D p1 = new Sprite2D() { Name = $"{v1}", Position = v1, Texture = (Texture2D)GD.Load("res://GameFile/StaticData/GameAssets/Texture/GridConceptPack/tiles/FeaturePoints.png"), Scale = new Godot.Vector2(0.1f, 0.1f) };
                Sprite2D p2 = new Sprite2D() { Name = $"{v2}", Position = v2, Texture = (Texture2D)GD.Load("res://GameFile/StaticData/GameAssets/Texture/GridConceptPack/tiles/FeaturePoints.png"), Scale = new Godot.Vector2(0.1f, 0.1f) }; 
                Sprite2D p3 = new Sprite2D() { Name = $"{v3}", Position = v3, Texture = (Texture2D)GD.Load("res://GameFile/StaticData/GameAssets/Texture/GridConceptPack/tiles/FeaturePoints.png"), Scale = new Godot.Vector2(0.1f, 0.1f) };
                
                // 构建边
                Line2D L1 = new Line2D() { Name = $"{v1}-{v2}", Points = new Godot.Vector2[]{ v1, v2 }, Width = 1 };
                Line2D L2 = new Line2D() { Name = $"{v2}-{v3}", Points = new Godot.Vector2[]{ v2, v3 }, Width = 1 };
                Line2D L3 = new Line2D() { Name = $"{v3}-{v1}", Points = new Godot.Vector2[]{ v3, v1 }, Width = 1 };

                // 绘制三个点
                if (!node2D.HasNode($"{p1.Name}"))
                {
                    node2D.AddChild(p1);
                    // await Task.Delay(500);
                }
                    
        
                if (!node2D.HasNode($"{p2.Name}"))
                {
                    node2D.AddChild(p2); 
                    // await Task.Delay(500);
                }

                if (!node2D.HasNode($"{p3.Name}"))
                {
                    node2D.AddChild(p3);
                    // await Task.Delay(500);
                }

                if (!node2D.HasNode($"{L1.Name}"))
                {
                    node2D.AddChild(L1);
                    // await Task.Delay(500);
                }

                if (!node2D.HasNode($"{L2.Name}"))
                {
                    node2D.AddChild(L2);
                    // await Task.Delay(500);
                }

                if (!node2D.HasNode($"{L3.Name}"))
                {
                    node2D.AddChild(L3);
                    // await Task.Delay(500);
                }
            }
            await Task.Delay(2000);
        }
    };
}