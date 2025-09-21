using UnityEngine;

public class Wander : MonoBehaviour
{
    
    public float radius = 5f;
    public float speed = 1f;

    private float angle = 0f;
    private Vector3 centerPoint;
    
    private float baseY;
    private float periodSpeed = 15f;
    private float periodHeight = 0.4f;
    private float periodOffset;

    void Start()
    {
        centerPoint = transform.position;
        baseY = transform.position.y;
            
        if (Random.value > 0.5f) speed = -speed;
        
        periodOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        angle += speed * Time.deltaTime;
        
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        float y = baseY + Mathf.Sin(Time.time * periodSpeed + periodOffset) * periodHeight;
        
        transform.position = new Vector3(centerPoint.x + x, y, centerPoint.z + z);
    }
}