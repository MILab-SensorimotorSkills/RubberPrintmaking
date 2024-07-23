using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DiggingTest;

public class CsvSystemWithStart : MonoBehaviour
{
    #region Object
    public TMP_Text startTime;
    public GameObject textBox;
    public GameObject rubber;
    private float rubberHeight;
    public GameObject knife;
    private Shovel knifeShovel;
    public List<GameObject> targetObjects;
    private float knifeHeight;
    #endregion

    private CSV_Making csv;
    private int count = 3;
    private bool WriteData = false;

    #region Data
    private float playTime;
    private int frameCount;
    private float depth;
    private float minDistance;
    private float hitY;
    private float hitX;
    private float hitZ;
    private Vector3? hitPoint;
    private float disAvg;
    private float disVar;
    private float depthAvg;
    private float depthVar;
    private float disAvg0x;
    private float disVar0x;
    private float depthAvg0x;
    private float depthVar0x;
    private List<float> distances = new List<float>();
    private List<float> depths = new List<float>();
    private List<float> distances0x = new List<float>();
    private List<float> depths0x = new List<float>();
    #endregion

    void Start()
    {
        csv = GetComponent<CSV_Making>();
        knifeShovel = knife.GetComponent<Shovel>();
        rubberHeight = rubber.transform.position.y;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && !WriteData)
        {
            //변수 초기화
            playTime = 0f;
            frameCount = 0;
            depth = 0f;
            minDistance = 0;
            distances.Clear();
            depths.Clear();
            distances0x.Clear();
            depths0x.Clear();

            StartCoroutine(CountdownCoroutine());
        }

        //데이터 쓰기를 멈추는 시점
        if (Input.GetKeyDown(KeyCode.Q) && WriteData)
        {
            WriteData = false;
            startTime.text = "End";
            textBox.SetActive(true);

        }

        if (WriteData)
        {
            CalculatePlayTime();
            CountFrame();
            SaveHitPoint();
            CalculateDepth();
            CalculateDistance();

            // 평균과 분산 계산
            disAvg = CalculateMean(distances);
            depthAvg = CalculateMean(depths);
            disAvg0x = CalculateMean(distances0x);
            depthAvg0x = CalculateMean(depths0x);

            disVar = CalculateVar(distances, disAvg);
            depthVar = CalculateVar(depths, depthAvg);
            disVar0x = CalculateVar(distances0x, disAvg0x);
            depthVar0x = CalculateVar(depths0x, depthAvg0x);

            // 데이터를 기록
            csv.WriteData(playTime, frameCount, minDistance, depth, disAvg, disVar, depthAvg, depthVar, disAvg0x, disVar0x, depthAvg0x, depthVar0x);
        }
    }

    IEnumerator CountdownCoroutine()
    {
        while (count >= 0)
        {
            switch (count)
            {
                case 3:
                case 2:
                case 1:
                    startTime.text = count.ToString();
                    break;
                case 0:
                    startTime.text = "Start";
                    break;
            }
            count--;
            yield return new WaitForSeconds(1);
        }
        textBox.SetActive(false);
        // 데이터 쓰기 시작
        WriteData = true;
        // Header 추가
        csv.WriteCol();
    }

    void CountFrame()
    {
        frameCount++;
    }

    void CalculatePlayTime()
    {
        playTime += Time.deltaTime;
    }

    void CalculateDepth()
    {
        if (knifeHeight <= -0.141)
        {
            hitY = hitPoint.Value.y;
            depth = hitY - rubberHeight;
        }
        else
        {
            depth = 0;
        }

        depths.Add(depth);
        if (depth != 0)
        {
            depths0x.Add(depth);
        }
    }

    void CalculateDistance()
    {
        if (knifeHeight <= -0.141)
        {
            minDistance = float.MaxValue;
            GameObject closestObject = null;

            foreach (GameObject target in targetObjects)
            {
                MeshFilter meshFilter = target.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    float distance = MeshDistanceCalculator.GetShortestDistanceToMesh(hitPoint.Value, meshFilter);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestObject = target;
                    }
                }
            }

            if (minDistance != 0)
            {
                distances0x.Add(minDistance);
            }

            if (closestObject != null)
            {
                //Debug.Log("Shortest distance to any mesh: " + minDistance + " (Object: " + closestObject.name + ")");
            }
        }
        else
        {
            minDistance = 0;
        }
        distances.Add(minDistance);
    }

    void SaveHitPoint()
    {
        hitPoint = knifeShovel.hitPoint;
        knifeHeight = knife.transform.position.y;
    }

    float CalculateMean(List<float> values){
        float sum = 0f;

        foreach (float value in values)
        {
            sum += value;
        }

        return values.Count > 0 ? sum / values.Count : 0;
    }

    float CalculateVar(List<float> values, float mean){
        float sumOfSquares = 0f;

        foreach (float value in values)
        {
            float difference = value - mean;
            sumOfSquares += difference * difference;
        }

        return values.Count > 1 ? sumOfSquares / (values.Count - 1) : 0;
    }
}


public static class MeshDistanceCalculator
{
    public static float PointTriangleDistance(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
    {
        // Convert to xz plane
        Vector2 pointXZ = new Vector2(point.x, point.z);
        Vector2 aXZ = new Vector2(a.x, a.z);
        Vector2 bXZ = new Vector2(b.x, b.z);
        Vector2 cXZ = new Vector2(c.x, c.z);

        // Compute vectors
        Vector2 ab = bXZ - aXZ;
        Vector2 ac = cXZ - aXZ;
        Vector2 ap = pointXZ - aXZ;

        // Compute dot products
        float d1 = Vector2.Dot(ab, ap);
        float d2 = Vector2.Dot(ac, ap);
        if (d1 <= 0.0f && d2 <= 0.0f)
            return Vector2.Distance(pointXZ, aXZ); // Barycentric coordinates (1,0,0)

        // Check if P in vertex region outside B
        Vector2 bp = pointXZ - bXZ;
        float d3 = Vector2.Dot(ab, bp);
        float d4 = Vector2.Dot(ac, bp);
        if (d3 >= 0.0f && d4 <= d3)
            return Vector2.Distance(pointXZ, bXZ); // Barycentric coordinates (0,1,0)

        // Check if P in edge region of AB, if so return projection of P onto AB
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
        {
            float v = d1 / (d1 - d3);
            return Vector2.Distance(pointXZ, aXZ + v * ab); // Barycentric coordinates (1-v,v,0)
        }

        // Check if P in vertex region outside C
        Vector2 cp = pointXZ - cXZ;
        float d5 = Vector2.Dot(ab, cp);
        float d6 = Vector2.Dot(ac, cp);
        if (d6 >= 0.0f && d5 <= d6)
            return Vector2.Distance(pointXZ, cXZ); // Barycentric coordinates (0,0,1)

        // Check if P in edge region of AC, if so return projection of P onto AC
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
        {
            float w = d2 / (d2 - d6);
            return Vector2.Distance(pointXZ, aXZ + w * ac); // Barycentric coordinates (1-w,0,w)
        }

        // Check if P in edge region of BC, if so return projection of P onto BC
        float va = d3 * d6 - d5 * d4;
        if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return Vector2.Distance(pointXZ, bXZ + w * (cXZ - bXZ)); // Barycentric coordinates (0,1-w,w)
        }

        // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
        float denom = 1.0f / (va + vb + vc);
        float vU = vb * denom;
        float vV = vc * denom;
        float vW = va * denom;
        Vector2 q = vU * aXZ + vV * bXZ + vW * cXZ;
        return Vector2.Distance(pointXZ, q);
    }

    public static float GetShortestDistanceToMesh(Vector3 point, MeshFilter meshFilter)
    {
        Mesh mesh = meshFilter.mesh;
        Transform meshTransform = meshFilter.transform;

        float minDistance = float.MaxValue;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = meshTransform.TransformPoint(vertices[triangles[i]]);
            Vector3 b = meshTransform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 c = meshTransform.TransformPoint(vertices[triangles[i + 2]]);

            float distance = PointTriangleDistance(point, a, b, c);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }
}