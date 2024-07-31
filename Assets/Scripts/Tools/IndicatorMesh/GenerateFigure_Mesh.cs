using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateFigure_Mesh : MonoBehaviour
{
    public enum Type
    {
        CIRCLE,
        SECTOR,
        RECTANGLE
    }
    public enum RenderType
    {
        Mesh,
        Gizmo,
    }

    [Header("[ RESOURCES ]")]
    public MeshFilter meshFilter;

    [Header("[ FIGURE TYPE ]")]
    public Type type;
    public RenderType renderType;

    [Header("[ INFO ]")]
    public int SEGMENTS = 50;
    public float radius = 3f; 
    public float degree = 360;
    public float radian;
    public Transform playerTr;
    public Transform detectedTr;

    [Header("[ VERTICS ]")]
    public Vector3[] vertices;
    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    void Update()
    {
        if (renderType != RenderType.Mesh)
            return;

        switch (type)
        {
            case Type.CIRCLE:
                DrawCircleMesh();
                break;
            case Type.SECTOR:
                DrawSectorMesh();
                break;
            case Type.RECTANGLE:
                break;
        }

        Debug.Log($"IsPointInSector - [{IsPointInSector(detectedTr.position)}]");
    }

    private void DrawCircleMesh()
    {
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;


        // 3D������ ��� ����(Vertex)�� ��ġ������ ���� �迭.
        // +1 �� �ϴ� ������ �߽����� �߰��� �����ϱ� ���ؼ�
        //vertices[0]: �߽����� ��ġ�� �����մϴ�.
        //vertices[1]���� vertices[segments]����: ���� �ѷ��� �����ϴ� �������� ��ġ�� �����մϴ�
        vertices = new Vector3[SEGMENTS + 1];

        // �ﰢ���� �޽��� �����ϴ� ���� �⺻���� ����
        int[] triangles = new int[SEGMENTS * 3];

        // �ﰢ���� �����ϴ� �ε��� �迭�Դϴ�. �� �ﰢ���� 3���� �������� �̷���� �ֽ��ϴ�.

        // �߽���
        vertices[0] = Vector3.zero;

        // ���� �� �� ���
        for (int i = 0; i < SEGMENTS; i++)
        {
            // ���� �� ���� �����Դϴ�. i�� �����Կ� ���� angle�� 0���� 2����� ����
            float angle = (float)i / SEGMENTS * 2 * Mathf.PI;
            // �ﰢ�Լ� sin�� ����Ͽ� x��ǥ�� ���
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            vertices[i + 1] = new Vector3(x, 0, z);
        }

        // �ﰢ�� ����
        for (int i = 0; i < SEGMENTS; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % SEGMENTS + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void DrawSectorMesh()
    {
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;


        CalculateVertices();

        int[] triangles = new int[SEGMENTS * 3];

        // �ﰢ�� ����
        for (int i = 0; i < SEGMENTS; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        // ������ ������ 0���� ���߾��ش�.
        triangles[(SEGMENTS - 1) * 3 + 2] = 0;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public bool IsPointInSector(Vector3 point)
    {
        return PointInVertices(point, vertices);
    }

    bool PointInVertices(Vector3 point, Vector3[] vertices)
    {
        int verticesLength = vertices.Length;
        bool inside = false;

        // detected point
        float pointX = point.x, pointZ = point.z;

        float startX, startZ, endX, endZ;

        // �߽��� ���� ����.
        Vector3 endPoint = vertices[verticesLength - 1];
        endX = endPoint.x;
        endZ = endPoint.z;

        for (int i = 0; i < verticesLength; i++)
        {
            startX = endX; 
            startZ = endZ;
            endPoint = vertices[i];
            endX = endPoint.x; 
            endZ = endPoint.z;

            bool condition1 = (endZ > pointZ ^ startZ > pointZ);
            bool condition2 = ((pointX - endX) < (pointZ - endZ) * (startX - endX) / (startZ - endZ));
            if (condition1 && condition2)
            {
                inside = !inside;
            }
        }
        return inside;
    }
    void CalculateVertices()
    {
        // ���� ����� vertices����
        // +2 �� ���ִ� ������ 0��° �ε����� ������ ������ ������ �ε����� 1��°
        vertices = new Vector3[SEGMENTS + 2];
        vertices[0] = playerTr.position;
        radian = degree * Mathf.Deg2Rad;
        for (int i = 0; i <= SEGMENTS; i++)
        {
            float angle = ((float)i / SEGMENTS) * ((degree / 180) * Mathf.PI);
            // �ﰢ�Լ� sin�� ����Ͽ� x��ǥ�� ���
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;

            Vector3 localPos = new Vector3(x, 0, z);
            Vector3 rotatedPos = playerTr.rotation * localPos;


            vertices[i + 1] = playerTr.position + rotatedPos;
        }
    }
    void OnDrawGizmos()
    {

        if (renderType != RenderType.Gizmo)
            return;
        Gizmos.color = Color.green;

        switch (type)
        {
            case Type.CIRCLE:
                CalculateVertices();
                for (int i = 0; i < SEGMENTS; i++)
                {
                    Gizmos.DrawLine(vertices[i + 1], vertices[i + 2]);

                }
                break;
            case Type.SECTOR:
                CalculateVertices();
                for (int i = 0; i < SEGMENTS; i++)
                {
                    Gizmos.DrawLine(vertices[i + 1], vertices[i + 2]);
                }
                Gizmos.DrawLine(vertices[0], vertices[1]);
                Gizmos.DrawLine(vertices[0], vertices[vertices.Length - 1]);
                break;
            case Type.RECTANGLE:
                break;
            default:
                break;
        }
       

        //if (playerTr != null)
        //{
        //    // AABB
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawWireCube(playerTr.GetComponent<Renderer>().bounds.center, playerTr.GetComponent<Renderer>().bounds.size);

        //    // OBB
        //    Gizmos.color = Color.green;
        //    DrawOBB(playerTr.gameObject);
        //}
    }

    void DrawOBB(GameObject obj)
    {
        var meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            var mesh = meshFilter.sharedMesh;
            if (mesh != null)
            {
                var vertices = mesh.vertices;
                var localToWorld = obj.transform.localToWorldMatrix;

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = localToWorld.MultiplyPoint3x4(vertices[i]);
                }

                var center = obj.transform.position;
                var extents = obj.GetComponent<Renderer>().bounds.extents;
                Gizmos.matrix = obj.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(center, extents * 2);
            }
        }
    }
}

