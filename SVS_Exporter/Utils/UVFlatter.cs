using ADV.Commands.Chara;
using AsmResolver.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SVSExporter.Utils
{
    class Vertex
    {
        private static int ids = 0;
        public Vector3 poi;
        public int id;
        public Dictionary<int, Edge> edges = new Dictionary<int, Edge>();//another vertex, Edge
        public List<Face> faces = new List<Face>();
        public int root;//Used to get uv island

        public Vector3 uv;
        public int visitedFaceCount = 0;
        public Vertex(Vector3 poi) 
        {
            this.poi = poi;
            id = ids++;
            root = id;
        }
        public bool GetEdge(int v, out Edge edge)
        {
            if (edges.TryGetValue(v, out edge))
            {
                return true;
            }
            return false;
        }
        public bool GetEdge(Vertex v, out Edge edge)
        {
            if (edges.TryGetValue(v.id, out edge))
            {
                return true;
            }
            return false;
            
        }
        public void Replace(Edge e)
        {
            int index = e.vertices[0].id;
            if (index == id) { index = e.vertices[1].id;  }
            edges[index] = e;
        }
        public static void Reset()
        {
            ids = 0;
        }
    }

    class Edge
    {
        private static int ids = 0;
        public int id;
        public Vertex[] vertices = new Vertex[2];
        public Dictionary<int, Face> faces = new Dictionary<int, Face>();// Third vertex id, face

        public Edge(Vertex v0, Vertex v1)
        {
            if (v0.id < v1.id)
            {
                vertices[0] = v0;
                vertices[1] = v1;
            }
            else
            {
                vertices[0] = v1;
                vertices[1] = v0;
            }
            id = ids++;
        }
        public bool Split(out Vertex v0, out Vertex v1, out Edge edge)
        {
            v0 = null;
            v1 = null;
            edge = null;

            if (faces.Count <= 1) return false;

            var data = faces.First();
            faces.Remove(data.Key);
            Face face = data.Value;
            v0 = new Vertex(vertices[0].poi);
            v1 = new Vertex(vertices[1].poi);
            v0.faces.Add(face);
            v1.faces.Add(face);
            edge = new Edge(v0, v1);
            face.Replace(vertices[0], vertices[1], v0, v1, this, edge);
            edge.faces.Add(face.GetVertex(edge).id, face);
            return true;
        }
        public static void Reset()
        {
            ids = 0;
        }

    }
    class Face
    {
        private static int ids = 0;
        public int id;
        public Vertex[] vertices = new Vertex[3];
        public Edge[] edges = new Edge[3];//edge and vertex with the same index compose this triangle
        private int[] vertexSequence = new int[3] { 0, 1, 2 };
        public Face(Vertex v0, Vertex v1, Vertex v2, Edge e01, Edge e12, Edge e20)
        {
            vertices[0] = v0;
            vertices[1] = v1;
            vertices[2] = v2;
            edges[0] = e12;
            edges[1] = e20;
            edges[2] = e01;
            id = ids++;
        }
        public void Replace(Vertex oriv0, Vertex oriv1, Vertex newv0, Vertex newv1, Edge orie0, Edge newe0e)
        {
            for (int i = 0; i < 3; i++)
            {
                if (vertices[i] == oriv0) { vertices[i] = newv0; }
                else if (vertices[i] == oriv1) { vertices[i] = newv1; }
            }
            for (int i = 0; i < 3; i++)
            {
                if (edges[i] == orie0) { edges[i] = newe0e; break; }
            }
        }
        public Vertex GetVertex(Edge edge)
        {
            for(int i = 0; i < 3; i++)
            {
                if (vertices[i] != edge.vertices[0] && vertices[i] != edge.vertices[1]) { return vertices[i]; }
            }
            throw new Exception("Face do not have this Edge");
        }
        public void GetVertex(Vertex vertex, out Vertex v0, out Vertex v1)
        {
            v0 = null;
            v1 = null;
            for(int i = 0;i < 3; i++)
            {
                if (vertices[i] != vertex)
                {
                    if (v0 == null) { v0 = vertices[i]; }
                    else { v1 = vertices[i]; return; }
                }
            }
        }
        public int GetVertexSequence(Vertex vertex)
        {
            for(int i = 0; i < 3; i++)
            {
                if (vertices[i] == vertex) { return vertexSequence[i]; }
            }
            throw new Exception("Given Vertex is not present in Face");
        }
        public void Exchange(int v0, int v1)
        {
            vertexSequence[v0] = v1;
            vertexSequence[v1] = v0;
        }
        public static void Reset()
        {
            ids = 0;
        }
    }

    class UVIsland
    {
        public List<KeyValuePair<int, Vector2>> uv;
        public float minX;
        public float minY;
        public float width;
        public float height;
        public float maxX;
        public float maxY;

        public UVIsland(List<KeyValuePair<int, Vector2>> uv, float minX, float minY, float width, float height, float maxX, float maxY)
        {
            this.uv = uv;
            this.minX = minX;
            this.minY = minY;
            this.width = width;
            this.height = height;
            this.maxX = maxX;
            this.maxY = maxY;
        }
    }
    internal class UVFlatter
    {
        public static bool Solve(Vector3[] vs, int[] tris, out Vector3[] vs1, out int[] tris1, out Vector2[] uv)
        {

            Vertex.Reset();
            Edge.Reset();
            Face.Reset();

            vs1 = null;
            tris1 = null;
            uv = null;
            List<Vertex> vertices = new List<Vertex>(vs.Length);
            List<Edge> edges = new List<Edge>();
            List<Face> faces = new List<Face>(tris.Length);

            int GetRoot(Vertex vertex)
            {
                Stack<Vertex> stack = new Stack<Vertex>();
                while (vertex.root != vertex.id)
                {
                    stack.Push(vertex);
                    vertex = vertices[vertex.root];
                }
                int root = vertex.root;
                while (stack.Count > 0)
                {
                    vertex = stack.Pop();
                    vertex.root = root;
                }
                return root;
            }

            UVIsland SolveIsland(Vertex vertex)
            {
                float minX = 0f;
                float minY = 0f;
                float maxX = vertex.poi.x;
                float maxY = vertex.poi.y;
                HashSet<int> visitedVertex = new HashSet<int>();
                Stack<Vertex> vertexStake = new();
                Vector3[] tmpCarrier = new Vector3[3];
                

                visitedVertex.Add(vertex.id);
                vertexStake.Push(vertex);

                vertex.uv = new Vector2(vertex.poi.x, vertex.poi.y);

                while (vertexStake.Count > 0)
                {
                    Vertex curVertex = vertexStake.Peek();
                    if (curVertex.visitedFaceCount >= curVertex.faces.Count)
                    {
                        vertexStake.Pop();
                        continue;
                    }
                    Face curFace = curVertex.faces[curVertex.visitedFaceCount++];
                    curFace.GetVertex(curVertex, out Vertex v0, out Vertex v1);
                    int vs0 = curFace.GetVertexSequence(v0);
                    int vs1 = curFace.GetVertexSequence(v1);
                    int vs2 = curFace.GetVertexSequence(curVertex);

                    if (visitedVertex.Contains(v0.id) && visitedVertex.Contains(v1.id))
                    {
                        var _vec0 = v0.uv;
                        var _vec1 = v1.uv;

                        tmpCarrier[vs0] = _vec0;
                        tmpCarrier[vs1] = _vec1;
                        tmpCarrier[vs2] = curVertex.uv;

                        var normal = Vector3.Cross(tmpCarrier[1] - tmpCarrier[0], tmpCarrier[2] - tmpCarrier[1]);
                        if (normal.z < 0)
                        {

                        }
                    }

                    else if (visitedVertex.Contains(v0.id) || visitedVertex.Contains(v1.id))
                    {

                    }
                    else
                    {
                        var _vec0 = v0.poi - curVertex.poi - curVertex.uv;
                        var _vec1 = v1.poi - curVertex.poi - curVertex.uv;

                        tmpCarrier[vs0] = _vec0;
                        tmpCarrier[vs1] = _vec1;
                        tmpCarrier[vs2] = curVertex.uv;

                        var normal = Vector3.Cross(tmpCarrier[1] - tmpCarrier[0], tmpCarrier[2] - tmpCarrier[1]);
                        if (Math.Abs(normal.z) < 1e-5)
                        {

                        }
                        else if (normal.z < 0)
                        {

                        }


                    }

                    //minX = Math.Min(minX, vertex.poi.x);
                    //minY = Math.Min (minY, vertex.poi.y);
                    //maxX = Math.Max(maxX, vertex.poi.x);
                    //maxY = Math.Max (maxY, vertex.poi.y);

                    

                }

                List<KeyValuePair<int, Vector2>> subUV = new List<KeyValuePair<int, Vector2>>();
                return new UVIsland(subUV, minX, minY, maxX - minX, maxY - minY, maxX, maxY);
            }


            //Step1, initialize and use union to get uv island info
            for(int i = 0; i < vs.Length; i++)
            {
                var tmp = new Vertex(vs[i]);
                vertices[i] = tmp;
            }
            for(int i = 0;i < tris.Length; i += 3)
            {
                Vertex v0 = vertices[tris[i]];
                Vertex v1 = vertices[tris[i + 1]];
                Vertex v2 = vertices[tris[i + 2]];

                if (!v0.edges.TryGetValue(v1.id, out Edge e01))
                {
                    e01 = new Edge(v0, v1);
                    v0.edges.Add(v1.id, e01);
                    v1.edges.Add(v0.id, e01);
                    edges.Add(e01);
                }
                if (!v1.edges.TryGetValue(v2.id, out Edge e12))
                {
                    e12 = new Edge(v1, v2);
                    v1.edges.Add(v2.id, e12);
                    v2.edges.Add(v1.id, e12);
                    edges.Add(e12);
                }
                if (!v2.edges.TryGetValue(v2.id, out Edge e20))
                {
                    e20 = new Edge(v2, v0);
                    v2.edges.Add(v0.id, e20);
                    v0.edges.Add(v2.id, e20);
                    edges.Add(e20);
                }
                Face tmp = new Face(v0, v1, v2, e01, e12, e20);
                faces.Add(tmp);
                v0.faces.Add(tmp);
                v1.faces.Add(tmp);
                v2.faces.Add(tmp);

                int root = GetRoot(v0);
                vertices[GetRoot(v1)].root = root;
                vertices[GetRoot(v2)].root = root;
            }

            //Step2, modify mesh.If there are more than two faces link to an edge, split the edge.
            int count = edges.Count;//Ignore the new added
            for(int i = 0; i < count; i++)
            {
                var edge = edges[i];
                while (edge.Split(out var v0, out var v1, out var e))
                {
                    vertices.Add(v0);
                    vertices.Add(v1);
                    edges.Add(e);
                }
            }

            //Step3, process all uv islands
            count = vertices.Count;
            List<int> vistedRoot = new List<int>();
            List<UVIsland> islands = new List<UVIsland>();
            for(int i = 0; i < count; i++)
            {
                if (!vistedRoot.Contains(vertices[i].root))
                {
                    vistedRoot.Add(vertices[i].root);
                    islands.Add(SolveIsland(vertices[i]));
                }
            }

            //Step4 arrange these uv islands


            //Step5 scale and remap coordinate to 0 and 1


            return vs.Length != vertices.Count;

        }
    }
}
//If there is a circuit-like struct(like circle in 2-dimensioin space), split the vertex
