using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Wheel Positions")]
    public Transform FLWheel;
    public Transform FRWheel;
    public Transform RLWheel;
    public Transform RRWheel;
    
    [Header("Tire Visual Meshes")]
    public Transform FLTireMesh;
    public Transform FRTireMesh;
    public Transform RLTireMesh;
    public Transform RRTireMesh;

    [Header("Suspension Settings")]
    public float suspensionRestDistance = 0.2f;
    public float springStrength = 20000f;
    public float springDamper = 3000f; //force that resists spring movement
    public float wheelRadius = 0.375f;

    private Rigidbody carRigidbody;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        DoSuspension(FLWheel, FLTireMesh);
        DoSuspension(FRWheel, FRTireMesh);
        DoSuspension(RLWheel, RLTireMesh);
        DoSuspension(RRWheel, RRTireMesh);
    }

    void DoSuspension(Transform wheel, Transform tireMesh)
    {
        
        //trace ray from the center of the wheel to the ground
        Vector3 rayStart = wheel.position;
        Vector3 rayDirection = -transform.up;
        float rayLength = suspensionRestDistance + wheelRadius;
        
        RaycastHit groundHit;
        bool isGrounded = Physics.Raycast(rayStart, rayDirection, out groundHit, rayLength);

        float suspensionLength = suspensionRestDistance;
        
        if (isGrounded)
        {
            //calculate the spring force based on how much spring is compressed
            suspensionLength = groundHit.distance - wheelRadius;
            float springCompression = suspensionRestDistance - suspensionLength;
            float springForce = springCompression * springStrength;
            
            //calculate the damper force
            Vector3 wheelVelocity = carRigidbody.GetPointVelocity(wheel.position);
            float wheelUpSpeed = Vector3.Dot(wheelVelocity, transform.up);
            float damperForce = wheelUpSpeed * springDamper;
            
            float totalForce = springForce - damperForce;
            
            //apply the total force
            Vector3 forceVector = Vector3.up * totalForce;
            carRigidbody.AddForceAtPosition(forceVector, wheel.position);
        }
        
        //tire viusals need to be updated manually
        UpdateTireVisual(wheel, tireMesh, suspensionLength);
    }
    
    void UpdateTireVisual(Transform wheel, Transform tireMesh, float suspensionLength)
    {
        //calculate the position based on how much the spring is compressed
        Vector3 tirePosition = wheel.position;
        tirePosition += (-transform.up) * suspensionLength;
        tireMesh.position = tirePosition;
        
    }
}