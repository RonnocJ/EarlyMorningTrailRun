using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerBehavior : MonoBehaviour
{
    public float moveForce, maxSpeed, lightAngleScale;
    private Camera mainCam;
    private Rigidbody2D playerRb;
    private Light2D flashlight;
    private Vector2 lookPos, direction;
    private float distanceFromMouse, angleRange;
    void Start()
    {
        mainCam = Camera.main;
        playerRb = GetComponent<Rigidbody2D>();
        flashlight = transform.GetChild(0).GetComponent<Light2D>();
    }
    void FixedUpdate()
    {
        lookPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        direction = lookPos - (Vector2)transform.position;
        distanceFromMouse = Mathf.Clamp(Vector2.Distance(lookPos, (Vector2)transform.position), 0f, 10f);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        if (Input.GetMouseButton(0))
        {
            Vector2 force = direction * moveForce * 
                ((Mathf.Abs(lookPos.x - transform.position.x) / 8f > Mathf.Abs(lookPos.y - transform.position.y) / 4.5f) ?
                Mathf.Abs(lookPos.x - transform.position.x) / 8f : Mathf.Abs(lookPos.y - transform.position.y) / 4.5f);
            
            playerRb.AddForce(force, ForceMode2D.Force);
        }

        if (playerRb.linearVelocity.magnitude > maxSpeed)
        {
            playerRb.linearVelocity = playerRb.linearVelocity.normalized * maxSpeed;
        }

        angleRange = Mathf.Clamp(100 - ((distanceFromMouse - 0.5f) * lightAngleScale), 10f, 100f);
        flashlight.pointLightOuterAngle = angleRange;
        flashlight.pointLightInnerAngle = angleRange / 2f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name.Contains("Bush"))
        {
            playerRb.linearDamping = 40;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.name.Contains("Bush"))
        {
            playerRb.linearDamping = 10;
        }
    }
}
