﻿// // 회전불가능코드
// // using System.Collections;
// // using System.Collections.Generic;
// // using UnityEngine;

// // namespace DiggingTest
// // {
// //     public class Shovel : MonoBehaviour
// //     {
// //         [Header("References")]
// //         public MeshFilter groundMesh;

// //         [Header("Settings Terrain")]
// //         public bool updateGround = false;
// //         public float updateInterval = 0.01f; // 업데이트 간격
// //         public float cellSize = 0.1f; // 그리드 셀 크기
// //         public float maxDepth = 0.7f; // 최대 깊이, 인스펙터에서 조정 가능

// //         private Vector3 shovelPrevPos;
// //         private List<Collector> collectors = new List<Collector>();
// //         private Collider shovelCollider;
// //         public GameObject virtual_shovelCollider;
// //         private float timeSinceLastUpdate = 0f;

// //         private Dictionary<Vector2Int, List<int>> grid = new Dictionary<Vector2Int, List<int>>();
// //         private Vector3[] initialVertices;
        
// //         //distance, depth용
// //         public Vector3 hitPoint;
  

// //         void Start()
// //         {
// //             shovelCollider = GetComponent<Collider>();
// //             shovelPrevPos = transform.position;
// //             //sound = gameObject.GetComponent<SoundPlayer>();

// //             // 초기 버텍스 위치 저장
// //             initialVertices = groundMesh.mesh.vertices.Clone() as Vector3[];

// //             // 그리드 생성
// //             CreateGrid();
// //         }

// //         void Update()
// //         {
// //             timeSinceLastUpdate += Time.deltaTime;

// //             if (updateGround && timeSinceLastUpdate >= updateInterval)
// //             {
// //                 UpdateGroundMesh();
// //                 timeSinceLastUpdate = 0f;
// //             }
// //         }

// //         void CreateGrid()
// //         {
// //             Vector3[] vertices = groundMesh.mesh.vertices;

// //             for (int i = 0; i < vertices.Length; i++)
// //             {
// //                 Vector3 worldVertexPosition = groundMesh.transform.TransformPoint(vertices[i]);
// //                 Vector2Int gridPos = GetGridPosition(worldVertexPosition);

// //                 if (!grid.ContainsKey(gridPos))
// //                 {
// //                     grid[gridPos] = new List<int>();
// //                 }

// //                 grid[gridPos].Add(i);
// //             }
// //         }

// //         Vector2Int GetGridPosition(Vector3 worldPosition)
// //         {
// //             int x = Mathf.FloorToInt(worldPosition.x / cellSize);
// //             int z = Mathf.FloorToInt(worldPosition.z / cellSize);
// //             return new Vector2Int(x, z);
// //         }

// //         void UpdateGroundMesh()
// //         {
// //             const float MaxRaycastDistance = 0.5f; // 레이캐스트 거리 증가
// //             const float MaxDistanceSquared = MaxRaycastDistance;
// //             Vector3[] vertices = groundMesh.mesh.vertices;
// //             Vector3 shovelPosition = shovelCollider.transform.position;
// //             Vector2Int shovelGridPos = GetGridPosition(shovelPosition);
// //             bool isMeshUpdated = false;

// //             List<int> verticesToUpdate = new List<int>();
// //             for (int dx = -1; dx <= 1; dx++)
// //             {
// //                 for (int dz = -1; dz <= 1; dz++)
// //                 {
// //                     Vector2Int gridPos = shovelGridPos + new Vector2Int(dx, dz);
// //                     if (grid.ContainsKey(gridPos))
// //                     {
// //                         verticesToUpdate.AddRange(grid[gridPos]);
// //                     }
// //                 }
// //             }

// //             foreach (int i in verticesToUpdate)
// //             {
// //                 Vector3 worldVertexPosition = groundMesh.transform.TransformPoint(vertices[i]);
// //                 if ((shovelPosition - worldVertexPosition).sqrMagnitude <= MaxDistanceSquared)
// //                 //그니까 조각도랑 고무판의 거리 차이가 맥스 디스턴스 이내일경우, (고무판 내부 또는 그 밑이겠지?)
// //                 {
// //                     RaycastHit hit;
// //                     if (RaycastGround(worldVertexPosition, MaxRaycastDistance, out hit))
// //                     {
// //                         Vector3 newVertexPosition = groundMesh.transform.InverseTransformPoint(hit.point);
                        
// //                         // 새로운 버텍스 위치가 초기 위치에서 Y축 아래로 maxDepth 이상 변형되지 않도록 클램핑
// //                         Vector3 initialWorldVertexPosition = groundMesh.transform.TransformPoint(initialVertices[i]);
// //                         if (newVertexPosition.y < initialWorldVertexPosition.y - maxDepth)
// //                         {
// //                             newVertexPosition.y = initialWorldVertexPosition.y - maxDepth;
// //                         }

// //                         vertices[i] = newVertexPosition;
// //                         isMeshUpdated = true;
// //                     }
// //                 }
// //             }

// //             if (isMeshUpdated)
// //             {
// //                 groundMesh.mesh.vertices = vertices;
// //                 groundMesh.mesh.RecalculateBounds();
// //             }
// //         }

// //         private bool RaycastGround(Vector3 origin, float distance, out RaycastHit hit)
// //         {
// //             Ray ray = new Ray(origin, Vector3.down);
        
// //             // Draw the ray for visualization
// //             Collider Virtual_shovelCollider = virtual_shovelCollider.GetComponent<Collider>();
// //             bool result = Virtual_shovelCollider.Raycast(ray, out hit, distance);
// //             //result1 = result;
// //             //Debug.Log(result1);
            
// //             if(result){
// //                 hitPoint = hit.point;
// //                 // Debug.Log(hit.point+"HIT.POINT");
// //                 //Debug.Log(hitPoint+"hitPOINT");
// //             }
// //             return result;
// //         }

// //     }
// // }

// //회전가능코드

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace DiggingTest
// {
//     public class Shovel : MonoBehaviour
//     {
//         [Header("References")]
//         public MeshFilter groundMesh;

//         [Header("Settings Terrain")]
//         public bool updateGround = false;
//         public float updateInterval = 0.01f; // 업데이트 간격
//         public float cellSize = 0.1f; // 그리드 셀 크기
//         public float maxDepth = 0.7f; // 최대 깊이, 인스펙터에서 조정 가능

//         private Vector3 shovelPrevPos;
//         private List<Collector> collectors = new List<Collector>();
//         private Collider shovelCollider;
//         public GameObject virtual_shovelCollider;
//         private float timeSinceLastUpdate = 0f;

//         private Dictionary<Vector2Int, List<int>> grid = new Dictionary<Vector2Int, List<int>>();
//         private Vector3[] initialVertices;
        
//         //distance, depth용
//         public Vector3 hitPoint;

//         private bool hasRotated = false;//**************
//         private Vector3 lastRotation; //*************
//         void Start()
//         {
//             shovelCollider = GetComponent<Collider>();
//             shovelPrevPos = transform.position;
//             //sound = gameObject.GetComponent<SoundPlayer>();

//             // 초기 버텍스 위치 저장
//             initialVertices = groundMesh.mesh.vertices.Clone() as Vector3[];

//             // 그리드 생성
//             CreateGrid();

//             lastRotation = transform.rotation.eulerAngles; //***********
//         }

//         // void Update()
//         // {
//         //     timeSinceLastUpdate += Time.deltaTime;

//         //     if (updateGround && timeSinceLastUpdate >= updateInterval)
//         //     {
//         //         UpdateGroundMesh();
//         //         timeSinceLastUpdate = 0f;
//         //     }
//         // }

//         void Update()
//         {
//             timeSinceLastUpdate += Time.deltaTime;

//             // 회전이 발생했는지 확인하여 그리드 재생성
//             if (updateGround && timeSinceLastUpdate >= updateInterval)
//             {
//                 if (HasRotationChanged())
//                 {
//                     CreateGrid(); // 회전된 정점 위치에 맞게 그리드를 재생성
//                 }
//                 UpdateGroundMesh();
//                 timeSinceLastUpdate = 0f;
//             }
//         }

//         // 회전이 발생했는지 확인하는 메서드
//         private bool HasRotationChanged()
//         {
//             Vector3 currentRotation = transform.rotation.eulerAngles;
//             if (lastRotation != currentRotation)
//             {
//                 lastRotation = currentRotation; // 회전이 변경되었으므로 마지막 회전 값 갱신
//                 return true;
//             }
//             return false;
//         }

//         // void CreateGrid()
//         // {
//         //     Vector3[] vertices = groundMesh.mesh.vertices;

//         //     for (int i = 0; i < vertices.Length; i++)
//         //     {
//         //         Vector3 worldVertexPosition = groundMesh.transform.TransformPoint(vertices[i]);
//         //         Vector2Int gridPos = GetGridPosition(worldVertexPosition);

//         //         if (!grid.ContainsKey(gridPos))
//         //         {
//         //             grid[gridPos] = new List<int>();
//         //         }

//         //         grid[gridPos].Add(i);
//         //     }
//         // }

//         void CreateGrid()
//         {
//             Vector3[] vertices = groundMesh.mesh.vertices;

//             grid.Clear(); // 기존 그리드를 초기화하고 새로 생성

//             for (int i = 0; i < vertices.Length; i++)
//             {
//                 Vector3 worldVertexPosition = groundMesh.transform.TransformPoint(vertices[i]);
//                 Vector2Int gridPos = GetGridPosition(worldVertexPosition);

//                 if (!grid.ContainsKey(gridPos))
//                 {
//                     grid[gridPos] = new List<int>();
//                 }

//                 grid[gridPos].Add(i);
//             }
//         }  

//         Vector2Int GetGridPosition(Vector3 worldPosition)
//         {
//             int x = Mathf.FloorToInt(worldPosition.x / cellSize);
//             int z = Mathf.FloorToInt(worldPosition.z / cellSize);
//             return new Vector2Int(x, z);
//         }

//         // void UpdateGroundMesh()
//         // {
//         //     const float MaxRaycastDistance = 0.5f; // 레이캐스트 거리 증가
//         //     const float MaxDistanceSquared = MaxRaycastDistance;
//         //     Vector3[] vertices = groundMesh.mesh.vertices;
//         //     Vector3 shovelPosition = shovelCollider.transform.position;
//         //     Vector2Int shovelGridPos = GetGridPosition(shovelPosition);
//         //     bool isMeshUpdated = false;

//         //     List<int> verticesToUpdate = new List<int>();
//         //     for (int dx = -1; dx <= 1; dx++)
//         //     {
//         //         for (int dz = -1; dz <= 1; dz++)
//         //         {
//         //             Vector2Int gridPos = shovelGridPos + new Vector2Int(dx, dz);
//         //             if (grid.ContainsKey(gridPos))
//         //             {
//         //                 verticesToUpdate.AddRange(grid[gridPos]);
//         //             }
//         //         }
//         //     }

//         //     foreach (int i in verticesToUpdate)
//         //     {
//         //         Vector3 worldVertexPosition = groundMesh.transform.TransformPoint(vertices[i]);
//         //         if ((shovelPosition - worldVertexPosition).sqrMagnitude <= MaxDistanceSquared)
//         //         //그니까 조각도랑 고무판의 거리 차이가 맥스 디스턴스 이내일경우, (고무판 내부 또는 그 밑이겠지?)
//         //         {
//         //             RaycastHit hit;
//         //             if (RaycastGround(worldVertexPosition, MaxRaycastDistance, out hit))
//         //             {
//         //                 Vector3 newVertexPosition = groundMesh.transform.InverseTransformPoint(hit.point);
                        
//         //                 // 새로운 버텍스 위치가 초기 위치에서 Y축 아래로 maxDepth 이상 변형되지 않도록 클램핑
//         //                 Vector3 initialWorldVertexPosition = groundMesh.transform.TransformPoint(initialVertices[i]);
//         //                 if (newVertexPosition.y < initialWorldVertexPosition.y - maxDepth)
//         //                 {
//         //                     newVertexPosition.y = initialWorldVertexPosition.y - maxDepth;
//         //                 }

//         //                 vertices[i] = newVertexPosition;
//         //                 isMeshUpdated = true;
//         //             }
//         //         }
//         //     }

//         //     if (isMeshUpdated)
//         //     {
//         //         groundMesh.mesh.vertices = vertices;
//         //         groundMesh.mesh.RecalculateBounds();
//         //     }
//         // }

//          void UpdateGroundMesh()
//     {
//         const float MaxRaycastDistance = 0.5f; // 레이캐스트 거리 증가
//         const float MaxDistanceSquared = MaxRaycastDistance * MaxRaycastDistance;
//         Vector3[] vertices = groundMesh.mesh.vertices;
//         Vector3 shovelPosition = shovelCollider.transform.position;
//         Vector2Int shovelGridPos = GetGridPosition(shovelPosition);
//         bool isMeshUpdated = false;

//         List<int> verticesToUpdate = new List<int>();
//         for (int dx = -1; dx <= 1; dx++)
//         {
//             for (int dz = -1; dz <= 1; dz++)
//             {
//                 Vector2Int gridPos = shovelGridPos + new Vector2Int(dx, dz);
//                 if (grid.ContainsKey(gridPos))
//                 {
//                     verticesToUpdate.AddRange(grid[gridPos]);
//                 }
//             }
//         }

//         foreach (int i in verticesToUpdate)
//         {
//             Vector3 worldVertexPosition = groundMesh.transform.TransformPoint(vertices[i]);
//             if ((shovelPosition - worldVertexPosition).sqrMagnitude <= MaxDistanceSquared)
//             {
//                 RaycastHit hit;
//                 if (RaycastGround(worldVertexPosition, MaxRaycastDistance, out hit))
//                 {
//                     Vector3 newVertexPosition = groundMesh.transform.InverseTransformPoint(hit.point);

//                     Vector3 initialWorldVertexPosition = groundMesh.transform.TransformPoint(initialVertices[i]);
//                     if (newVertexPosition.y < initialWorldVertexPosition.y - maxDepth)
//                     {
//                         newVertexPosition.y = initialWorldVertexPosition.y - maxDepth;
//                     }

//                     vertices[i] = newVertexPosition;
//                     isMeshUpdated = true;
//                 }
//             }
//         }

//         if (isMeshUpdated)
//         {
//             groundMesh.mesh.vertices = vertices;
//             groundMesh.mesh.RecalculateBounds();
//         }
//     }

//     private bool RaycastGround(Vector3 origin, float distance, out RaycastHit hit)
//     {
//         Ray ray = new Ray(origin, Vector3.down);

//         Collider Virtual_shovelCollider = virtual_shovelCollider.GetComponent<Collider>();
//         bool result = Virtual_shovelCollider.Raycast(ray, out hit, distance);

//         if (result)
//         {
//             hitPoint = hit.point;
//         }
//         return result;
//     }
// }
// }


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
        public float updateInterval = 0.05f; 
        public float cellSize = 0.1f; // 그리드 셀 크기
        public float maxDepth = 0.7f; // 최대 깊이, 인스펙터에서 조정 가능

        private Vector3 shovelPrevPos;
        private List<Collector> collectors = new List<Collector>();
        private Collider shovelCollider;
        public GameObject virtual_shovelCollider;
        private float timeSinceLastUpdate = 0f;

        private Dictionary<Vector2Int, List<int>> grid = new Dictionary<Vector2Int, List<int>>();
        private Vector3[] initialVertices;

        //distance, depth용
        public Vector3 hitPoint;

        private Vector3 lastRotation;

        void Start()
        {
            shovelCollider = GetComponent<Collider>();
            shovelPrevPos = transform.position;

            // 초기 버텍스 위치 저장
            initialVertices = groundMesh.mesh.vertices.Clone() as Vector3[];

            // 그리드 생성
            CreateGrid();

            lastRotation = transform.rotation.eulerAngles;
        }

        void Update()
        {
            timeSinceLastUpdate += Time.deltaTime;

            if (updateGround && timeSinceLastUpdate >= updateInterval)
            {
                if (HasRotationChanged())
                {
                    CreateGrid(); // 회전된 정점 위치에 맞게 그리드를 재생성
                }
                UpdateGroundMesh();
                timeSinceLastUpdate = 0f;
            }
        }

        private bool HasRotationChanged()
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            // 작은 각도 변화는 무시 (성능 최적화를 위해)
            if (Vector3.Distance(lastRotation, currentRotation) > 0.1f)
            {
                lastRotation = currentRotation;
                return true;
            }
            return false;
        }

        void CreateGrid()
        {
            Vector3[] vertices = groundMesh.mesh.vertices;
            grid.Clear(); // 기존 그리드를 초기화하고 새로 생성

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
            const float MaxRaycastDistance = 0.5f;
            const float MaxDistanceSquared = MaxRaycastDistance * MaxRaycastDistance;
            Vector3[] vertices = groundMesh.mesh.vertices;
            Vector3 shovelPosition = shovelCollider.transform.position;
            Vector2Int shovelGridPos = GetGridPosition(shovelPosition);
            bool isMeshUpdated = false;

            HashSet<int> verticesToUpdate = new HashSet<int>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    Vector2Int gridPos = shovelGridPos + new Vector2Int(dx, dz);
                    if (grid.ContainsKey(gridPos))
                    {
                        foreach (int index in grid[gridPos])
                        {
                            verticesToUpdate.Add(index);
                        }
                    }
                }
            }

            foreach (int i in verticesToUpdate)
            {
                Vector3 worldVertexPosition = groundMesh.transform.TransformPoint(vertices[i]);
                if ((shovelPosition - worldVertexPosition).sqrMagnitude <= MaxDistanceSquared)
                {
                    RaycastHit hit;
                    if (RaycastGround(worldVertexPosition, MaxRaycastDistance, out hit))
                    {
                        Vector3 newVertexPosition = groundMesh.transform.InverseTransformPoint(hit.point);
                        Vector3 initialWorldVertexPosition = groundMesh.transform.TransformPoint(initialVertices[i]);
                        if (newVertexPosition.y < initialWorldVertexPosition.y - maxDepth)
                        {
                            newVertexPosition.y = initialWorldVertexPosition.y - maxDepth;
                        }

                        vertices[i] = newVertexPosition;
                        isMeshUpdated = true;
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

            Collider Virtual_shovelCollider = virtual_shovelCollider.GetComponent<Collider>();
            bool result = Virtual_shovelCollider.Raycast(ray, out hit, distance);

            if (result)
            {
                hitPoint = hit.point;
            }
            return result;
        }
    }
}
