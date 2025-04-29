using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    private float timeBetweenUpdates;
    private float elapsedTime;
    private float meshLength;

    private float startZ;
    private float endZ;

    private bool isUpdating = true;

    // precise length of railroad mesh
    // use a method which can support any mesh if possible
    //private const float meshLength = 1.809603f;

    // Start is called before the first frame update
    void Start()
    {
        spline = GetComponent<Spline>();
        railSegments = new List<GameObject>();
        startZ = float.MaxValue;
        endZ = float.MinValue;

        timeBetweenUpdates = 1.0f;
        elapsedTime = 0.0f;

        foreach (Vector3 vertex in mesh.vertices)
        {
            startZ = Mathf.Min(vertex.z, startZ);
            endZ = Mathf.Max(vertex.z, endZ);
        }

        meshLength = Mathf.Abs(endZ - startZ);
    }

    public void DeformMeshToSpline(Spline spline)
    {
        // clear all existing segments
        foreach (GameObject rail in railSegments)
        {
            Destroy(rail);
        }
        railSegments.Clear();
        // get length of spline and rail segment
        float splineLength = spline.getApproximateSplineLength();

        // get the number of segments that fit along spline (spline length / mesh length)
        int segmentAmount = Mathf.CeilToInt(splineLength / meshLength);

        for (int i = 0; i < segmentAmount; i++)
        {
            // get relative position of mesh in spline
            // this can be done by finding (i * meshLength) / splineLength
            // for the starting t value of the segment
            // and ((i + 1) * meshLength) / splineLength
            // for the end value (max of 1)
            float startT = (i * meshLength) / splineLength;
            float endT = ((i + 1) * meshLength) / splineLength;

            //endT = MathF.Min(endT, 1);

            // instantiate a segment 
            GameObject segment = Instantiate(rail);
            railSegments.Add(segment);
            Vector3[] meshVertices = segment.GetComponent<MeshFilter>().mesh.vertices;
            Vector3[] newVertices = new Vector3[meshVertices.Length];

            int vertexCount = 0;
            foreach (Vector3 vertex in meshVertices)
            {
                // get the vertex z position
                float localZ = vertex.z;
                // get local t value (how far between ends of the segment the vertex is)
                float localT = Mathf.InverseLerp(startZ, endZ, localZ);
                // find the final t value in the overarching spline by lerping between the start and end t values using the local t value
                float finalT = Mathf.Lerp(startT, endT, localT) * spline.curves.Count;

                finalT = Mathf.Min(Mathf.Max(finalT, 0), spline.curves.Count);

                // find spline position and rotation at final t value
                Vector3 finalPosition = spline.getPositionAtTime(finalT);

                // calculate offset using local x and y value and calculate final vertex

                Vector3 forwardVector = spline.getForwardVectorAtTime(finalT).normalized;
                Vector3 rightVector = Vector3.Cross(Vector3.up, forwardVector).normalized;
                Vector3 upVector = Vector3.Cross(forwardVector, rightVector).normalized;

                Vector3 offset = vertex.x * rightVector + vertex.y * upVector;

                newVertices[vertexCount] = finalPosition + offset;
                vertexCount++;
            }
            Mesh splineMesh = segment.GetComponent<MeshFilter>().mesh;
            splineMesh.vertices = newVertices;
            splineMesh.triangles = mesh.triangles;
            splineMesh.RecalculateBounds();
            splineMesh.RecalculateNormals();
        }

        // previous method of mesh warping (resulted in stretching of mesh)
        /*
        // calculate spline length by adding together the length of each contained bezier curve            
        //float splineLength = spline.getApproximateSplineLength();
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
                startZ = Mathf.Min(vertex.z, startZ);
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

                Vector3 offset = v.x * rightVector + v.y * upVector;

                //Quaternion rotation = Quaternion.LookRotation(forwardVector, upVector);

                //newVertices[vertexCount] = (positionInSpline + rotation * new Vector3(v.x, v.y, v.z));
                newVertices[vertexCount] = positionInSpline + offset;
                vertexCount++;
            }
            Mesh splineMesh = newRail.GetComponent<MeshFilter>().mesh;
            splineMesh.vertices = newVertices;
            splineMesh.triangles = mesh.triangles;
            splineMesh.RecalculateBounds();
            splineMesh.RecalculateNormals();

            //meshFilter.mesh = splineMesh;
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        // keep updates to only 5x per second instead of every frame for better performance
        if (elapsedTime > timeBetweenUpdates && isUpdating)
        {
            DeformMeshToSpline(spline);
            elapsedTime = 0.0f;
        }
        elapsedTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isUpdating = !isUpdating;
        }
    }

}
