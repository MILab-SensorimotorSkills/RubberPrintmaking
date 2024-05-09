using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class MeshColliderUpdater : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Vector3[] lastVertices;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        // 초기 정점 배열을 복사합니다.
        lastVertices = meshFilter.mesh.vertices.Clone() as Vector3[];
        meshCollider.sharedMesh = meshFilter.mesh; // 초기 콜라이더 설정
    }

    void Update()
    {
        if (HasMeshChanged())
        {
            UpdateCollider();
        }
    }

    // 메쉬가 변경되었는지 확인합니다.
    bool HasMeshChanged()
    {
        Vector3[] currentVertices = meshFilter.mesh.vertices;
        if (currentVertices.Length != lastVertices.Length)
        {
            return true;
        }

        for (int i = 0; i < currentVertices.Length; i++)
        {
            if (currentVertices[i] != lastVertices[i])
            {
                return true;
            }
        }

        return false;
    }

    // 메쉬 콜라이더를 업데이트합니다.
    void UpdateCollider()
    {
        meshCollider.sharedMesh = null; // 메쉬 콜라이더를 리셋합니다.
        meshCollider.sharedMesh = meshFilter.mesh; // 업데이트된 메쉬로 콜라이더를 설정합니다.
        lastVertices = meshFilter.mesh.vertices.Clone() as Vector3[]; // 현재 정점 상태를 저장합니다.
    }
}
