using UnityEngine;

public class HoverRotateAndBreathEffect : MonoBehaviour
{
    [System.Serializable]
    public class AxisRotationSettings
    {
        public bool continuousRotation = false;
        public bool reverseDirection = false;
        public float oscillationAmount = 0f;
        public float oscillationSpeed = 0f;
        public float rotationSpeed = 0f;
    }

    [Header("Hover Settings")]
    [SerializeField] private bool enableHover = false;
    [SerializeField] private float hoverSpeed = 0f;
    [SerializeField] private float hoverAmount = 0f;
    [SerializeField] private Vector3 hoverDirection;

    [Header("Rotation Settings")]
    [SerializeField] private bool enableRotation = false;
    [SerializeField] private AxisRotationSettings xRotation = new AxisRotationSettings();
    [SerializeField] private AxisRotationSettings yRotation = new AxisRotationSettings();
    [SerializeField] private AxisRotationSettings zRotation = new AxisRotationSettings();

    [Header("Pulse Settings")]
    [SerializeField] private bool enablePulse = false;
    [SerializeField] private float pulseAmount = 0.2f;
    // [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseCooldown = 1f;
    [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool pulseX = true;
    [SerializeField] private bool pulseY = true;
    [SerializeField] private bool pulseZ = true;

    [Header("Breathing Settings")]
    [SerializeField] private bool enableBreathing = false;
    [SerializeField] private float breatheAmount = 0.2f;
    [SerializeField] private float breatheSpeed = 1f;
    [SerializeField] private AnimationCurve breatheCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool breatheX = true;
    [SerializeField] private bool breatheY = true;
    [SerializeField] private bool breatheZ = true;

    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Quaternion initialRotation;
    private float hoverTime;
    private Vector3 rotationTime;
    private float pulseTimer;
    private float breatheTime;
    private Vector3 continuousRotationAngles;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;
        hoverTime = Random.value * Mathf.PI * 2;
        breatheTime = Random.value * Mathf.PI * 2;
        
        rotationTime = new Vector3(
            Random.value * Mathf.PI * 2,
            Random.value * Mathf.PI * 2,
            Random.value * Mathf.PI * 2
        );

        pulseTimer = 0f;
        continuousRotationAngles = Vector3.zero;
    }

    private void FixedUpdate()
    {
        HoverEffect();
        RotationEffect();
        
        Vector3 pulseScale = CalculatePulseScale();
        Vector3 breatheScale = CalculateBreatheScale();
        
        transform.localScale = Vector3.Scale(initialScale, Vector3.Scale(pulseScale, breatheScale));
    }

    private void HoverEffect()
    {
        if (!enableHover) return;

        hoverTime += hoverSpeed * Time.fixedDeltaTime;

        float hoverOffset = Mathf.Sin(hoverTime) * hoverAmount;
        Vector3 normalizedHoverDir = hoverDirection.normalized;
        Vector3 newPosition = initialPosition + normalizedHoverDir * hoverOffset;
        transform.position = newPosition;
    }

    private void RotationEffect()
    {
        if (!enableRotation) return;

        Vector3 rotationChange = new Vector3(
            CalculateAxisRotation(ref rotationTime.x, xRotation),
            CalculateAxisRotation(ref rotationTime.y, yRotation),
            CalculateAxisRotation(ref rotationTime.z, zRotation)
        );

        if (xRotation.continuousRotation) continuousRotationAngles.x += rotationChange.x;
        if (yRotation.continuousRotation) continuousRotationAngles.y += rotationChange.y;
        if (zRotation.continuousRotation) continuousRotationAngles.z += rotationChange.z;

        Vector3 finalRotation = new Vector3(
            xRotation.continuousRotation ? continuousRotationAngles.x : rotationChange.x,
            yRotation.continuousRotation ? continuousRotationAngles.y : rotationChange.y,
            zRotation.continuousRotation ? continuousRotationAngles.z : rotationChange.z
        );

        transform.rotation = initialRotation * Quaternion.Euler(finalRotation);
    }

    private float CalculateAxisRotation(ref float rotationTime, AxisRotationSettings settings)
    {
        if (settings.continuousRotation)
        {
            float direction = settings.reverseDirection ? -1f : 1f;
            return settings.rotationSpeed * Time.fixedDeltaTime * direction;
        }
        else if (settings.oscillationAmount != 0)
        {
            rotationTime += settings.oscillationSpeed * Time.fixedDeltaTime;
            return Mathf.Sin(rotationTime) * settings.oscillationAmount;
        }
        return 0f;
    }

    private Vector3 CalculatePulseScale()
    {
        if (!enablePulse)
            return Vector3.one;

        pulseTimer += Time.fixedDeltaTime;
        
        if (pulseTimer >= pulseCooldown)
        {
            pulseTimer = 0f;
        }

        float progress = pulseTimer / pulseCooldown;
        float pulseValue = pulseCurve.Evaluate(progress) * pulseAmount;

        return new Vector3(
            pulseX ? 1f + pulseValue : 1f,
            pulseY ? 1f + pulseValue : 1f,
            pulseZ ? 1f + pulseValue : 1f
        );
    }

    private Vector3 CalculateBreatheScale()
    {
        if (!enableBreathing)
            return Vector3.one;

        breatheTime += breatheSpeed * Time.fixedDeltaTime;
        
        float normalizedBreatheCycle = (Mathf.Sin(breatheTime) + 1f) * 0.5f;
        float breatheValue = breatheCurve.Evaluate(normalizedBreatheCycle) * breatheAmount;

        return new Vector3(
            breatheX ? 1f + breatheValue : 1f,
            breatheY ? 1f + breatheValue : 1f,
            breatheZ ? 1f + breatheValue : 1f
        );
    }
}