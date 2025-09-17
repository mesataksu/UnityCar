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
    
    [Header("Steering Settings")]
    public float maxSteerAngle = 30f;
    
    [Header("Friction Settings")]
    public float frictionCoefficient = 1.0f;
    public float tireMass = 20f;
    
    private bool FLWheelSteers = true;      //front wheels steer
    private bool FRWheelSteers = true;
    private bool RLWheelSteers = false;     //rear wheels don't steer
    private bool RRWheelSteers = false;

    private Rigidbody carRigidbody;
    private float currentSteerAngle = 0f;

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
        float steerInput = Input.GetAxis("Horizontal");
        float brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        currentSteerAngle = steerInput * maxSteerAngle;
        
        //do physics for each wheel
        DoWheelPhysics(FLWheel, FLTireMesh, FLWheelPowered, FLWheelSteers, accelInput, brakeInput);
        DoWheelPhysics(FRWheel, FRTireMesh, FRWheelPowered, FRWheelSteers, accelInput, brakeInput);
        DoWheelPhysics(RLWheel, RLTireMesh, RLWheelPowered, RLWheelSteers, accelInput, brakeInput);
        DoWheelPhysics(RRWheel, RRTireMesh, RRWheelPowered, RRWheelSteers, accelInput, brakeInput);
    }

    void DoWheelPhysics(Transform wheel, Transform tireMesh, bool isPowered, bool canSteer, float accelInput, float brakeInput)
    {
        if (canSteer)
        {
            //apply steering
            wheel.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
        }
        
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

            DoFriction(wheel);
        }
        
        //tire viusals need to be updated manually
        UpdateTireVisual(wheel, tireMesh, suspensionLength);
    }

    void DoGasBrake(Transform wheel, bool isPowered, float accelInput, float brakeInput)
    {
        //right is forward
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
    
    void DoFriction(Transform wheel)
    {
        
        Vector3 tireWorldVel = carRigidbody.GetPointVelocity(wheel.position);

        //right is forward
        Vector3 forwardDir = wheel.right;
        Vector3 sidewaysDir = wheel.forward; 
        float forwardVel = Vector3.Dot(forwardDir, tireWorldVel);
        float sidewaysVel = Vector3.Dot(sidewaysDir, tireWorldVel);

        //resist forward velocity
        float forwardVelChange = -forwardVel * frictionCoefficient;
        float forwardAccel = forwardVelChange / Time.fixedDeltaTime;
        Vector3 forwardForce = forwardDir * forwardAccel * tireMass;

        //resist sideways velocity
        float sidewaysVelChange = -sidewaysVel * frictionCoefficient;
        float sidewaysAccel = sidewaysVelChange / Time.fixedDeltaTime;
        Vector3 sidewaysForce = sidewaysDir * sidewaysAccel * tireMass;
        
        carRigidbody.AddForceAtPosition(forwardForce + sidewaysForce, wheel.position);
    }
    
    void UpdateTireVisual(Transform wheel, Transform tireMesh, float suspensionLength)
    {
        //calculate the position based on how much the spring is compressed
        Vector3 tirePosition = wheel.position;
        tirePosition += (-transform.up) * suspensionLength;
        tireMesh.position = tirePosition;
    }
    
}