using UnityEngine;

public class BearBehavior : MonoBehaviour
{
    public Transform playerTr;
    public float moveSpeed, turnSpeed;
    public float maxShakeDistance;
    public float shakeIntensity;
    private Camera mainCam;
    private CameraBehavior cam;
    private Rigidbody2D rb;

    void Awake()
    {
        mainCam = Camera.main;
        cam = mainCam.GetComponent<CameraBehavior>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 direction = (playerTr.position - transform.position).normalized;
        rb.AddForce(direction * moveSpeed);
        rb.AddTorque(90 + Mathf.DeltaAngle(rb.rotation, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) * turnSpeed);
    }

    void ShakeCamera()
    {
        if(Vector2.Distance(transform.position, (Vector2)mainCam.transform.position) < maxShakeDistance)
        {
            StartCoroutine(cam.ShakeCamera(0.1f, (maxShakeDistance - Vector2.Distance(transform.position, (Vector2)mainCam.transform.position)) * shakeIntensity));
        }
    }
}
