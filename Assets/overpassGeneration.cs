using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.Splines;

public class overpassGeneration : MonoBehaviour
{
    public PathCreator path;
    public SplineContainer splineContainer;
    public SplineInstantiate splineInstantiate;
    // Start is called before the first frame update
    void Start()
    {
        var spline = splineContainer.AddSpline();
        var knots = new BezierKnot[path.path.NumPoints];
        for(int i = 0; i < path.path.NumPoints; i++)
        {
            knots[i] = new BezierKnot(path.path.GetPoint(i));
        }
        spline.Knots = knots;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
