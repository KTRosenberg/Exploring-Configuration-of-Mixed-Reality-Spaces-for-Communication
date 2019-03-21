using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshContent {

    public class MeshData {
        public int ID;
        public int subID;
        public int boardID;
        public short type;

        public Mesh mesh;
        public Matrix4x4 xform; // thinking of sending the whole matrix at some point instead

        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public Material mat;

        public Vector3[] vertices;
        public int[] triangles;

        public Vector3[] originalVertices;
        public int[] originalTriangles;
        public Vector3[] originalNormals;

        public Vector4[] tangents;
        public Vector3[] normals;
        public Vector2[] uvs;
    }

    public struct Triangle {
        public int v1;
        public int v2;
        public int v3;
    }

    public static MeshData CreateCubeMesh(int assetID)
    {
        Vector3[] vertices = new Vector3[6 * 4];
        Vector4[] tangents = new Vector4[6 * 4];
        Vector3[] normals = new Vector3[6 * 4];
        Vector2[] uvs = new Vector2[6 * 4];
        int[] triangles = new int[6 * 2 * 3];

        Vector3[,] faces = new Vector3[6, 3] {
                { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) },
                { new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0) },
                { new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0) },
                { new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 1, 0) },
                { new Vector3(0, -1, 0), new Vector3(0, 0, -1), new Vector3(1, 0, 0) },
                { new Vector3(-1, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 0, 1) },
            };

        Mesh mesh = new Mesh();

        for (int i = 0; i < 6; i++) {
            vertices[i * 4 + 0] = faces[i, 0] - faces[i, 1] - faces[i, 2];
            vertices[i * 4 + 1] = faces[i, 0] + faces[i, 1] - faces[i, 2];
            vertices[i * 4 + 2] = faces[i, 0] + faces[i, 1] + faces[i, 2];
            vertices[i * 4 + 3] = faces[i, 0] - faces[i, 1] + faces[i, 2];

            triangles[i * 6 + 0] = i * 4 + 0;
            triangles[i * 6 + 1] = i * 4 + 1;
            triangles[i * 6 + 2] = i * 4 + 2;

            triangles[i * 6 + 3] = i * 4 + 3;
            triangles[i * 6 + 4] = i * 4 + 0;
            triangles[i * 6 + 5] = i * 4 + 2;

            tangents[i * 4 + 0] = faces[i, 1];
            tangents[i * 4 + 1] = faces[i, 1];
            tangents[i * 4 + 2] = faces[i, 1];
            tangents[i * 4 + 3] = faces[i, 1];

            normals[i * 4 + 0] = faces[i, 0];
            normals[i * 4 + 1] = faces[i, 0];
            normals[i * 4 + 2] = faces[i, 0];
            normals[i * 4 + 3] = faces[i, 0];

            uvs[i * 4 + 0] = new Vector2(0, 0);
            uvs[i * 4 + 1] = new Vector2(1, 0);
            uvs[i * 4 + 2] = new Vector2(1, 1);
            uvs[i * 4 + 3] = new Vector2(0, 1);
        }

        mesh.vertices = vertices;
        mesh.tangents = tangents;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

        return new MeshData {
            mesh = mesh,
            xform = Matrix4x4.identity,
            ID = assetID,
            originalVertices = null,
            originalTriangles = null,
            originalNormals = null,
            vertices = vertices,
            triangles = triangles,
            tangents = tangents,
            normals = normals,
            uvs = uvs
        };
    }

    public static MeshData CreatePolyhedronMesh(int assetID, int subID, bool isDynamic, Vector3[] vertices, int[] triangles, Vector3[] normals = default(Vector3[]), 
        Vector3 position = default(Vector3), Vector3 rotation = default(Vector3), float scale = 1.0f)
    {
        List<Vector3> finalVertexList = new List<Vector3>();
        List<Vector3> finalNormalList = new List<Vector3>();
        List<Vector4> finalTangentList = new List<Vector4>();
        List<Vector2> finalUVList = new List<Vector2>();
        int parity = 1;

        Debug.Assert((triangles.Length % 3) == 0);

        for (int t = 0; t < triangles.Length; t += 3) {
            var tri = new Triangle();
            tri.v1 = triangles[t];
            tri.v2 = triangles[t + 1];
            tri.v3 = triangles[t + 2];

            AddTriangle(ref tri, vertices, triangles, normals, parity, finalVertexList, finalNormalList,
                        finalTangentList, finalUVList);
            parity = 1 - parity;
        }

        Mesh mesh = new Mesh();
        if (isDynamic) {
            mesh.MarkDynamic();
        }



        Vector3[] _vertices = finalVertexList.ToArray();
        Vector3[] _normals = finalNormalList.ToArray();
        Vector4[] _tangents = finalTangentList.ToArray();
        Vector2[] _uvs = finalUVList.ToArray();

        mesh.vertices = _vertices;
        mesh.normals = _normals;
        mesh.tangents = _tangents;
        mesh.uv = _uvs;



        int triComponentCount = triangles.Length;
        int[] indexList = new int[triComponentCount * 2];
        for (int i = 0; i < triComponentCount; i += 1) {
            indexList[i] = i;
        }
        for (int i = 0; i < triComponentCount; i += 1) {
            indexList[triComponentCount + i] = triComponentCount - i - 1;
        }
        mesh.triangles = indexList;

        return new MeshData {
            mesh = mesh,
            xform = Matrix4x4.identity,
            ID = assetID,
            subID = subID,
            originalVertices = vertices,
            originalTriangles = triangles,
            originalNormals = normals,
            vertices = _vertices,
            triangles = indexList,
            tangents = _tangents,
            normals = _normals,
            uvs = _uvs
        };
    }

    private static void AddTriangle(ref Triangle t, Vector3[] vertices, int[] triangles, Vector3[] normals, int parity,
                             List<Vector3> vertexListToAddTo, List<Vector3> normalListToAddTo,
                             List<Vector4> tangentListToAddTo, List<Vector2> uvListToAddTo)
    {
        Vector3 a = vertices[t.v1];
        Vector3 b = vertices[t.v2];
        Vector3 c = vertices[t.v3];
        Vector4 tangent = (b - a).normalized;
        Vector3 genNormal = Vector3.Cross(tangent, c - b).normalized;

        vertexListToAddTo.Add(a);
        vertexListToAddTo.Add(b);
        vertexListToAddTo.Add(c);

        int n = normalListToAddTo.Count;

        if (normals == null) {
            normalListToAddTo.Add(genNormal);
            normalListToAddTo.Add(genNormal);
            normalListToAddTo.Add(genNormal);
        }
        else {
            normalListToAddTo.Add(normals.Length > n ? normals[n].normalized : genNormal);
            normalListToAddTo.Add(normals.Length > n + 1 ? normals[n + 1].normalized : genNormal);
            normalListToAddTo.Add(normals.Length > n + 2 ? normals[n + 2].normalized : genNormal);
        }

        tangentListToAddTo.Add(tangent);
        tangentListToAddTo.Add(tangent);
        tangentListToAddTo.Add(tangent);

        uvListToAddTo.Add(new Vector2(parity, parity));
        uvListToAddTo.Add(new Vector2(parity, 1 - parity));
        uvListToAddTo.Add(new Vector2(1 - parity, 1 - parity));
    }

    public static MeshData UpdatePolyhedronMeshData(MeshContent.MeshData meshData, Vector3[] vertices, int[] triangles, Vector3[] normals = default(Vector3[]),
    Vector3 position = default(Vector3), Vector3 rotation = default(Vector3), float scale = 1.0f)
    {
        List<Vector3> finalVertexList = new List<Vector3>();
        List<Vector3> finalNormalList = new List<Vector3>();
        List<Vector4> finalTangentList = new List<Vector4>();
        List<Vector2> finalUVList = new List<Vector2>();
        int parity = 1;

        Debug.Assert((triangles.Length % 3) == 0);

        for (int t = 0; t < triangles.Length; t += 3) {
            var tri = new Triangle();
            tri.v1 = triangles[t];
            tri.v2 = triangles[t + 1];
            tri.v3 = triangles[t + 2];

            AddTriangle(ref tri, vertices, triangles, normals, parity, finalVertexList, finalNormalList,
                        finalTangentList, finalUVList);
            parity = 1 - parity;
        }

        Vector3[] _vertices = finalVertexList.ToArray();
        Vector3[] _normals = finalNormalList.ToArray();
        Vector4[] _tangents = finalTangentList.ToArray();
        Vector2[] _uvs = finalUVList.ToArray();

        meshData.originalVertices = vertices;
        meshData.originalTriangles = triangles;
        meshData.originalNormals = normals;
        meshData.tangents = _tangents;
        meshData.uvs = _uvs;

        meshData.mesh.vertices = _vertices;
        meshData.mesh.normals = _normals;
        meshData.mesh.tangents = _tangents;
        meshData.mesh.uv = _uvs;

        int triComponentCount = triangles.Length;
        int[] indexList = new int[triComponentCount * 2];
        for (int i = 0; i < triComponentCount; i += 1) {
            indexList[i] = i;
        }
        for (int i = 0; i < triComponentCount; i += 1) {
            indexList[triComponentCount + i] = triComponentCount - i - 1;
        }
        meshData.triangles = indexList;
        meshData.mesh.triangles = indexList;

        return meshData;
    }

    public static Dictionary<int, MeshContent.MeshData> idToMeshMap = new Dictionary<int, MeshContent.MeshData>();
    public static Dictionary<int, MeshGO> idToMeshGOMap = new Dictionary<int, MeshGO>();
    public static Dictionary<int, List<MeshContent.MeshData>> idToMeshesMap = new Dictionary<int, List<MeshContent.MeshData>>(); // TODO
    public static Queue<int> needToUpdateQ = new Queue<int>();


    public static Queue<MeshContent.MeshData> activeMeshData = new Queue<MeshContent.MeshData>();
}
