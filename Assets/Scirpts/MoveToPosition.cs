
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;

public class MoveToPosition : MonoBehaviour
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
   [SerializeField] private bool playSfx;
   [EnableIf(nameof(playSfx))] 
   [SerializeField] private string sfx;
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
        
        if (shakeCamera) CameraController.Instance?.ShakeCamera(0.1f, magnitude, 3, 3);
        
        if (transform.position == position.position)
        {
            SetMoving(false);
        }
    }
    
    public void SetMoving(bool move)
    {
        _move = move;
        
        if (!playSfx) return;
        if (move) { SoundManager.Instance?.FadeSoundIn(sfx, fadeTime: 0.5f); }
        else { SoundManager.Instance?.FadeSoundOut(sfx, fadeTime: 2f); }
        
    }
}
