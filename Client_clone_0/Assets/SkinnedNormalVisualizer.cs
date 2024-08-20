using UnityEngine;

public class SkinnedNormalVisualizer : MonoBehaviour
{
    public float normalLength = 0.1f;

    void OnDrawGizmos()
    {
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
        {
            Mesh mesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(mesh); // 변형된 메쉬를 가져옵니다.

            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertexWorldPosition = transform.TransformPoint(vertices[i]);
                Vector3 normalWorldDirection = transform.TransformDirection(normals[i]);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(vertexWorldPosition, vertexWorldPosition + normalWorldDirection * normalLength);
            }
        }
    }
}
