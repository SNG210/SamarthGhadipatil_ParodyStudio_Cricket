using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Aim : MonoBehaviour
{
    [Header("General")]
    public float sensitivity = 30f;
    public Vector2 xBounds;
    public Vector2 zBounds;

    private Vector3 targetPosition;

    [Header("References")]
    public Transform crosshair;
    public GameObject ballPrefab;
    public ThrowMeter throwMeter;

    [Header("Spawn Points")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    private Transform selectedSpawnPoint;

    [Header("UI")]
    public Button spawnPointButton;
    public GameObject spawnPointMarkerPrefab;

    private GameObject currentMarker;

    [Header("Throw Settings")]
    public float speed = 1.0f;
    public float throwheight = 20f;

    [Space]
    public float swingAmount = 30f;
    [Range(-1f, 1f)] public float swingDiveation = 0f;

    [Space]
    public float spinAmount = 2.5f;
    [Range(-1f, 1f)] public float spinDiveation = 0f;

    [Space]
    [Range(0f, 1f)] public float heightDampening = 0.2f;
    [Range(0f, 1f)] public float distanceDampening = 0.6f;
    [Range(0f, 1f)] public float speedDampening = 0.1f;

    [Header("Throw Control")]
    public ThrowState currentThrowState = ThrowState.Swing;
    public bool ShouldAddError = true;

    private bool isAiming = true;
    private bool isMeterActive = false;
    private bool readyForThrow = true;

    private float errorAmount = 0f;

    void Start()
    {
        selectedSpawnPoint = spawnPoint1;
        targetPosition = crosshair.position;

        if (spawnPointButton != null)
            spawnPointButton.onClick.AddListener(ToggleSpawnPoint);

        PlaceMarker();
    }

    void Update()
    {
        if (!readyForThrow) return;

        if (isAiming)
        {
            HandleAiming();
            HandleKeyboardSpawnSwitch();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                targetPosition = crosshair.position;
                isAiming = false;

                if (ShouldAddError)
                {
                    isMeterActive = true;
                    throwMeter.StartMeter();
                }
                else
                {
                    LaunchBall(0f);
                    StartCoroutine(ResetAfterDelay());
                }
            }
        }
        else if (isMeterActive && Input.GetKeyDown(KeyCode.Space))
        {
            isMeterActive = false;
            errorAmount = throwMeter.StopMeterAndGetError();
            LaunchBall(errorAmount);
            StartCoroutine(ResetAfterDelay());
        }
    }

    IEnumerator ResetAfterDelay()
    {
        readyForThrow = false;
        yield return new WaitForSeconds(1f);
        isAiming = true;
        readyForThrow = true;
    }

    void HandleAiming()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, 0, moveZ) * sensitivity * Time.deltaTime;
        targetPosition += movement;

        targetPosition.x = Mathf.Clamp(targetPosition.x, xBounds.x, xBounds.y);
        targetPosition.z = Mathf.Clamp(targetPosition.z, zBounds.x, zBounds.y);

        crosshair.position = targetPosition;
    }

    void HandleKeyboardSpawnSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetSpawnPoint(spawnPoint1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetSpawnPoint(spawnPoint2);
        }
    }

    void ToggleSpawnPoint()
    {
        if (selectedSpawnPoint == spawnPoint1)
            SetSpawnPoint(spawnPoint2);
        else
            SetSpawnPoint(spawnPoint1);
    }

    void SetSpawnPoint(Transform newSpawnPoint)
    {
        selectedSpawnPoint = newSpawnPoint;
        PlaceMarker();
    }

    void PlaceMarker()
    {
        if (spawnPointMarkerPrefab == null || selectedSpawnPoint == null) return;

        if (currentMarker != null)
            Destroy(currentMarker);

        currentMarker = Instantiate(spawnPointMarkerPrefab, selectedSpawnPoint.position, Quaternion.identity);
    }

    void LaunchBall(float error)
    {
        GameObject ball = Instantiate(ballPrefab, selectedSpawnPoint.position, Quaternion.identity);

        Vector3 throwTarget = targetPosition;
        float swing = swingDiveation * swingAmount;
        float spin = spinDiveation * spinAmount;
        float throwSpeed = speed;

        if (ShouldAddError)
        {
            ApplyError(ref throwTarget, ref swing, ref spin, ref throwSpeed, error);
        }

        ball.GetComponent<Ball>().InitializeBall(
            throwTarget,
            currentThrowState,
            swing,
            spin,
            throwSpeed,
            throwheight,
            heightDampening,
            distanceDampening,
            speedDampening
        );

        Destroy(ball, 5f);
    }

    void ApplyError(ref Vector3 throwTarget, ref float swing, ref float spin, ref float throwSpeed, float error)
    {
        Vector3 positionOffset = new Vector3(Random.Range(-error, error), 0, Random.Range(-error, error));
        throwTarget += positionOffset;

        swing = Mathf.Lerp(swing, 0, Mathf.Abs(error));
        spin = Mathf.Lerp(spin, 0, Mathf.Abs(error));

        float minSpeed = 0.1f;
        throwSpeed = Mathf.Lerp(throwSpeed, minSpeed, Mathf.Abs(error));
    }

    void OnValidate()
    {
        heightDampening = Mathf.Clamp(heightDampening, 0f, 1f);
        distanceDampening = Mathf.Clamp(distanceDampening, 0f, 1f);
        speedDampening = Mathf.Clamp(speedDampening, 0f, 1f);
    }

    void OnDrawGizmos()
    {
        if (selectedSpawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(selectedSpawnPoint.position, 0.5f);
        }
    }
}
