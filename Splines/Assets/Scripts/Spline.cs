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
    bool looping;
    float timePerCurve;

    private void Start()
    {
        curves = new List<GameObject>();
        knots = new List<GameObject>();

        previousKnotRotations = new List<Quaternion>();
        previousKnotPositions = new List<Vector3>();
        currentKnotPositions = new List<Vector3>();
        currentKnotRotations = new List<Quaternion>();

        foreach (GameObject knot in knots)
        {
            currentKnotPositions.Add(knot.transform.position);
            currentKnotRotations.Add(knot.transform.rotation);
        }

        time = 0.0f;
        timePerCurve = 1.0f;
        looping = false;

        GameObject startingKnot = Instantiate(knot, gameObject.transform.position, Quaternion.identity);

        knots.Add(startingKnot);

        AddCurve();

        addCurveButton.onClick.AddListener(AddCurve);
        removeCurveButton.onClick.AddListener(RemoveCurve);
        toggleLoopButton.onClick.AddListener(ToggleEnclosedLoop);
    }

    private void Update()
    {
        objectFollowingSpline.transform.position = getPositionAtTime(time);
        objectFollowingSpline.transform.forward = getForwardVectorAtTime(time);

        foreach (GameObject knot in knots)
        {
            currentKnotPositions.Clear();
            currentKnotRotations.Clear();
            currentKnotPositions.Add(knot.transform.position);
            currentKnotRotations.Add(knot.transform.rotation);
        }

        time += Time.deltaTime;

        if (time > timePerCurve * curves.Count)
        {
            time = 0;
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

    public float getApproximateSplineLength()
    {
        float total = 0;
        foreach (GameObject curve in curves)
        {
            total += curve.GetComponent<BezierCurve>().GetApproximateCurveLength();
        }
        return total;
    }
}
