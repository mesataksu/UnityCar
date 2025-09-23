using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public CinemachineCamera playerCamera;

    [Header("Physics Suspension")]
    public float height = 1.5f;
    public float springStrength = 100f;
    public float springDamper = 10f;

    private Rigidbody rb;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool isGrounded = true;
    
    private Vector3 downDir = Vector3.down;
    private RaycastHit rayHit;
    private bool rayDidHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        yRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        HandleMouseLook();
        HandleJump();
    }

    void FixedUpdate()
    {
        HandleSuspension();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        yRotation += mouseX;
        
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        rb.MoveRotation(Quaternion.Euler(0f, yRotation, 0f));
    }

    void HandleSuspension()
    {
        
        Vector3 rayOrigin = transform.position;
        Vector3 rayDir = transform.TransformDirection(downDir);
        float rayDistance = height;
        
        rayDidHit = Physics.Raycast(rayOrigin, rayDir, out rayHit, rayDistance);
        
        if (rayDidHit)
        {
            Vector3 vel = rb.linearVelocity;
            Vector3 otherVel = Vector3.zero;
            Rigidbody hitBody = rayHit.rigidbody;
            
            if (hitBody != null)
                otherVel = hitBody.linearVelocity;
            
            float rayDirVel = Vector3.Dot(rayDir, vel);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);
            float relVel = rayDirVel - otherDirVel;
            float x = rayHit.distance - height;
            float springForce = (x * springStrength) - (relVel * springDamper);
            
            rb.AddForce(rayDir * springForce);
            
            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(rayDir * -springForce, rayHit.point);
            }
            
            isGrounded = rayHit.distance <= height;

            Debug.DrawRay(rayOrigin, rayDir * rayHit.distance, Color.green);
        }
        else
        {
            isGrounded = false;
            Debug.DrawRay(rayOrigin, rayDir * rayDistance, Color.red);
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        Vector3 targetVelocity = move * moveSpeed;
        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 velocityDifference = targetVelocity - currentVelocity;
        
        if (moveX != 0 || moveZ != 0)
        {
            rb.linearDamping = 0f;
            rb.AddForce(velocityDifference * rb.mass);
        }
        else if(isGrounded)
        {
            rb.linearDamping = 5f;
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    
}