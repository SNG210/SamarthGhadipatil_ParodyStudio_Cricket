using UnityEngine;
using System.Collections.Generic;

public class Ball : MonoBehaviour
{
    private int currentSegment = 0;
    private float t = 0f;

    private List<Vector3> pointAList = new List<Vector3>();
    private List<Vector3> pointBList = new List<Vector3>();
    private List<Vector3> controlList = new List<Vector3>();
    private List<float> speedPerSegment = new List<float>();

    public void InitializeBall(Vector3 firstTarget, ThrowState throwState, float swingDeviation, float spinDeviation, float speed, float throwHeight, float heightDampning, float distanceDampning, float speedDampning, int numberOfBounces)
    {
        pointAList.Clear();
        pointBList.Clear();
        controlList.Clear();

        Vector3 start = transform.position;
        Vector3 end = firstTarget;

        for (int i = 0; i < numberOfBounces; i++)
        {
            Vector3 control;
            Vector3 nextPointC;

            float currentSwingDeviation = (i == 0) ? swingDeviation : 0f;
            float currentSpinDeviation = (i == 0) ? spinDeviation : 0f;

            if (throwState == ThrowState.Swing)
            {
                Vector3 mid = (start + end) * 0.5f;
                control = new Vector3(mid.x + currentSwingDeviation, throwHeight, mid.z);

                Vector3 direction = (control - end) * 2;
                Vector3 rawC = Vector3.ProjectOnPlane((end - direction), Vector3.up);
                nextPointC = Vector3.Lerp(end, rawC, 1f - distanceDampning);
            }
            else // Spin
            {
                Vector3 mid = (start + end) * 0.5f;
                control = mid;
                control.y = throwHeight;

                Vector3 direction = (start - end);
                Vector3 tempPointC = Vector3.ProjectOnPlane((end - direction), Vector3.up);
                Vector3 rawC = new Vector3(tempPointC.x + currentSpinDeviation, tempPointC.y, tempPointC.z);
                nextPointC = Vector3.Lerp(end, rawC, 1f - distanceDampning);
            }

            float dampenedHeight = throwHeight * Mathf.Pow(1f - heightDampning, i);
            control.y = dampenedHeight;

            pointAList.Add(start);
            pointBList.Add(end);
            controlList.Add(control);
            speedPerSegment.Add(speed);

            start = end;
            end = nextPointC;

            speed *= (1f - speedDampning);
        }
    }

    private void Update()
    {
        if (currentSegment >= pointAList.Count)
            return;

        t += Time.deltaTime * speedPerSegment[currentSegment];

        if (t >= 1f)
        {
            t = 0f;
            currentSegment++;

            if (currentSegment >= pointAList.Count)
                return;
        }

        if (currentSegment < pointAList.Count)
        {
            transform.position = GetQuadraticBezierCurvePoint(t, pointAList[currentSegment], controlList[currentSegment], pointBList[currentSegment]);
        }
    }

    Vector3 GetQuadraticBezierCurvePoint(float t, Vector3 p0, Vector3 c, Vector3 p1)
    {
        return Mathf.Pow(1 - t, 2) * p0 + 2 * (1 - t) * t * c + Mathf.Pow(t, 2) * p1;
    }

    private void OnDrawGizmos()
    {
        if (pointAList.Count == 0 || controlList.Count == 0 || pointBList.Count == 0)
            return;

        int resolution = 20; 

        for (int i = 0; i < pointAList.Count; i++)
        {
            Vector3 prevPoint = pointAList[i];

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointAList[i], 0.15f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pointBList[i], 0.15f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(controlList[i], 0.15f);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(pointAList[i], controlList[i]);
            Gizmos.DrawLine(controlList[i], pointBList[i]);

            Gizmos.color = Color.cyan;
            for (int j = 1; j <= resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 point = GetQuadraticBezierCurvePoint(t, pointAList[i], controlList[i], pointBList[i]);
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }
    }


}
