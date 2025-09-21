using UnityEngine;
using Unity.Cinemachine;

public class CarController : MonoBehaviour
{
    [Header("Player Control")]
    public bool playerInCar = false;
    
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
    
    [Header("Cinemachine Cameras")]
    public CinemachineCamera backCamera;
    public CinemachineCamera rightCamera;
    public CinemachineCamera leftCamera;
    
    [Header("Camera Animation")]
    public float cameraAnimationSpeed = 1f;
    private Vector3 backCameraOriginalPos;
    private Vector3 rightCameraOriginalPos;
    private Vector3 leftCameraOriginalPos;
    
    private bool FLWheelSteers = true;      //front wheels steer
    private bool FRWheelSteers = true;
    private bool RLWheelSteers = false;     //rear wheels don't steer
    private bool RRWheelSteers = false;

    private Rigidbody carRigidbody;
    private float currentSteerAngle = 0f;
    
    private float currentAcceleration = 0f;
    private float lastVelocity = 0f;
    
    private float accelInput = 0f;
    private float steerInput = 0f;
    private float brakeInput = 0f;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        
        backCameraOriginalPos = backCamera.transform.localPosition;
        rightCameraOriginalPos = rightCamera.transform.localPosition;
        leftCameraOriginalPos = leftCamera.transform.localPosition;
        
        //create default power curve if not set
        if (powerCurve.keys.Length == 0)
        {
            powerCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.5f);
        }
    }

    void Update()
    {

        HandlePlayerInput();
        HandlePhysicsAndVisuals();
    }
    
    void HandlePlayerInput()
    {
        if (playerInCar)
        {
            // Get input from player
            accelInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
            brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
            
            currentSteerAngle = steerInput * maxSteerAngle;
            
            CalculateAcceleration();
            AnimateCamera(accelInput, brakeInput);
        }
        else
        {
            accelInput = 0f;
            steerInput = 0f;
            brakeInput = 0f;
            currentSteerAngle = 0f;
        }
    }
    
    void HandlePhysicsAndVisuals()
    {
        //physics always runs regardless of player presence
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

            //gas/brake physics (will be 0 when player not in car)
            DoGasBrake(wheel, isPowered, accelInput, brakeInput);

            //friction always applied
            DoFriction(wheel);
        }
        
        //tire visuals always updated
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
    
    void AnimateCamera(float accelInput, float brakeInput)
    {
        float targetXOffset = 0f;
    
        if (currentAcceleration > 1f)
            targetXOffset = -0.5f;
        else if (currentAcceleration < -1f)
            targetXOffset = 0.25f;
        
        if (currentSteerAngle == 0)
        {
            SetCameraPriorities(backCamera, rightCamera, leftCamera);
            AnimateCameraPosition(backCamera, backCameraOriginalPos, targetXOffset);
        }
        else if (currentSteerAngle > 0)
        {
            SetCameraPriorities(rightCamera, backCamera, leftCamera);
            AnimateCameraPosition(rightCamera, rightCameraOriginalPos, targetXOffset);
        }
        else
        {
            SetCameraPriorities(leftCamera, backCamera, rightCamera);
            AnimateCameraPosition(leftCamera, leftCameraOriginalPos, targetXOffset);
        }
    }

    void AnimateCameraPosition(CinemachineCamera camera, Vector3 originalPos, float xOffset)
    {
        Vector3 targetPos = originalPos + new Vector3(xOffset, 0, 0);
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, targetPos, Time.deltaTime * cameraAnimationSpeed);
    }
    
    void SetCameraPriorities(CinemachineCamera active, CinemachineCamera passive1, CinemachineCamera passive2)
    {
        active.Priority = 1;
        passive1.Priority = 0;
        passive2.Priority = 0;
    }
    
    void CalculateAcceleration()
    {
        Vector3 forwardDir = transform.right;
        float currentVelocity = Vector3.Dot(carRigidbody.linearVelocity, forwardDir);
        
        currentAcceleration = (currentVelocity - lastVelocity) / Time.deltaTime;
        lastVelocity = currentVelocity;
        
    }
    
}