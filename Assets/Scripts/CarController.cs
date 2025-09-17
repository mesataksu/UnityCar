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
    public float springDamper = 3000f;
    public float wheelRadius = 0.375f;

    [Header("Gas/Brake Settings")]
    public float maxSpeed = 60f;
    public float maxGasTorque = 2000f;
    public float maxBrakeTorque = 4000f;
    public AnimationCurve powerCurve;

    [Header("Wheel Drive Settings")]
    public bool FLWheelPowered = false;
    public bool FRWheelPowered = false; 
    public bool RLWheelPowered = true;
    public bool RRWheelPowered = true;

    private Rigidbody carRigidbody;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        
        //create default power curve if not set
        if (powerCurve.keys.Length == 0)
        {
            powerCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.5f);
        }
    }

    void Update()
    {
        //get input
        float accelInput = Input.GetAxis("Vertical");
        float brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        //do suspension and acceleration for each wheel
        DoWheelPhysics(FLWheel, FLTireMesh, FLWheelPowered, accelInput, brakeInput);
        DoWheelPhysics(FRWheel, FRTireMesh, FRWheelPowered, accelInput, brakeInput);
        DoWheelPhysics(RLWheel, RLTireMesh, RLWheelPowered, accelInput, brakeInput);
        DoWheelPhysics(RRWheel, RRTireMesh, RRWheelPowered, accelInput, brakeInput);
    }

    void DoWheelPhysics(Transform wheel, Transform tireMesh, bool isPowered, float accelInput, float brakeInput)
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

            //gas/brake physics
            DoGasBrake(wheel, isPowered, accelInput, brakeInput);
        }
        
        //tire viusals need to be updated manually
        UpdateTireVisual(wheel, tireMesh, suspensionLength);
    }

    void DoGasBrake(Transform wheel, bool isPowered, float accelInput, float brakeInput)
    {
        
        Vector3 dir = wheel.right;

        //get car speed 
        float carSpeed = Vector3.Dot(dir, carRigidbody.linearVelocity);
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / maxSpeed);

        //apply acceleration if this wheel is powered
        if (isPowered)
        {
            float availableTorque = powerCurve.Evaluate(normalizedSpeed) * maxGasTorque * accelInput;
            carRigidbody.AddForceAtPosition(dir * availableTorque, wheel.position);
        }

        //apply brake
        Vector3 brakeDirection = carSpeed > 0 ? -dir : dir;
        float brakeForce = maxBrakeTorque * brakeInput;
        carRigidbody.AddForceAtPosition(brakeDirection * brakeForce, wheel.position);

    }

    
    void UpdateTireVisual(Transform wheel, Transform tireMesh, float suspensionLength)
    {
        //calculate the position based on how much the spring is compressed
        Vector3 tirePosition = wheel.position;
        tirePosition += (-transform.up) * suspensionLength;
        tireMesh.position = tirePosition;
    }
    
}