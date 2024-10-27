using UnityEngine;

[RequireComponent(typeof(Animation))]
public class PlayAnimations : MonoBehaviour
{
    [Header("Settings:")]
    [SerializeField] private bool isLooping = true;
    [SerializeField] private bool playOnAwake = true;

    private Animation _animation;

    private void Reset()
    {
        GetAnimationComponentIfNeeded();
    }

    private void Awake()
    {
        Init();
        if (playOnAwake)
        {
            PlayAnimation();
        }
    }

    private void Init()
    {
        GetAnimationComponentIfNeeded();
        var clip = _animation.clip;
        if (clip == null)
        {
            return;
        }

        clip.legacy = true;
        clip.wrapMode = isLooping ? WrapMode.Loop : WrapMode.Default;
    }

    private void GetAnimationComponentIfNeeded()
    {
        _animation ??= GetComponent<Animation>();
        _animation.playAutomatically = false;
    }



  
    public void PlayAnimation()
    {
        if (_animation.clip == null)
        {
            Debug.LogError("AnimationPlayer PlayAnimation Error: No clip available in the animation component");
            return;
        }

        _animation.Play();
    }

    public void PlayAnimationByName(string name)
    {
        if (_animation == null)
        {
            Debug.LogError($"AnimationPlayer PlayAnimationByName Error: No Animation component found");
            return;
        }

        if (!_animation.GetClip(name))
        {
            Debug.LogError($"AnimationPlayer PlayAnimationByName Error: Animation clip '{name}' not found");
            return;
        }

        _animation.Play(name);
    }
}