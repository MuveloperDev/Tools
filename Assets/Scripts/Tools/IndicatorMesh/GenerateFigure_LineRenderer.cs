using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class GenerateFigure_LineRenderer : MonoBehaviour
{
    public enum Type
    {
        CIRCLE,
        SECTOR,
        RECTANGLE
    }

    [Header("[ RESOURCES ]")]
    public LineRenderer lineRenderer;

    [Header("[ FIGURE TYPE ]")]
    public Type type;

    [Header("[ INFO ]")]
    public float radius = 3f;
    public float degree = 360;
    public Transform playerTr;
    public Transform detectedTr;

    [Header("[ VERTICS ]")]
    Vector3[] vertices;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
    }
    void Update()
    {
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
    }

    private void DrawCircleMesh()
    {
        const int SEGMENTS = 50;
        lineRenderer.positionCount = SEGMENTS + 2; // 중심점 + 각도 분할 + 마지막 점 (중심점과 연결)

        // 정점
        vertices = new Vector3[SEGMENTS + 2];

        // 중심점
        vertices[0] = Vector3.zero;

        // 원의 각 점 계산
        for (int i = 0; i <= SEGMENTS; i++)
        {
            // 원의 각 점의 각도입니다. i가 증가함에 따라 angle도 0에서 2π까지 증가
            float angle = (float)i / SEGMENTS * 2 * Mathf.PI;
            // 삼각함수 sin을 사용하여 x좌표를 계산
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            vertices[i + 1] = new Vector3(x, 1, z);
        }
        vertices[vertices.Length - 1] = vertices[1];
        lineRenderer.SetPositions(vertices);
    }

    private void DrawSectorMesh()
    {
        const int SEGMENTS = 50;
        lineRenderer.positionCount = SEGMENTS + 2; // 중심점 + 각도 분할 + 마지막 점 (중심점과 연결)
        
        vertices = new Vector3[SEGMENTS + 2];

        // 중심점
        vertices[0] = playerTr.position;


        for (int i = 0; i <= SEGMENTS; i++)
        {
            // 부채꼴 각 점의 각도입니다. i가 증가함에 따라 angle도 0에서 2π까지 증가
            float angle = (float)i / SEGMENTS * (degree / 180) * Mathf.PI;
            // 삼각함수 sin을 사용하여 x좌표를 계산
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            vertices[i + 1] = new Vector3(playerTr.position.x + x, playerTr.position.y, playerTr.position.z + z);
        }

        //vertices[SEGMENTS + 1] = vertices[1];
        lineRenderer.SetPositions(vertices);
    }
}
