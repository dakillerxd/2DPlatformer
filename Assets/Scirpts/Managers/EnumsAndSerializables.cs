using CustomAttribute;
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
public class Unlock
{
    public string unlockName;
    public bool unlockState;
    public int unlockedAtCollectible;
    [CustomAttribute.ReadOnly] public bool received;
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


