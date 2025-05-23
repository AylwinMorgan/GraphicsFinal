using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



public class Spline : MonoBehaviour
{
    // Start is called before the first frame update

    public List<GameObject> knots;
    public List<GameObject> curves;

    public List<Vector3> previousKnotPositions;
    public List<Quaternion> previousKnotRotations;
    public List<Vector3> currentKnotPositions;
    public List<Quaternion> currentKnotRotations;


    [SerializeField] private GameObject knot;
    [SerializeField] private GameObject curve;
    [SerializeField] private Button addCurveButton;
    [SerializeField] private Button removeCurveButton;
    [SerializeField] private GameObject objectFollowingSpline;
    [SerializeField] private Button toggleLoopButton;

    float time;
    public bool looping;
    float timePerCurve;

    private void Start()
    {
        time = 0.0f;
        timePerCurve = 1.0f;
        looping = false;

        GameObject startingKnot = GameObject.FindGameObjectWithTag("Knot");

        if (startingKnot == null)
        {
            startingKnot = Instantiate(knot, gameObject.transform.position, Quaternion.identity);
            knots.Add(startingKnot);
            AddCurve();
        }

        //addCurveButton.onClick.AddListener(AddCurve);
        //removeCurveButton.onClick.AddListener(RemoveCurve);
        //toggleLoopButton.onClick.AddListener(ToggleEnclosedLoop);

        //ToggleEnclosedLoop();
    }

    private void Update()
    {
        objectFollowingSpline.transform.position = getPositionAtTime(time);
        objectFollowingSpline.transform.forward = getForwardVectorAtTime(time);

        objectFollowingSpline.transform.position += objectFollowingSpline.transform.up * 0.35f;

        time += Time.deltaTime;

        if (time > timePerCurve * curves.Count)
        {
            time = 0;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Knot");
            foreach (GameObject k in objects)
            {
                bool isActive = k.GetComponent<MeshRenderer>().enabled;
                k.GetComponent<MeshRenderer>().enabled = !isActive;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            AddCurve();
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            RemoveCurve();
        }
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            ToggleEnclosedLoop();
        }
    }

    public void ToggleEnclosedLoop()
    {
        time = 0;
        if (looping)
        {
            // remove the last curve
            Destroy(curves[^1]);
            curves.Remove(curves[^1]);
        }
        else
        {
            // add a curve that uses the first and last knots
            GameObject newCurve = Instantiate(curve,Vector3.zero, Quaternion.identity);
            newCurve.GetComponent<BezierCurve>().loop = true;
            curves.Add(newCurve);
        }
        looping = !looping;
    }

    public void AddCurve()
    {
        bool startedWithLoop = false;
        if (looping)
        {
            ToggleEnclosedLoop();
            startedWithLoop = true;
        }
        time = 0.0f;
        GameObject newCurve = Instantiate(curve, gameObject.transform.position, Quaternion.identity);
        curves.Add(newCurve);
        GameObject newKnot = Instantiate(knot, gameObject.transform.position, Quaternion.identity);
        knots.Add(newKnot);
        if (startedWithLoop)
        {
            ToggleEnclosedLoop();
        }
    }
    public void RemoveCurve()
    {
        time = 0.0f;
        if (looping)
        {
            ToggleEnclosedLoop();
        }
        else if (curves.Count > 1)
        {
            Destroy(curves[^1]);
            curves.Remove(curves[^1]);
            Destroy(knots[^1]);
            knots.Remove(knots[^1]);
        }
    }

    public Vector3 getPositionAtTime(float t)
    {
        int currentCurve = (int)(t / timePerCurve);
        if (currentCurve >= curves.Count)
        {
            currentCurve = curves.Count - 1;
        }
        if (curves[currentCurve] == null)
        {
            Debug.Log("No curve");
        }
        Vector3 pos = curves[currentCurve].GetComponent<BezierCurve>().GetPositionAtTime((t - (float)currentCurve * timePerCurve)/timePerCurve);
        
        return pos;
    }
    public Vector3 getForwardVectorAtTime(float t)
    {
        int currentCurve = (int)(t / timePerCurve);
        if (currentCurve >= curves.Count)
        {
            currentCurve = curves.Count - 1;
        }
        Vector3 velocity = curves[currentCurve].GetComponent<BezierCurve>().GetForwardVectorAtTime((t - (float)currentCurve * timePerCurve) / timePerCurve);
        return velocity;
    }

    // approximates the length of a spline by taking 100 or so samples from the start to the end of the spline
    public float getApproximateSplineLength()
    {
        float total = 0f;
        Vector3 previousPoint = getPositionAtTime(0);

        int sampleCount = 100;
        for (int i = 1; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount * curves.Count;
            Vector3 nextPoint = getPositionAtTime(t);

            total += Vector3.Distance(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }

        return total;
        // old method of doing this (less helpful and less accurate)
/*        
        float total = 0;
        foreach (GameObject curve in curves)
        {
            total += curve.GetComponent<BezierCurve>().GetApproximateCurveLength();
        }
        return total;
*/
    }
}
