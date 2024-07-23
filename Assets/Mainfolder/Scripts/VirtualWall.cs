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

    private Vector3 backPositionOffset = Vector3.zero;
    private Vector3 leftPositionOffset = Vector3.zero;
    private Vector3 rightPositionOffset = Vector3.zero;

    public Quaternion backRotation = Quaternion.Euler(0, 0, 0);
    public Quaternion leftRotation = Quaternion.Euler(0, 90, 0);
    public Quaternion rightRotation = Quaternion.Euler(0, 90, 0);

    private GameObject backPrefab;
    private GameObject leftPrefab;
    private GameObject rightPrefab;

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
            float minimumY = virtualKnife.GetMinimumY();
            
            if (virtualKnife != null)
            {
                float virtualObjectY = virtualKnife.transform.position.y;

                if ((virtualObjectY < virtualObjectInitialY && mainForce >= 2f && !hasSpawned) 
                || (virtualObjectY < virtualObjectInitialY && minimumY<referenceObjectY && !hasSpawned))
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

                CheckAndRespawnWalls();

                // 레이캐스트로 충돌 감지
                RaycastHit hit;
                Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
                Debug.DrawRay(transform.position, forward, Color.red);

                if (Physics.Raycast(transform.position, transform.forward, out hit, 10f))
                {
                    GameObject hitObject = hit.collider.gameObject;

                    if ((hitObject.CompareTag("wall") || hitObject.layer == LayerMask.NameToLayer("wall")) && mainForce >= 5f)
                    {
                        MoveObject(hitObject, transform.forward);
                    }
                }
                else if (mainForce >= 5f)
                {
                    // 레이에 맞는 wall 프리팹이 없을 경우, 새로운 프리팹을 생성
                    Vector3 spawnPosition = transform.position + forward.normalized * 0.02f; // 오브젝트 바로 앞에 생성
                    GameObject newWall = Instantiate(prefab, spawnPosition, Quaternion.identity);
                    newWall.tag = "wall";
                    spawnedPrefabs.Add(newWall);
                    Debug.Log("New Wall Prefab Spawned");

                    MoveObject(newWall, transform.forward);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 10);
    }

    void SpawnPrefabs()
    {
        //어디를 중심으로 생성하는지 0.02 
        float offset = 0.02f;

        Vector3 backPosition = transform.position + new Vector3(0, 0, -offset) + backPositionOffset;
        Vector3 leftPosition = transform.position + new Vector3(-offset, 0, 0) + leftPositionOffset;
        Vector3 rightPosition = transform.position + new Vector3(offset, 0, 0) + rightPositionOffset;

        // 앞에 프리팹 생성

        // 뒤에 프리팹 생성
        backPrefab = Instantiate(prefab, backPosition, backRotation);
        spawnedPrefabs.Add(backPrefab);
        // 왼쪽에 프리팹 생성
        leftPrefab = Instantiate(prefab, leftPosition, leftRotation);
        spawnedPrefabs.Add(leftPrefab);
        // 오른쪽에 프리팹 생성
        rightPrefab = Instantiate(prefab, rightPosition, rightRotation);
        spawnedPrefabs.Add(rightPrefab);
    }

    void DespawnPrefabs()
    {
        foreach (GameObject spawnedPrefab in spawnedPrefabs)
        {
            Destroy(spawnedPrefab);
        }
        spawnedPrefabs.Clear();
        backPrefab = null;
        leftPrefab = null;
        rightPrefab = null;
    }

    void CheckAndRespawnWalls()
    {
        //초기 0.02
        float respawnDistance = 0.05f;

        if (backPrefab != null && Vector3.Distance(transform.position, backPrefab.transform.position) > respawnDistance)
        {
            Destroy(backPrefab);
            backPrefab = Instantiate(prefab, transform.position + new Vector3(0, 0, -0.02f) + backPositionOffset, backRotation);
            spawnedPrefabs.Add(backPrefab);
        }

        if (leftPrefab != null && Vector3.Distance(transform.position, leftPrefab.transform.position) > respawnDistance)
        {
            Destroy(leftPrefab);
            leftPrefab = Instantiate(prefab, transform.position + new Vector3(-0.02f, 0, 0) + leftPositionOffset, leftRotation);
            spawnedPrefabs.Add(leftPrefab);
        }

        if (rightPrefab != null && Vector3.Distance(transform.position, rightPrefab.transform.position) > respawnDistance)
        {
            Destroy(rightPrefab);
            rightPrefab = Instantiate(prefab, transform.position + new Vector3(0.02f, 0, 0) + rightPositionOffset, rightRotation);
            spawnedPrefabs.Add(rightPrefab);
        }
    }

    void MoveObject(GameObject obj, Vector3 direction)
    {
        if (obj != null)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.MovePosition(rb.position + direction * Time.deltaTime);
            }
        }
    }
}
