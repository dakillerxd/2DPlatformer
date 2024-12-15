
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;

public class MovingWall : MonoBehaviour
{
    
    [Header("Settings")]
   [SerializeField] private float speed = 1f;
   [SerializeField] private Transform selectedPosition;
   [SerializeField] private bool moveOnStart;
   
   [Header("Camera Shake")]
   [SerializeField] private bool shakeCamera;
   [EnableIf(nameof(shakeCamera))]
   [SerializeField] private float magnitude = 5f; 
   
   
   [Header("SFX")]
   [SerializeField] private AudioSource audioSource;
   [SerializeField] private bool playSfx;

   [EnableIf(nameof(playSfx))] 
   [SerializeField] private float fadeInTime = 0.5f;
   [SerializeField] private float fadeOutTime = 1.5f;
   [EndIf]
   
   private bool _move;

   
    private void Start()
    {
        if (selectedPosition == null)
        {
            selectedPosition = transform;
        }
        
        if (moveOnStart)
        {
            SetMoving(true);
        }
    }
    
    private void Update()
    {
        MoveToSelectedPosition(selectedPosition);
    }
    
    
    private void MoveToSelectedPosition(Transform position)
    {
        if (!_move) return;
        
        transform.position = Vector3.MoveTowards(transform.position, position.position, speed * Time.deltaTime);
        
        if (shakeCamera) CameraController.Instance?.ShakeCamera(0.3f, magnitude, 2, 2);
        
        if (transform.position == position.position)
        {
            SetMoving(false);
        }
    }
    
    public void SetMoving(bool move)
    {
        _move = move;
        
        if (!playSfx) return;
        if (move) { SoundManager.Instance?.FadeSoundIn("WallMoving", audioSource:audioSource ,fadeTime: fadeInTime); }
        else { SoundManager.Instance?.FadeSoundOut("WallMoving", audioSource:audioSource ,fadeTime: fadeOutTime); }
        
    }
}
