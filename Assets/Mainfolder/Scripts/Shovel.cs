using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiggingTest
{
    public class Shovel : MonoBehaviour
    {
        [Header("References")]
        public MeshFilter groundMesh;

        [Header("Settings Terrain")]
        public bool updateGround = false;
        public float updateInterval = 0.01f; // 업데이트 간격
        public float cellSize = 0.1f; // 그리드 셀 크기
        public float maxDepth = 0.7f; // 최대 깊이, 인스펙터에서 조정 가능

        private Vector3 shovelPrevPos;
        private List<Collector> collectors = new List<Collector>();
        private Collider shovelCollider;
        public Collider virtual_shovelCollider;
        private float timeSinceLastUpdate = 0f;

        private Dictionary<Vector2Int, List<int>> grid = new Dictionary<Vector2Int, List<int>>();
        private Vector3[] initialVertices;

        #region Sound
        private SoundPlayer sound;
        #endregion

        void Start()
        {
            shovelCollider = GetComponent<Collider>();
            shovelPrevPos = transform.position;
            sound = gameObject.GetComponent<SoundPlayer>();

            // 초기 버텍스 위치 저장
            initialVertices = groundMesh.mesh.vertices.Clone() as Vector3[];

            // 그리드 생성
            CreateGrid();

            // 그리드 셀 개수 출력
            PrintGridCellCount();
        }

        void Update()
        {
            timeSinceLastUpdate += Time.deltaTime;

            if (updateGround && timeSinceLastUpdate >= updateInterval)
            {
                UpdateGroundMesh();
                timeSinceLastUpdate = 0f;
            }
        }

        void CreateGrid()
        {
            Vector3[] vertices = groundMesh.mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldVertexPosition = groundMesh.transform.TransformPoint(vertices[i]);
                Vector2Int gridPos = GetGridPosition(worldVertexPosition);

                if (!grid.ContainsKey(gridPos))
                {
                    grid[gridPos] = new List<int>();
                }

                grid[gridPos].Add(i);
            }
        }

        Vector2Int GetGridPosition(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt(worldPosition.x / cellSize);
            int z = Mathf.FloorToInt(worldPosition.z / cellSize);
            return new Vector2Int(x, z);
        }

        void UpdateGroundMesh()
        {
            const float MaxRaycastDistance = 0.5f; // 레이캐스트 거리 증가
            const float MaxDistanceSquared = MaxRaycastDistance;
            Vector3[] vertices = groundMesh.mesh.vertices;
            Vector3 shovelPosition = shovelCollider.transform.position;
            Vector2Int shovelGridPos = GetGridPosition(shovelPosition);
            bool isMeshUpdated = false;

            List<int> verticesToUpdate = new List<int>();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    Vector2Int gridPos = shovelGridPos + new Vector2Int(dx, dz);
                    if (grid.ContainsKey(gridPos))
                    {
                        verticesToUpdate.AddRange(grid[gridPos]);
                    }
                }
            }

            foreach (int i in verticesToUpdate)
            {
                Vector3 worldVertexPosition = groundMesh.transform.TransformPoint(vertices[i]);
                if ((shovelPosition - worldVertexPosition).sqrMagnitude <= MaxDistanceSquared)
                //그니까 조각도랑 고무판의 거리 차이가 맥스 디스턴스 이내일경우, (고무판 내부 또는 그 밑이겠지?)
                {
                    RaycastHit hit;
                    if (RaycastGround(worldVertexPosition, MaxRaycastDistance, out hit))
                    {
                        Vector3 newVertexPosition = groundMesh.transform.InverseTransformPoint(hit.point);
                        sound.PlaySound();
                        // 새로운 버텍스 위치가 초기 위치에서 Y축 아래로 maxDepth 이상 변형되지 않도록 클램핑
                        Vector3 initialWorldVertexPosition = groundMesh.transform.TransformPoint(initialVertices[i]);
                        if (newVertexPosition.y < initialWorldVertexPosition.y - maxDepth)
                        {
                            newVertexPosition.y = initialWorldVertexPosition.y - maxDepth;
                        }

                        vertices[i] = newVertexPosition;
                        isMeshUpdated = true;
                    }
                }else{
                    sound.StopSound();
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

    // Draw the ray for visualization

    bool result = virtual_shovelCollider.Raycast(ray, out hit, distance);

    return result;
}


        private void OnCollisionEnter(Collision other)
        {
            // 특정 조건하에 충돌 로그
        }

        private void PrintGridCellCount()
        {
            // 메쉬의 최소 및 최대 경계값을 사용하여 그리드 셀의 개수를 계산하고 출력
            Bounds bounds = groundMesh.mesh.bounds;
            Vector3 meshMin = groundMesh.transform.TransformPoint(bounds.min);
            Vector3 meshMax = groundMesh.transform.TransformPoint(bounds.max);

            float meshSizeX = meshMax.x - meshMin.x;
            float meshSizeZ = meshMax.z - meshMin.z;

            int gridCellsX = Mathf.CeilToInt(meshSizeX / cellSize);
            int gridCellsZ = Mathf.CeilToInt(meshSizeZ / cellSize);
            int totalGridCells = gridCellsX * gridCellsZ;

            Debug.Log("Total Grid Cells: " + totalGridCells);
        }
    }
}
