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
    private float releasePoint;
    private bool isDragging;
    private bool startedDrag;
    private const float throwVelocityScalerX = 0.0002f;
    private const float throwVelocityScalerY = 0.0001f;
    private const float throwVelocityScalerZ = 0.0023f;

    private bool released = false;

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
                mouseDelta = mouse.position.ReadValue() - lastMousePos;
                releasePoint = Screen.currentResolution.height / mouse.position.ReadValue().y;
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
        
        if (startedDrag)
        {
            if (released)
            {
                float releaseVelocity = releasePoint * mouseVelocity.magnitude;
                float throwY = releaseVelocity * throwVelocityScalerY;
                float throwX = mouseVelocity.x * throwVelocityScalerX;
                float throwZ = mouseVelocity.y * throwVelocityScalerZ;
                Vector3 throwVelocity = new Vector3(throwX, throwY, throwZ);
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = throwVelocity;
                Debug.Log($"Applied Velocity: {rb.linearVelocity}");
                released = false;
            }
        }
    }

    private void OnDisable()
    {
        // Prevent a stuck-drag if object is disabled mid-interaction
        isDragging = false;
    }
}
