using UnityEngine;

public class DynamicMeshCollider : MonoBehaviour
{
    public MeshFilter meshFilter;
    private Mesh mesh;
    private BoxCollider[] subColliders;
    private int numSubColliders = 9;
    private Bounds[] subBounds;
    
    void Start()
    {
        mesh = meshFilter.mesh;
        InitializeSubColliders();
        UpdateSubColliders();
    }
    
    void InitializeSubColliders()
    {
        subColliders = new BoxCollider[numSubColliders];
        subBounds = new Bounds[numSubColliders];

        for (int i = 0; i < numSubColliders; i++)
        {
            GameObject colliderObj = new GameObject("SubCollider" + i);
            colliderObj.transform.parent = transform;
            subColliders[i] = colliderObj.AddComponent<BoxCollider>();
        }
    }

    void UpdateSubColliders()
    {
        Vector3[] vertices = mesh.vertices;
        int width = 3; // 가로로 나눌 조각 수
        int height = 3; // 세로로 나눌 조각 수
        float subWidth = meshFilter.mesh.bounds.size.x / width;
        float subHeight = meshFilter.mesh.bounds.size.y / height;

        for (int i = 0; i < numSubColliders; i++)
        {
            int xIndex = i % width;
            int yIndex = i / width;

            Vector3 min = new Vector3(xIndex * subWidth, yIndex * subHeight, meshFilter.mesh.bounds.min.z);
            Vector3 max = new Vector3((xIndex + 1) * subWidth, (yIndex + 1) * subHeight, meshFilter.mesh.bounds.max.z);

            subBounds[i] = new Bounds((min + max) / 2, max - min);
            foreach (var vertex in vertices)
            {
                if (vertex.x >= min.x && vertex.x <= max.x && vertex.y >= min.y && vertex.y <= max.y)
                {
                    subBounds[i].Encapsulate(vertex);
                }
            }

            subColliders[i].center = subBounds[i].center - transform.position;
            subColliders[i].size = subBounds[i].size;
        }
    }

    void Update()
    {
        // 메쉬가 변형된 경우에만 업데이트 호출
        if (MeshHasChanged())
        {
            UpdateSubColliders();
        }
    }

    bool MeshHasChanged()
    {
        // 메쉬가 변경되었는지 확인하는 로직 구현
        // 이 예제에서는 항상 true를 반환
        return true;
    }
}
