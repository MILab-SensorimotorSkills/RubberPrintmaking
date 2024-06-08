using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiggingTest;
using TMPro;

public class CsvSystemWithStart : MonoBehaviour
{
    #region Object
    public TMP_Text startTime;
    public GameObject textBox;
    public GameObject rubber;
    private float rubberHeight;
    private float endPointHeight;
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
    private float? depth;
    //도안으로부터 떨어진 정도
    private float? minDistance;
    private float hitY; //Knife's Sovel hitPoint
    private float hitX;
    private float hitZ;
    private Vector3? hitPoint;
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
            CulculatePlayTime();
            CountFrame();
            SaveHitPoint();
            CulculateDepth();
            CulculateDistance();

            //데이터를 작성하는 중
            csv.WriteData(playTime, frameCount, minDistance, depth);
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
        //데이터를 쓰기 시작해야하는 시점
        WriteData = true;
    }

    void CountFrame()
    {
        frameCount++;
    }

    void CulculatePlayTime()
    {
        playTime += Time.deltaTime;
    }

    void CulculateDepth()
    {
        if (knifeHeight <= -0.141)
        {
            hitY = hitPoint.Value.y;
            depth = hitY - rubberHeight;
        }
        else
        {
            depth = null;
        }
    }

    void CulculateDistance()
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
                    float distance = GetShortestDistanceToMesh2D(hitPoint.Value, meshFilter);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestObject = target;
                    }
                }
            }

            /*if (closestObject != null)
            {
                Debug.Log("Shortest distance to any mesh: " + minDistance + " (Object: " + closestObject.name + ")");
            }*/
        }else{
            minDistance = null;
        }
    }

    float GetShortestDistanceToMesh2D(Vector3 point, MeshFilter meshFilter)
    {
        Mesh mesh = meshFilter.mesh;
        Transform meshTransform = meshFilter.transform;

        float minDistance = float.MaxValue;

        Vector2 point2D = new Vector2(point.x, point.z);

        foreach (Vector3 vertex in mesh.vertices)
        {
            Vector3 worldVertex = meshTransform.TransformPoint(vertex);
            Vector2 vertex2D = new Vector2(worldVertex.x, worldVertex.z);
            float distance = Vector2.Distance(point2D, vertex2D);

            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    void SaveHitPoint()
    {
        hitPoint = knifeShovel.hitPoint;
        knifeHeight = knife.transform.position.y;
        //Debug.Log(hitPoint);
    }
}
