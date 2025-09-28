using UnityEngine;
using UnityEngine.InputSystem;

public class DartControls : MonoBehaviour
{
    // Rigidbody component of the dart
    private Rigidbody rb;
    [SerializeField] Camera cam;
    [SerializeField] LayerMask pickMask;
    Plane dragPlane;
    Vector3 grabOffset;
    Vector3 targetPos;
    bool isDragging;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Get the Rigidbody component attached to the dart
        rb = GetComponent<Rigidbody>();

        if (!cam) cam = Camera.main;

        Debug.Log($"Awake: rb={rb != null}, cam={(cam ? cam.name : "NULL")}", this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Disable gravity
        rb.useGravity = false;

        // Render time smoothing and continuous collision detection
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        // Make the Rigidbody kinematic so the dart initially doesn't respond to physics
        rb.isKinematic = true;

        Debug.Log("Start: set to kinematic + no gravity", this);
    }

    void Update()
    {
        if (!isDragging && Mouse.current.leftButton.wasPressedThisFrame)
        {
            var mousePos = Mouse.current.position.ReadValue();
            var ray = cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, pickMask) &&
        (hit.rigidbody == rb || hit.transform == transform || hit.transform.IsChildOf(transform)))
            {
                dragPlane = new Plane(cam.transform.forward, transform.position);

                if (dragPlane.Raycast(ray, out var tPlane))
                {
                    var planeHit = ray.GetPoint(tPlane);
                    grabOffset = transform.position - planeHit;
                    targetPos = transform.position;
                    isDragging = true;
                    Debug.Log($"Started drag. planeHit={planeHit}, grabOffset={grabOffset}", this);
                }
            }
        }

        if (isDragging)
        {
            var mousePos = Mouse.current.position.ReadValue();
            var ray = cam.ScreenPointToRay(mousePos);

            if (dragPlane.Raycast(ray, out var t))
            {
                var hitOnPlane = ray.GetPoint(t);
                targetPos = hitOnPlane + grabOffset;
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
            Debug.Log($"Ended drag at {targetPos}", this);
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
