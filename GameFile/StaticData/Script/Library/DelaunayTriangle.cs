using System;
using System.Collections.Generic;
using Godot;
using RoseIsland.Library.CalculationTool.Determinant;
using Vector2 = Godot.Vector2;

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
// 
// - Main: 程序的入口
// - InsertVertex: 插入一个点到三角剖分中
// - FlipEdge: 翻转一个边
// - CreateSuperTriangle: 创建超级三角形
// - RemoveSuperTriangle: 移除超级三角形
// - UpdateBucket: 更新桶
// - UpdateEdgeFromFace: 基于面更改边的参数
// - SetTwin: 寻找并设置伴生边
// - ToPoint: 将Vertex转化为Point
// - InsertPoint: 将一个点插入Point表中
// - isFaceEqule: 判断两个面是否是同一个面
// - IsInFace: 判断一个点是否在某个面上
// - IsFlipNeeded: 判断是否需要翻转边
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

        public static Point Main(List<Vector2> Vectors, int triangleSize)
        {
            bool isFinish = false;
            List<Vertex> vertexs = new List<Vertex>();  // 分配给桶之前,存放转化成Vertex的Vector2点
            List<Face> faces = new List<Face>();        // 用于记录所有面
            List<Edge> suspects = new List<Edge>();     // 用于记录可疑边

            // 将所有传入的二维向量转化为Vertex类型
            foreach (Vector2 temp in Vectors)
            {
                if (!vertexs.Contains(new Vertex(temp)))
                {
                    vertexs.Add(new Vertex(temp));
                }
            }

            CreateSuperTriangle(faces, triangleSize); 

            // 统一分配所有的点
            UpdateBucket(vertexs, faces);            

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
                    InsertVertex(faces, face, vertex, suspects);

                    // 不断纠正可疑边
                    while (suspects.Count > 0)
                    {
                        Edge suspect = suspects[0]; // 取出一个可疑边

                        if (IsFlipNeeded(vertex, suspect))
                        {
                            FlipEdge(faces, suspect, suspects);
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

            RemoveSuperTriangle(faces);

            return ToPoint(faces);
        }

        /// <summary>
        /// 将一个点插入到当前的三角剖分中
        /// </summary>
        /// <param name="faces">面表，包含所有面的信息</param>
        /// <param name="face">待插入点所在的面</param>
        /// <param name="vertex">待插入的点</param>
        /// <param name="suspects">可疑边列表，用于记录需要检查翻转的边</param>
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

            // 创建更加简单的面表和点集
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
        private static void FlipEdge(List<Face> faces, Edge edge, List<Edge> suspects)
        {
            // 这里不能设置成别的引用,必须是统一edge的参数
            // 这样设置后参数会更加直观(当然是对照实物图的时候)
            Face f1 = edge.Face;
            Face f2 = edge.Twin.Face;
            Face f3 = new Face(edge);
            Face f4 = new Face(edge.Twin);
            Edge e21 = edge.In;
            Edge e32 = edge.Out;
            Edge e43 = edge.Twin.In;
            Edge e14 = edge.Twin.Out;

            faces.Remove(f1);
            faces.Remove(f2);
            faces.Add(f3);
            faces.Add(f4);

            edge.Start = new Vertex(e43.Start.Position);
            edge.Twin.Start = new Vertex(e21.Start.Position);

            // f1变成f3,f2变成f4
            UpdateEdgeFromFace(f3, e21, e14, edge.Twin, e21.Twin, e14.Twin);
            UpdateEdgeFromFace(f4, e43, e32, edge, e43.Twin, e32.Twin);

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
                    face.Edge.Out.Start.Position == superVertexC.Position ||
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

            foreach (Face face in faces)
            {
                InsertPoint(points, face);
            }

            return points[0];
        }

        private static void InsertPoint(List<Point> points, Face face)
        {
            Vector2 v1 = face.Edge.Start.Position;
            Vector2 v2 = face.Edge.In.Start.Position;
            Vector2 v3 = face.Edge.Out.Start.Position;

            // 先确保每个点都存在
            Point p1 = points.Find(p => p.Position.Equals(v1));
            if (p1 == null)
            {
                p1 = new Point(v1);
                points.Add(p1);
            }

            Point p2 = points.Find(p => p.Position.Equals(v2));
            if (p2 == null)
            {
                p2 = new Point(v2);
                points.Add(p2);
            }

            Point p3 = points.Find(p => p.Position.Equals(v3));
            if (p3 == null)
            {
                p3 = new Point(v3);
                points.Add(p3);
            }

            if (p1.Neighbors.Find(p => p.Position.Equals(v2)) == null)
            {
                p1.Neighbors.Add(p2);
            }

            if (p1.Neighbors.Find(p => p.Position.Equals(v3)) == null)
            {
                p1.Neighbors.Add(p3);
            }

            if (p2.Neighbors.Find(p => p.Position.Equals(v1)) == null)
            {
                p2.Neighbors.Add(p1);
            }

            if (p2.Neighbors.Find(p => p.Position.Equals(v3)) == null)
            {
                p2.Neighbors.Add(p3);
            }

            if (p3.Neighbors.Find(p => p.Position.Equals(v1)) == null)
            {
                p3.Neighbors.Add(p1);
            }

            if (p3.Neighbors.Find(p => p.Position.Equals(v2)) == null)
            {
                p3.Neighbors.Add(p2);
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

            if (face2Points.Contains(face1Points[0]) && face2Points.Contains(face1Points[1]) && face2Points.Contains(face1Points[2]))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isFaceIn(List<Face> faces, Face face)
        {
            foreach (Face f in faces)
            {
                if (isFaceEqule(f, face))
                {
                    return true;
                }
            }
            return false;
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
    };
}