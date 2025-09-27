using UnityEngine;

public class DartControls : MonoBehaviour
{
    // Rigidbody component of the dart
    private Rigidbody rb;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Get the Rigidbody component attached to the dart
        rb = GetComponent<Rigidbody>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Disable gravity
        rb.useGravity = false;

        // Set initial velocities to zero
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Render time smoothing and continuous collision detection
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        // Make the Rigidbody kinematic so the dart initially doesn't respond to physics
        rb.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
