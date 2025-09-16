using UnityEngine;
using UnityEngine.Serialization;

public class CarController : MonoBehaviour
{
    [Header("Wheel Colliders")] 
    public WheelCollider FLWheelCollider;
    public WheelCollider FRWheelCollider;
    public WheelCollider RLWheelCollider;
    public WheelCollider RRWheelCollider;

    [Header("Wheel Transforms (Visual)")]
    public Transform FLWheelMesh;
    public Transform FRWheelMesh;


    [Header("Car Settings")] 
    public float maxMotorPower = 1500f;
    public float maxSteerAngle = 30f;
    public float brakePower = 4000f;

    void Start()
    {
        FLWheelCollider.steerAngle = 90f;
        FRWheelCollider.steerAngle = 90f;
        RRWheelCollider.steerAngle = 90f;
        RLWheelCollider.steerAngle = 90f;
    }
    
    void Update()
    {
        float motorInput = Input.GetAxis("Vertical");
        float steerInput = Input.GetAxis("Horizontal");
        float brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        
        float steerAngle = steerInput * maxSteerAngle;
        FLWheelCollider.steerAngle = steerAngle + 90f;
        FRWheelCollider.steerAngle = steerAngle + 90f;

        
        RLWheelCollider.motorTorque = motorInput * maxMotorPower;
        RRWheelCollider.motorTorque = motorInput * maxMotorPower;

        
        RLWheelCollider.brakeTorque = brakeInput * brakePower;
        RRWheelCollider.brakeTorque = brakeInput * brakePower;
        
        FLWheelMesh.localRotation = Quaternion.Euler(90f, steerAngle, 0f);
        FRWheelMesh.localRotation = Quaternion.Euler(90f, steerAngle, 0f);
    }
    
}