using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiggingTest
{
    public class Shovel : MonoBehaviour
    {
        [Header("References")]
        //public Transform edgeRoot;
        //public MeshFilter sandMesh;
        public MeshFilter groundMesh;

        [Header("Settings Terrain")]
        public bool updateGround = false;

        private Vector3 shovelPrevPos;
        private List<Collector> collectors = new List<Collector>();
        private Collider shovelCollider;

        void Start()
        {
            shovelCollider = GetComponent<Collider>();
            shovelPrevPos = transform.position;
        }

        void Update()
        {
            if (updateGround != null)
            {
                UpdateGroundMesh();
            }
        }

void UpdateGroundMesh()
{
    const float MaxRaycastDistance = 1f;
    const float MaxDistanceSquared = MaxRaycastDistance * MaxRaycastDistance;
    Vector3[] vertices = groundMesh.mesh.vertices;
    Vector3 shovelPosition = shovelCollider.transform.position;
    bool isMeshUpdated = false;

    // 수정된 반복문 조건
    for (int i = 0; i < vertices.Length; i ++)
    {
        Vector3 worldVertexPosition = groundMesh.transform.TransformPoint(vertices[i]);
        if ((shovelPosition - worldVertexPosition).sqrMagnitude <= MaxDistanceSquared)
        {
            RaycastHit hit;
            if (RaycastGround(worldVertexPosition, MaxRaycastDistance, out hit))
            {
                Vector3 newVertexPosition = groundMesh.transform.InverseTransformPoint(hit.point);
                vertices[i] = newVertexPosition;
                isMeshUpdated = true;

                // Interpolate between vertices
                // if (i > 0)
                // {
                //     Vector3 previousVertexPosition = vertices[i - 4];
                //     for (int j = 1; j < 4 && (i + j) < vertices.Length; j++)
                //     {
                //         vertices[i + j] = Vector3.Lerp(previousVertexPosition, newVertexPosition, j / 4.0f);
                //     }
                // }
            }else{
                //Debug.DrawRay(worldVertexPosition, Vector3.up * 0.01f, Color.green);
        }
            }
    }

    if (isMeshUpdated)
    {
        groundMesh.mesh.vertices = vertices;
        groundMesh.mesh.RecalculateBounds();
    }
}

private bool RaycastGround(Vector3 origin, float distance, out RaycastHit hit)
{
    Ray ray = new Ray(origin, Vector3.down);
    bool result = shovelCollider.Raycast(ray, out hit, distance);
    return result;
}



        private void OnCollisionEnter(Collision other) {
            // 특정 조건하에 충돌 로그
        }
    }
}
