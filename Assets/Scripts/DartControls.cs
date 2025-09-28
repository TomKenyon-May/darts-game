using UnityEngine;
using UnityEngine.InputSystem;

public class DartControls : MonoBehaviour
{
    // Rigidbody component of the dart
    private Rigidbody rb;
    private Camera cam;
    Plane dragPlane;
    Vector3 grabOffset;
    Vector3 targetPos;
    bool isDragging;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Get the Rigidbody component attached to the dart
        rb = GetComponent<Rigidbody>();

        // If no camera is assigned, use the main camera
        if (!cam) cam = Camera.main;

        // Disable gravity
        rb.useGravity = false;

        // Render time smoothing and continuous collision detection
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        // Make the Rigidbody kinematic so the dart initially doesn't respond to physics
        rb.isKinematic = true;
    }

    void Update()
    {
        var mouse = Mouse.current;
        bool pressed = mouse.leftButton.wasPressedThisFrame;

        // If not dragging and mouse button not pressed, do nothing
        if (!isDragging && !pressed)
        {
            return;
        }

        bool released = mouse.leftButton.wasReleasedThisFrame;
        var mousePos = mouse.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

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

        if (isDragging)
        {
            if (dragPlane.Raycast(ray, out var distance))
            {
                var hitOnPlane = ray.GetPoint(distance);
                targetPos = hitOnPlane + grabOffset;
            }
        }

        if (released)
        {
            isDragging = false;
        }
    }

    // FixedUpdate is called at a fixed interval and is independent of frame rate
    void FixedUpdate()
    {
        if (isDragging)
        {
            rb.MovePosition(targetPos);
        }
    }
}
