using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualWall : MonoBehaviour
{
    public GameObject prefab; // 생성할 프리팹 오브젝트를 설정합니다.
    public GameObject referenceObject; // y 좌표 비교를 위한 참조 오브젝트
    private VirtualKnife virtualKnife;
    private AdvancedPhysicsHapticEffector advancedHapticEffector;
    private bool hasSpawned = false; // 프리팹 생성 여부를 추적
    private float virtualObjectInitialY; // 버추얼오브젝트의 초기 Y값
    private List<GameObject> spawnedPrefabs = new List<GameObject>(); // 생성된 프리팹 목록

    public Vector3 frontPositionOffset = Vector3.forward;
    public Vector3 backPositionOffset = Vector3.back;
    public Vector3 leftPositionOffset = Vector3.left;
    public Vector3 rightPositionOffset = Vector3.right;

    public Quaternion frontRotation = Quaternion.identity;
    public Quaternion backRotation = Quaternion.identity;
    public Quaternion leftRotation = Quaternion.Euler(0, 90, 0);
    public Quaternion rightRotation = Quaternion.Euler(0, 90, 0);

    // Start is called before the first frame update
    void Start()
    {
        virtualKnife = GetComponent<VirtualKnife>();
        advancedHapticEffector = GetComponent<AdvancedPhysicsHapticEffector>();

        if (virtualKnife != null && virtualKnife.virtualObject != null)
        {
            virtualObjectInitialY = referenceObject.transform.position.y;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (virtualKnife != null && prefab != null && referenceObject != null && advancedHapticEffector != null)
        {
            float mainForce = advancedHapticEffector.MainForce;
            float referenceObjectY = referenceObject.transform.position.y;

            if (virtualKnife != null)
            {
                float virtualObjectY = virtualKnife.transform.position.y;

                if (virtualObjectY < virtualObjectInitialY && mainForce >= 2f && !hasSpawned)
                {
                    SpawnPrefabs();
                    Debug.Log("Prefab Spawned");
                    hasSpawned = true; // 프리팹이 생성되었음을 표시
                }
                else if (virtualObjectY > virtualObjectInitialY && hasSpawned)
                {
                    DespawnPrefabs();
                    Debug.Log("Prefab Despawned");
                    hasSpawned = false; // 프리팹이 제거되었음을 표시
                }
            }
        }
    }

    void SpawnPrefabs()
    {
        float xMin = transform.position.x - (transform.localScale.x / 2) - (prefab.transform.localScale.x / 2);
        float xMax = transform.position.x + (transform.localScale.x / 2) + (prefab.transform.localScale.x / 2);
        float zMin = transform.position.z - (transform.localScale.z / 2) - (prefab.transform.localScale.z / 2);
        float zMax = transform.position.z + (transform.localScale.z / 2) + (prefab.transform.localScale.z / 2);

        Vector3 frontPosition = new Vector3(transform.position.x, transform.position.y, zMax) + frontPositionOffset;
        Vector3 backPosition = new Vector3(transform.position.x, transform.position.y, zMin) + backPositionOffset;
        Vector3 leftPosition = new Vector3(xMin, transform.position.y, transform.position.z) + leftPositionOffset;
        Vector3 rightPosition = new Vector3(xMax, transform.position.y, transform.position.z) + rightPositionOffset;

        // 앞에 프리팹 생성
        spawnedPrefabs.Add(Instantiate(prefab, frontPosition, frontRotation));
        // 뒤에 프리팹 생성
        spawnedPrefabs.Add(Instantiate(prefab, backPosition, backRotation));
        // 왼쪽에 프리팹 생성
        spawnedPrefabs.Add(Instantiate(prefab, leftPosition, leftRotation));
        // 오른쪽에 프리팹 생성
        spawnedPrefabs.Add(Instantiate(prefab, rightPosition, rightRotation));
    }

    void DespawnPrefabs()
    {
        foreach (GameObject spawnedPrefab in spawnedPrefabs)
        {
            Destroy(spawnedPrefab);
        }
        spawnedPrefabs.Clear();
    }
}
