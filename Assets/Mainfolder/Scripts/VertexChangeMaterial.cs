using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VertexChangeMaterial : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] initialVertices;
    private HashSet<int> changedVertexIndices = new HashSet<int>();
    private Material[] initialMaterials;
    public Material changedMaterial;
    private bool needToUpdateSubmesh = false;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        initialVertices = mesh.vertices.Clone() as Vector3[];

        initialMaterials = GetComponent<MeshRenderer>().materials;
        GetComponent<MeshRenderer>().materials = initialMaterials;
    }

    void Update()
    {
        Vector3[] currentVertices = mesh.vertices;

        for (int i = 0; i < currentVertices.Length; i++)
        {
            if (currentVertices[i] != initialVertices[i] && !changedVertexIndices.Contains(i))
            {
                changedVertexIndices.Add(i);
                needToUpdateSubmesh = true;
            }
        }

        if (needToUpdateSubmesh)
        {
            ApplyChangedMaterial();
            needToUpdateSubmesh = false;
        }
    }

    void ApplyChangedMaterial()
    {
        var triangles = mesh.triangles;
        var submeshTriangles = new List<int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (changedVertexIndices.Contains(triangles[i]) || changedVertexIndices.Contains(triangles[i + 1]) || changedVertexIndices.Contains(triangles[i + 2]))
            {
                submeshTriangles.AddRange(new int[] { triangles[i], triangles[i + 1], triangles[i + 2] });
            }
        }

        if (submeshTriangles.Count > 0)
        {
            mesh.subMeshCount = 2;
            mesh.SetTriangles(triangles, 0);
            mesh.SetTriangles(submeshTriangles.ToArray(), 1);

            var materials = new Material[] { initialMaterials[0], changedMaterial };
            GetComponent<MeshRenderer>().materials = materials;
        }
    }

    void OnDestroy()
    {
        initialVertices = null;
        changedVertexIndices.Clear();
    }
}
