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


        // 3D공간의 모든 정점(Vertex)의 위치정보를 담을 배열.
        // +1 을 하는 이유는 중심점을 추가로 포함하기 위해서
        //vertices[0]: 중심점의 위치를 저장합니다.
        //vertices[1]부터 vertices[segments]까지: 원의 둘레를 구성하는 정점들의 위치를 저장합니다
        vertices = new Vector3[SEGMENTS + 1];

        // 삼각형은 메쉬를 구성하는 가장 기본적인 도형
        int[] triangles = new int[SEGMENTS * 3];

        // 삼각형을 정의하는 인덱스 배열입니다. 각 삼각형은 3개의 정점으로 이루어져 있습니다.

        // 중심점
        vertices[0] = Vector3.zero;

        // 원의 각 점 계산
        for (int i = 0; i < SEGMENTS; i++)
        {
            // 원의 각 점의 각도입니다. i가 증가함에 따라 angle도 0에서 2π까지 증가
            float angle = (float)i / SEGMENTS * 2 * Mathf.PI;
            // 삼각함수 sin을 사용하여 x좌표를 계산
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            vertices[i + 1] = new Vector3(x, 0, z);
        }

        // 삼각형 설정
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

        // 삼각형 설정
        for (int i = 0; i < SEGMENTS; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        // 마지막 정점은 0으로 맞추어준다.
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

        // 중심점 부터 시작.
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
        // 원을 기반한 vertices로직
        // +2 를 해주는 이유는 0번째 인덱스는 중점을 가지고 마지막 인덱스는 1번째
        vertices = new Vector3[SEGMENTS + 2];
        vertices[0] = playerTr.position;
        radian = degree * Mathf.Deg2Rad;
        for (int i = 0; i <= SEGMENTS; i++)
        {
            float angle = ((float)i / SEGMENTS) * ((degree / 180) * Mathf.PI);
            // 삼각함수 sin을 사용하여 x좌표를 계산
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

