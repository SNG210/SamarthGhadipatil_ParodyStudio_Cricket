using UnityEngine;

public enum ThrowState { Swing, Spin }

public class Ball : MonoBehaviour
{
    private Vector3 pointA;
    public Vector3 pointB;
    public Vector3 pointC;
    public Vector3 CurvePoint1;
    public Vector3 CurvePoint2;

    private float speed = 1.0f;
    private float originalSpeed;
    private float speedDampning;
    private float t = 0f;
    private bool toSecondCurve = false;

    public float bounceDampning = 1f;

    public ThrowState currentThrowState = ThrowState.Swing;

    public void InitializeBall(Vector3 pointB, ThrowState currentThrowState, float swingDiveation, float spinDiveation, float speed, float throwheight, float heightDampning, float distanceDampning, float speedDampning)
    {
        this.speed = speed;
        originalSpeed = speed;
        this.speedDampning = speedDampning;
        this.pointB = pointB;
        pointA = transform.position;

        if (currentThrowState == ThrowState.Swing)
        {
            Vector3 tempCurvePoint1 = (pointA + pointB) * 0.5f;
            CurvePoint1 = new Vector3(tempCurvePoint1.x + swingDiveation, throwheight, tempCurvePoint1.z);
            Vector3 directionBc1 = (CurvePoint1 - pointB) * 2;
            Vector3 rawC = Vector3.ProjectOnPlane((pointB - directionBc1), Vector3.up);
            pointC = Vector3.Lerp(pointB, rawC, 1f - distanceDampning);

        }
        else if (currentThrowState == ThrowState.Spin)
        {
            CurvePoint1 = (pointA + pointB) * 0.5f;
            CurvePoint1.y = throwheight;
            Vector3 directionBA = (pointA - pointB);
            Vector3 tempPointC = Vector3.ProjectOnPlane((pointB - directionBA), Vector3.up);
            Vector3 rawC = new Vector3(tempPointC.x + spinDiveation, tempPointC.y, tempPointC.z);
            pointC = Vector3.Lerp(pointB, rawC, 1f - distanceDampning);

        }

        CurvePoint2 = (pointB + pointC) * 0.5f;
        float originalHeight = CurvePoint2.y;
        CurvePoint2.y = Mathf.Lerp(throwheight, 0, heightDampning);

    }


    private void Update()
    {
        t += Time.deltaTime * speed;

        if (!toSecondCurve)
        {
            transform.position = GetQuadraticBezierCurvePoint(t, pointA, CurvePoint1, pointB);

            if (t >= 1f)
            {
                t = 0f;
                toSecondCurve = true;
                speed = Mathf.Lerp(originalSpeed, 0f, speedDampning);
            }
        }
        else
        {
            transform.position = GetQuadraticBezierCurvePoint(t, pointB, CurvePoint2, pointC);
        }
    }

    Vector3 GetQuadraticBezierCurvePoint(float t, Vector3 p0, Vector3 c, Vector3 p1)
    {
        return Mathf.Pow(1 - t, 2) * p0 + 2 * (1 - t) * t * c + Mathf.Pow(t, 2) * p1;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(pointA, 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pointB, 0.2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pointC, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(CurvePoint1, 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(CurvePoint2, 0.2f);

        // Draw lines between points to visualize paths
        Gizmos.color = Color.white;
        Gizmos.DrawLine(pointA, CurvePoint1);
        Gizmos.DrawLine(CurvePoint1, pointB);
        Gizmos.DrawLine(pointB, CurvePoint2);
        Gizmos.DrawLine(CurvePoint2, pointC);
    }
}