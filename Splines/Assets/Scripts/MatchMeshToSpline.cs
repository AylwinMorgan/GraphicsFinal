using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class MatchMeshToSpline : MonoBehaviour
{
    private MeshFilter meshFilter;
    [SerializeField] private Mesh mesh;
    //private Mesh splineMesh;
    //private MeshRenderer meshRenderer;
    private Spline spline;
    private List<GameObject> railSegments;
    [SerializeField] private GameObject rail;
    private float meshLength;
    private float timeBetweenUpdates;
    private float elapsedTime;

    // Start is called before the first frame update
    void Start()
    {
//        meshRenderer = GetComponent<MeshRenderer>();
//        meshFilter = GetComponent<MeshFilter>();
        //mesh = meshFilter.sharedMesh;
        spline = GetComponent<Spline>();
        railSegments = new List<GameObject>();
        float startPositionZ = float.MaxValue;
        float endPositionZ = float.MinValue;

        timeBetweenUpdates = 0.2f;
        elapsedTime = 0.0f;

        foreach (Vector3 vertex in mesh.vertices)
        {
            startPositionZ = Mathf.Min(vertex.z, startPositionZ);
            endPositionZ = Mathf.Max(vertex.z, endPositionZ);
        }


        meshLength = Mathf.Abs(endPositionZ - startPositionZ);
    }

    public void DeformMeshToSpline(Spline spline)
    {
        // calculate spline length by adding together the length of each contained bezier curve
        float splineLength = spline.getApproximateSplineLength();

        //int meshCount = Math.Max((int)(splineLength / meshLength),spline.curves.Count);
        int meshCount = spline.curves.Count;


        // reset rail segments list
        if (meshCount < GameObject.FindGameObjectsWithTag("Rail").Length)
        {
            foreach (GameObject rail in railSegments)
            {
                Destroy(rail);
            }
            railSegments.Clear();
        }
        while (meshCount > GameObject.FindGameObjectsWithTag("Rail").Length)
        {
            GameObject newRail = Instantiate(rail);
            railSegments.Add(newRail);
        }

        for (int i = 0; i < meshCount; i++)
        {
            // create a new rail segment and add it to list
            GameObject newRail = railSegments[i];
            int vertexCount = 0;
            Vector3[] meshVertices = newRail.GetComponent<MeshFilter>().mesh.vertices;
            Vector3[] newVertices = new Vector3[meshVertices.Length];
            float startPositionZ = float.MaxValue;
            float endPositionZ = float.MinValue;

            // get start and end position by calculating greatest and least Z values
            foreach (Vector3 vertex in mesh.vertices)
            {
                startPositionZ = Mathf.Min(vertex.z, startPositionZ);
                endPositionZ = Mathf.Max(vertex.z, endPositionZ);
            }


            foreach (Vector3 v in mesh.vertices)
            {
                // 0 = start of spline. 1 = end of spline
                // inverse lerp to get relative position
                float tValueInSpline = i + (v.z - startPositionZ) / (endPositionZ - startPositionZ);



                // actual Vec3 position
                Vector3 positionInSpline = spline.getPositionAtTime(tValueInSpline);

                // get orientation of point on spline
                Vector3 forwardVector = spline.getForwardVectorAtTime(tValueInSpline).normalized;
                Vector3 rightVector = Vector3.Cross(Vector3.up, forwardVector).normalized;
                Vector3 upVector = Vector3.Cross(forwardVector, rightVector).normalized;

                Quaternion rotation = Quaternion.LookRotation(forwardVector, upVector);

                newVertices[vertexCount] = (positionInSpline + rotation * new Vector3(v.x, v.y, 0.0f));
                vertexCount++;
            }
            Mesh splineMesh = newRail.GetComponent<MeshFilter>().mesh;
            splineMesh.vertices = newVertices;
            splineMesh.triangles = mesh.triangles;
            splineMesh.RecalculateBounds();
            splineMesh.RecalculateNormals();

            //meshFilter.mesh = splineMesh;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // keep updates to only 5x per second instead of every frame for better performance
        // TODO: only call DeformMeshToSpline() when one of the knots is altered/a knot is added/removed for better performance
        if (elapsedTime > timeBetweenUpdates)
        {
            DeformMeshToSpline(spline);
            elapsedTime = 0.0f;
        }
        elapsedTime += Time.deltaTime;
    }
}
