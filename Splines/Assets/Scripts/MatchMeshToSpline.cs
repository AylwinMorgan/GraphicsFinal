using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchMeshToSpline : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Mesh mesh;
    private Mesh splineMesh;
    private MeshRenderer meshRenderer;
    private Spline spline;
    private List<GameObject> railSegments;
    [SerializeField] private GameObject rail;
    float meshLength;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.sharedMesh;
        spline = gameObject.GetComponent<Spline>();
        
        float startPositionX = float.MaxValue;
        float endPositionX = float.MinValue;

        foreach (Vector3 vertex in mesh.vertices)
        {
            startPositionX = Mathf.Min(vertex.z, startPositionX);
            endPositionX = Mathf.Max(vertex.z, endPositionX);
        }

        meshLength = Mathf.Abs(endPositionX - startPositionX);
    }

    public void DeformMeshToSpline(Spline spline)
    {
        // calculate spline length by adding together the length of each contained bezier curve
        float splineLength = spline.getApproximateSplineLength();

        int meshCount = (int)(splineLength / meshLength);
        
        // reset rail segments list
        foreach (GameObject rail in railSegments)
        {
            Destroy(rail);
        }
        railSegments.Clear();

        for (int i = 0; i < meshCount; i++)
        {
            // create a new rail segment and add it to list
            GameObject newRail = Instantiate(rail);
            railSegments.Add(newRail);
            int vertexCount = 0;
            Vector3[] meshVertices = newRail.GetComponent<MeshFilter>().mesh.vertices;
            Vector3[] newVertices = new Vector3[meshVertices.Length];
            float startPositionX = float.MaxValue;
            float endPositionX = float.MinValue;
            foreach (Vector3 vertex in mesh.vertices)
            {
                startPositionX = Mathf.Min(vertex.z, startPositionX);
                endPositionX = Mathf.Max(vertex.z, endPositionX);
            }

            foreach (Vector3 v in meshVertices)
            {
                // 0 = start of spline. 1 = end of spline
                // inverse lerp to get relative position
                float tValueInSpline = (v.z - startPositionX) / (endPositionX - startPositionX);

                // actual Vec3 position
                Vector3 positionInSpline = spline.getPositionAtTime(tValueInSpline);

                // get orientation of point on spline
                Vector3 forwardVector = spline.getForwardVectorAtTime(tValueInSpline).normalized;
                Vector3 rightVector = Vector3.Cross(Vector3.up, forwardVector).normalized;
                Vector3 upVector = Vector3.Cross(forwardVector, rightVector).normalized;

                Quaternion rotation = Quaternion.LookRotation(forwardVector, upVector);

                newVertices[vertexCount] = positionInSpline + rotation * new Vector3(v.x, v.y, v.z);
                vertexCount++;
            }
            Mesh splineMesh = newRail.GetComponent<MeshFilter>().sharedMesh;
            splineMesh.vertices = newVertices;
            splineMesh.triangles = mesh.triangles;
            splineMesh.RecalculateBounds();
            splineMesh.RecalculateNormals();

            meshFilter.mesh = splineMesh;
        }
    }

    // Update is called once per frame
    void Update()
    {
        DeformMeshToSpline(spline);
    }
}
