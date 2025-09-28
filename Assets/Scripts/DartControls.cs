using UnityEngine;
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
    private Vector3 lastMousePos;
    private bool isDragging;

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

        if (released)
        {
            // Throw logic should go here, physics related
            // calculate mouse velocity
            // calulate dart direction
            // apply gravity to dart rigidbody
            // apply direction and velocity to dart rigidbody
        }
    }

    private Vector3 CalculateMouseVelocity()
    {
        // Implement mouse velocity calculation
        return Vector3.zero;
    }

    private void OnDisable()
    {
        // Prevent a stuck-drag if object is disabled mid-interaction
        isDragging = false;
    }
}
