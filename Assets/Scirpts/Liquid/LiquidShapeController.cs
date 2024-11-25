using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class LiquidShapeController : MonoBehaviour
{
    [SerializeField] private float springStiffness = 0.1f;
    [SerializeField] private float springDamping = 0.03f;
    [SerializeField] private float spread = 0.006f;
    [SerializeField] private List<LiquidSpring> springs = new();

    private int cornersCount = 2;
    [SerializeField] private LiquidShapeController liquidShapeController;
    [SerializeField] private int wavesCount = 6;
    


    private void FixedUpdate()
    {
        foreach (LiquidSpring spring in springs)
        {
            spring.WaveSpringUpdate(springStiffness, springDamping);
        }
        
        UpdateSprings();
    }


    private void UpdateSprings() {
        
        int count = springs.Count;
        float[] leftDeltas = new float[count];
        float[] rightDeltas = new float[count];
        
        for (int i = 0; i < count; i++)
        {
            if (i > 0)
            {
                leftDeltas[i] = spread * (springs[i].height - springs[i - 1].height);
                springs[i - 1].velocity += leftDeltas[i];
            }
            if (i < count - 1)
            {
                rightDeltas[i] = spread * (springs[i].height - springs[i + 1].height);
                springs[i + 1].velocity += rightDeltas[i];
            }
        }
    }
    


    private void Splash(int index, float speed) {
        
        if (index >= 0 && index < springs.Count)
        {
            springs[index].velocity += speed;
        }
    }

    private void Wave()
    {
        Spline liquidSpline = liquidShapeController.GetComponent<SpriteShapeController>().spline;
        int liquidPointsCount = liquidSpline.GetPointCount();
    }
}
