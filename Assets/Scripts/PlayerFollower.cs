using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFollower : MonoBehaviour
{
    public Transform root;
    public float followStrength = 50f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        
        Vector3 toTarget = (root.position - transform.position);
        Vector3 desiredVel = toTarget * followStrength;
        
        Vector3 velocityChange = desiredVel - rb.linearVelocity;
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
        
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(rb.linearVelocity.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 10f));
        }
    }
}