using UnityEngine;

[CreateAssetMenu(fileName = "ShootConfig", menuName = "Configs/ShootConfig", order = 1)]
public class ShootConfig : ScriptableObject
{
    [Header("Input")]
    public float KeptVelocityOnShot = 0.75f; // 0 means complete reset, 1 means keep old vel and add new
    public Vector2 ShootStrMinMax = new Vector2(0, 30);
    [Space]
    public Vector2 _dragDistMinMax = new Vector2(0, 1); //above doesn't do more strength, below and it can be released to abort
    public Vector2 DragDistMinMax => _dragDistMinMax * Screen.width;

    public float DragDistOutBeforeCanRelease = 0f; 
    public float DragCancelDist = 0.125f;
    
    [Header("Trajectory")]
    public float TrajectoryDistance = 0.5f;
    public int TrajectoryBounces = 1;
    public int TrajectorySubdivisionsByDistance = 32;
    
    [Header("Drag UI")]
    public Vector2 DragLineMinMaxWidth = new Vector2(0.25f, 0.75f);
    public float DragIndicationSnapDur = 0.25f;
    public float DragIndicationBallHitTime = 0.1f;
    public AnimationCurve DragIndicationSnap;
}