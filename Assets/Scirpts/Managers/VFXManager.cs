using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine.Serialization;

[RequireComponent(typeof(Volume))]
public class VFXManager : MonoBehaviour
{
    
    public static VFXManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Volume globalVolume;
    private  MotionBlur _motionBlur;
    private ChromaticAberration _chromaticAberration;
    private LensDistortion _lensDistortion;
    

    private void Awake() {
        
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (!globalVolume) {globalVolume = GetComponent<Volume>();}

        globalVolume.profile.TryGet<MotionBlur>(out _motionBlur);
        globalVolume.profile.TryGet<ChromaticAberration>(out _chromaticAberration);
        globalVolume.profile.TryGet<LensDistortion>(out _lensDistortion);
    }


#region Particle Effects

    public void SpawnVfxEffect(ParticleSystem effect) {
        if (!effect) return;
        Instantiate(effect, transform.position, Quaternion.identity);
    }


#endregion Particle Effects



#region Global Volume

    public void ToggleMotionBlur(bool state, float intensity = 0)
    {
        _motionBlur.active = state;
        _motionBlur.intensity.value = intensity;
    }
    
    public void ToggleChromaticAberration(bool state, float intensity = 0)
    {
        _chromaticAberration.active = state;
        _chromaticAberration.intensity.value = intensity;
    }
    
    public void ToggleLensDistortion(bool state, float intensity = 0)
    {
        _lensDistortion.active = state;
        _lensDistortion.intensity.value = intensity;
    }
        

    public IEnumerator LerpChromaticAberration(bool lerpIn, float time)
    {
        float startIntensity = lerpIn ? 0 : 1;
        float endIntensity = lerpIn ? 1 : 0;
        float elapsedTime = 0;
        _chromaticAberration.active = true;

        while (elapsedTime < time)
        {
            _chromaticAberration.intensity.value = Mathf.Lerp(startIntensity, endIntensity, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _chromaticAberration.intensity.value = endIntensity;
        if (!lerpIn) {_chromaticAberration.active = false;}
    }
        
    public IEnumerator LerpLensDistortion(bool lerpIn, float time)
    {
        float startIntensity = lerpIn ? 0 : 1;
        float endIntensity = lerpIn ? 1 : 0;
        float elapsedTime = 0;
        _lensDistortion.active = true;

        while (elapsedTime < time)
        {
            _lensDistortion.intensity.value = Mathf.Lerp(startIntensity, endIntensity, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _lensDistortion.intensity.value = endIntensity;
        if (!lerpIn) {_lensDistortion.active = false;}
    }

#endregion Global Volume

    
}
