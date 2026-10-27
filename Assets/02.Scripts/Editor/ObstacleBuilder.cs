using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 이동/회피가 잘 보이도록 바닥 곳곳에 큐브(파란 계열)와 콘(노란색) 장애물을 배치한다.
public static class ObstacleBuilder
{
    private const string RootName = "Obstacles";
    private const string MaterialFolder = "Assets/09.Materials";
    private const string ModelFolder = "Assets/05.Models";

    private static readonly Color[] CubeColors =
    {
        new Color(0.12f, 0.35f, 0.85f),
        new Color(0.20f, 0.60f, 0.95f),
        new Color(0.10f, 0.20f, 0.55f),
        new Color(0.35f, 0.75f, 0.90f),
    };

    private static readonly Color ConeColor = new Color(1f, 0.82f, 0.10f);

    // 아레나 범위 (벽 안쪽)
    private const float MinX = -46f;
    private const float MaxX = 46f;
    private const float MinZ = -51f;
    private const float MaxZ = 41f;
    private const float Spacing = 6f;

    private static readonly Vector3 PlayerStart = new Vector3(3.1f, 0f, -3.6f);
    private const float PlayerClearRadius = 4f;

    [MenuItem("Tools/Build Obstacles")]
    public static void Build()
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null)
        {
            Object.DestroyImmediate(old);
        }

        List<Material> cubeMaterials = new List<Material>();
        for (int i = 0; i < CubeColors.Length; i++)
        {
            cubeMaterials.Add(GetOrCreateMaterial($"ObstacleCube_{i}", CubeColors[i]));
        }

        Material coneMaterial = GetOrCreateMaterial("ObstacleCone", ConeColor);
        Mesh coneMesh = GetOrCreateConeMesh();

        GameObject root = new GameObject(RootName);
        Random.State prevState = Random.state;
        Random.InitState(1234);

        int index = 0;
        for (float x = MinX; x <= MaxX; x += Spacing)
        {
            for (float z = MinZ; z <= MaxZ; z += Spacing)
            {
                Vector3 pos = new Vector3(
                    x + Random.Range(-2f, 2f),
                    0f,
                    z + Random.Range(-2f, 2f));

                if (Vector3.Distance(pos, PlayerStart) < PlayerClearRadius)
                {
                    continue;
                }

                // 약간 비워서 너무 빽빽하지 않게
                if (Random.value < 0.2f)
                {
                    continue;
                }

                if (Random.value < 0.55f)
                {
                    CreateCube(root.transform, pos, cubeMaterials[Random.Range(0, cubeMaterials.Count)], index);
                }
                else
                {
                    CreateCone(root.transform, pos, coneMesh, coneMaterial, index);
                }

                index++;
            }
        }

        Random.state = prevState;

        EditorSceneManager.MarkSceneDirty(root.scene);
        EditorSceneManager.SaveScene(root.scene);
        Debug.Log($"[ObstacleBuilder] {index} obstacles placed");
    }

    private static void CreateCube(Transform parent, Vector3 pos, Material material, int index)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = $"Cube_{index}";
        cube.transform.SetParent(parent);

        float size = Random.Range(0.8f, 1.8f);
        float height = Random.Range(0.8f, 2.2f);
        cube.transform.localScale = new Vector3(size, height, size);
        cube.transform.position = pos + Vector3.up * (height * 0.5f);
        cube.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 90f), 0f);
        cube.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static void CreateCone(Transform parent, Vector3 pos, Mesh mesh, Material material, int index)
    {
        GameObject cone = new GameObject($"Cone_{index}");
        cone.transform.SetParent(parent);

        float radius = Random.Range(0.8f, 1.3f);
        float height = Random.Range(1.2f, 2.2f);
        cone.transform.position = pos;
        cone.transform.localScale = new Vector3(radius, height, radius);

        cone.AddComponent<MeshFilter>().sharedMesh = mesh;
        cone.AddComponent<MeshRenderer>().sharedMaterial = material;

        MeshCollider meshCollider = cone.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;
    }

    private static Material GetOrCreateMaterial(string name, Color color)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(material);
        return material;
    }

    // 높이 1, 밑면 지름 1, 피벗이 바닥 중심인 콘 메시
    private static Mesh GetOrCreateConeMesh()
    {
        string path = $"{ModelFolder}/Cone.asset";
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh != null)
        {
            return mesh;
        }

        const int segments = 32;
        const float radius = 0.5f;
        const float height = 1f;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        float slope = radius / height;
        for (int i = 0; i < segments; i++)
        {
            float a0 = Mathf.PI * 2f * i / segments;
            float a1 = Mathf.PI * 2f * (i + 1) / segments;
            float am = (a0 + a1) * 0.5f;

            Vector3 p0 = new Vector3(Mathf.Cos(a0) * radius, 0f, Mathf.Sin(a0) * radius);
            Vector3 p1 = new Vector3(Mathf.Cos(a1) * radius, 0f, Mathf.Sin(a1) * radius);
            Vector3 top = new Vector3(0f, height, 0f);

            // 옆면
            int start = vertices.Count;
            vertices.Add(p0);
            vertices.Add(top);
            vertices.Add(p1);
            normals.Add(new Vector3(Mathf.Cos(a0), slope, Mathf.Sin(a0)).normalized);
            normals.Add(new Vector3(Mathf.Cos(am), slope, Mathf.Sin(am)).normalized);
            normals.Add(new Vector3(Mathf.Cos(a1), slope, Mathf.Sin(a1)).normalized);
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);

            // 밑면
            start = vertices.Count;
            vertices.Add(Vector3.zero);
            vertices.Add(p0);
            vertices.Add(p1);
            normals.Add(Vector3.down);
            normals.Add(Vector3.down);
            normals.Add(Vector3.down);
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
        }

        mesh = new Mesh { name = "Cone" };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();

        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }
}
