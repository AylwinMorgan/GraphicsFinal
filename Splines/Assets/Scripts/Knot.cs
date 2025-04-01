using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knot : MonoBehaviour
{
    [SerializeField] public float scale;
    public GameObject controlPoint1;
    public GameObject controlPoint2;

    private MeshRenderer knotRenderer;

    private MeshFilter usedMesh;
    [SerializeField] private Mesh knotMesh;

    public Color color;

    private void Start()
    {
        scale = 1f;
        knotRenderer = gameObject.GetComponent<MeshRenderer>();
        usedMesh = gameObject.GetComponent<MeshFilter>();

        color = Color.red;

        usedMesh.mesh = knotMesh;
        knotRenderer.material.color = color;

        // set up dependent control points
        controlPoint1 = Instantiate(controlPoint1, transform.position, Quaternion.identity);
        controlPoint2 = Instantiate(controlPoint2, transform.position, Quaternion.identity);

        controlPoint1.GetComponent<MeshRenderer>().material.color = color;
        
        controlPoint2.GetComponent<MeshRenderer>().material.color = color;
    }

    private void Update()
    {
        Vector3 dir = transform.forward * scale;
        controlPoint1.transform.position = transform.position + dir;
        controlPoint2.transform.position = transform.position - dir;
    }

    private void OnDestroy()
    {
        Destroy(controlPoint1);
        Destroy(controlPoint2);
    }

    public Vector3 GetNextControlPoint()
    {
        Vector3 dir = transform.forward * scale;
        Vector3 pointPosition = transform.position + dir;
        return pointPosition;
    }

    public Vector3 GetPreviousControlPoint()
    {
        Vector3 dir = transform.forward * scale;
        Vector3 pointPosition = transform.position - dir;
        return pointPosition;
    }
}
