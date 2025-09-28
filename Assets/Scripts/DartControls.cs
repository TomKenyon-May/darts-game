using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
// using UnityEngine.EventSystems; // Uncomment if you want the UI guard

[RequireComponent(typeof(Rigidbody))]
public class DartControls : MonoBehaviour
{
    private Rigidbody rb;
    private Camera cam;
    private Plane dragPlane;
    private Vector3 grabOffset;
    private Vector3 targetPos;

    private Vector2 lastMousePos;
    private Vector2 mouseDelta = Vector2.zero;
    private Vector2 mouseVelocity = Vector2.zero;
    
    private float releaseMouseY;
    private bool isDragging;
    private bool startedDrag;
    private bool released = false;

    private const float impulseScalerX = 0.00001f;
    private const float impulseScalerZ = 0.00006f;
    private const float impulseScalerTheta = 0.3f;
    private const float maxTheta = 60f;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = cam ? cam : Camera.main;

        if (!cam)
        {
            Debug.LogError("DartControls: No camera found. Assign one or tag a camera as MainCamera.", this);
        }

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        rb.isKinematic = true;

        lastMousePos = Mouse.current.position.ReadValue();
    }

    // Update is called once per frame
    void Update()
    {
        // Input device may be null (rare, but safe to guard).
        var mouse = Mouse.current;
        if (mouse == null || cam == null) return;

        // Optional UI guard
        // if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

        bool pressed = mouse.leftButton.wasPressedThisFrame;

        // If not dragging and mouse button not pressed, do nothing
        if (!isDragging && !pressed) return;

        released = mouse.leftButton.wasReleasedThisFrame;
        var mousePos = mouse.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        // Try to start dragging
        if (!isDragging && pressed)
        {
            if (Physics.Raycast(ray, out var hit) && hit.rigidbody == rb)
            {
                dragPlane = new Plane(cam.transform.forward, rb.position);

                if (dragPlane.Raycast(ray, out var distance))
                {
                    var planeHit = ray.GetPoint(distance);
                    grabOffset = rb.position - planeHit;
                    targetPos = rb.position;
                    isDragging = true;
                    startedDrag = true;
                }
            }
        }

        // Update or stop dragging
        if (isDragging)
        {
            if (dragPlane.Raycast(ray, out var distance))
            {
                var planeHit = ray.GetPoint(distance);
                targetPos = planeHit + grabOffset;
            }

            if (released)
            {
                mouseDelta = mousePos - lastMousePos;
                releaseMouseY = mouse.position.ReadValue().y;
                mouseVelocity = mouseDelta / Time.deltaTime;
                isDragging = false;
            }
        }

        lastMousePos = mouse.position.ReadValue();
    }

    // FixedUpdate is called at a fixed interval and is independent of frame rate
    void FixedUpdate()
    {
        if (isDragging)
        {
            rb.MovePosition(targetPos);
        }

        if (startedDrag && released)
        {
            float impulseX = mouseVelocity.x * impulseScalerX;
            float impulseZ = mouseVelocity.y * impulseScalerZ;

            float screenHeight = (float)Screen.currentResolution.height;
            float yNorm = 1f - (releaseMouseY / screenHeight);

            float theta = yNorm * maxTheta;

            float h = Mathf.Sqrt(impulseX * impulseX + impulseZ * impulseZ);

            float impulseY = Mathf.Tan(theta * Mathf.Deg2Rad) * h * impulseScalerTheta;

            Vector3 impulse = new Vector3(impulseX, impulseY, impulseZ);

            rb.isKinematic = false;
            rb.useGravity = true;

            rb.AddForce(impulse, ForceMode.Impulse);

            Debug.Log($"Applied Impulse: {impulse:F6}  -> mass={rb.mass}, Δv ≈ {impulse / rb.mass}");

            startedDrag = false;
            released = false;
        }
    }

    private void OnDisable()
    {
        // Prevent a stuck-drag if object is disabled mid-interaction
        isDragging = false;
    }
}
