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
    
    [Header("Car Interaction")]
    public float interactionRange = 1f;
    public CarManager carManager;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    void Update()
    {
        HandleMouseLook();
        HandleJump();
        HandleCarInteraction();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 newPos = rb.position + move * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPos);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    
    void HandleCarInteraction()
    {
        
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, interactionRange);
        
        CarController nearestCar = null;
        float nearestDistance = float.MaxValue;
        
        foreach (Collider col in nearbyObjects)
        {
            CarController car = col.GetComponent<CarController>();
            if (car != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCar = car;
                }
            }
        }
        
        if (nearestCar != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                carManager.EnterCar(this, nearestCar);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }
    
    // Gizmo to show interaction range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}