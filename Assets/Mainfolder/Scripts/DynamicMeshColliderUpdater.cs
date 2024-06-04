using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class DynamicMeshColliderUpdater : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh previousMesh;
    private Vector3[] previousVertices;
    private float updateInterval = 1.0f / 30.0f;
    private float nextUpdateTime = 0f;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        previousMesh = Instantiate(meshFilter.mesh);
        previousVertices = previousMesh.vertices;
    }

    void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            if (HasMeshChanged())
            {
                UpdateMeshCollider();
                previousVertices = meshFilter.mesh.vertices; //이전 버텍스 업데이트
            }
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    bool HasMeshChanged()
    {
        Mesh currentMesh = meshFilter.mesh;

        //이전 메쉬와 현재 메쉬의 버텍스 데이터를 비교하여 변형 여부 확인
        if (currentMesh.vertexCount != previousVertices.Length)
            return true;

        Vector3[] currentVertices = currentMesh.vertices;

        for (int i = 0; i < currentVertices.Length; i++)
        {
            if (currentVertices[i] != previousVertices[i])
                return true;
        }

        return false;
    }

    void UpdateMeshCollider()
    {
        // 메쉬 콜라이더를 변형된 메쉬에 맞춰 업데이트
        meshCollider.sharedMesh = null; //콜라이더 리셋
        meshCollider.sharedMesh = meshFilter.mesh;
        //Debug.Log("Mesh Collider Updated");
    }
}
