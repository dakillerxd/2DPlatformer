using CustomAttribute;
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;



public enum GameStates {
    None,
    GamePlay,
    Paused,
    GameOver
}

public enum GameDifficulty {
    None,
    Easy,
    Normal,
    Hard,
}

public enum CosmeticItems
{
    GooglyEye,
    PropellerHat,
    CurlyMustache
}

public enum PlayerState {
    Controllable,
    Frozen,
}

public enum PlayerAbilities {
    DoubleJump,
    WallSlide,
    WallJump,
    Dash,
}


[System.Serializable]
public class Sound
{
    public string name;
    [Range(0f, 2f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(-1f, 1f)] public float stereoPan = 0f;
    [Range(0f, 1f)] public float spatialBlend = 0f;
    [Range(0f, 1.1f)] public float reverbZoneMix = 1f;
    public bool loop = false;
    public AudioClip[] clips;
    public AudioClip[] funnyModeClips;
    [HideInInspector] public AudioSource source;
    
}

[System.Serializable]
public class Unlock
{
    public string unlockName;
    public bool unlockState;
    public int unlockedAtCollectible;
    [FormerlySerializedAs("received")] [CustomAttribute.ReadOnly] public bool unlockReceived;
}

[System.Serializable]
public class Collectible
{
    public SceneField connectedLevel;
    public bool countsTowardsUnlocks = true;
    [CustomAttribute.ReadOnly] public bool collected;
}

[System.Serializable]
public class Level
{
    public string levelName;
    public SceneField scene;
    public bool gameLevel = true;
    [VInspector.ShowIf("gameLevel")] public int levelNumber ;
}


