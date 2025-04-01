using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{

    public GameObject k1;
    public GameObject k2;
    [SerializeField] private Color color;
    private LineRenderer lineRenderer;
    const int subsegmentCount = 50;
    const float lineWidth = 0.05f;
    public bool loop = false;

    private void Start()
    {
        // instantiate knots
        if (!loop)
        {
            getKnotsFromList();
        }
        else
        {
            getFirstAndLastKnots();
        }
        
        // setup linerenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = subsegmentCount;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }

    public void getKnotsFromList()
    {
        GameObject spline = GameObject.FindGameObjectWithTag("Spline");
        k1 = spline.GetComponent<Spline>().knots[^2];
        k2 = spline.GetComponent<Spline>().knots[^1];
    }

    public void getFirstAndLastKnots()
    {
        GameObject spline = GameObject.FindGameObjectWithTag("Spline");
        k1 = spline.GetComponent<Spline>().knots[^1];
        k2 = spline.GetComponent<Spline>().knots[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (k1 != null && k2 != null)
        {
            for (int i = 0; i < subsegmentCount; i++)
            {
                lineRenderer.SetPosition(i, GetPositionAtTime((float)i / subsegmentCount));
            }
        }
    }

    public Vector3 GetPositionAtTime(float t)
    {
        Vector3 p0 = k1.transform.position;
        Vector3 p1 = k1.GetComponent<Knot>().GetNextControlPoint();
        Vector3 p2 = k2.GetComponent<Knot>().GetPreviousControlPoint();
        Vector3 p3 = k2.transform.position;

        Vector3 result = p0 * ((1 - t) * (1 - t) * (1 - t)) + p1 * (3 * t * (1 - t) * (1 - t)) + p2 * (3 * t * t * (1 - t)) + p3 * (t * t * t);
        //return result;

        Vector3 a = Vector3.Lerp(k1.transform.position,k1.GetComponent<Knot>().GetNextControlPoint(),t);
        Vector3 b = Vector3.Lerp(k1.GetComponent<Knot>().GetNextControlPoint(),k2.GetComponent<Knot>().GetPreviousControlPoint(),t);
        Vector3 c = Vector3.Lerp(k2.GetComponent<Knot>().GetPreviousControlPoint(),k2.transform.position,t);
        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(d, e, t);
    }

    public Vector3 GetForwardVectorAtTime(float t)
    {
        Vector3 p0 = k1.transform.position;
        Vector3 p1 = k1.GetComponent<Knot>().GetNextControlPoint();
        Vector3 p2 = k2.GetComponent<Knot>().GetPreviousControlPoint();
        Vector3 p3 = k2.transform.position;

        Vector3 result = p0 * (-3 * t * t + 6 * t - 3) + p1 * (9 * t * t - 12 * t + 3) + p2 * (-9 * t * t + 6 * t) + p3 * (3 * t * t);

        return result;
    }
}
