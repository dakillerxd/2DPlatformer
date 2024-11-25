using UnityEngine;

public class LiquidSpring : MonoBehaviour
{

    public  float velocity = 0;
    public  float force = 0;
    public  float height = 0;
    public  float targetHeight = 0;
    
    

    public void WaveSpringUpdate(float springStiffness, float dampening)
    {
        height = transform.position.y;
        var x = height - targetHeight;
        var loss = -dampening * velocity;
        
        force = - springStiffness * x + loss;
        velocity += force;
        var y = transform.localPosition.y;
        transform.localPosition = new Vector3(transform.localPosition.x, y + velocity, transform.localPosition.z);
    }
    
    
}
