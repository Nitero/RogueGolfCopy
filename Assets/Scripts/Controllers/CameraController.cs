using System;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _boardCamera;
    [SerializeField] private CinemachineVirtualCamera _ballGoalCamera;
    [SerializeField] private CinemachineCameraOffset _ballGoalCameraOffset;
    [SerializeField] private CinemachineRecomposer _ballGoalCameraZoom;
    [SerializeField] private CinemachineTargetGroup _boardTargetGroup;
    [SerializeField] private CinemachineTargetGroup _ballGoalTargetGroup;
    [SerializeField] private Transform _boardTileParent;
    [SerializeField] private float _tileRadius = 4f;
    [SerializeField] private float _ballGoalRadius = 1f;
    [SerializeField] private float _zoomSpeedInBallCamera;
    [SerializeField] private float _zoomLerpSpeed = 0.9f;
    [SerializeField] private Vector2 _zoomMinMax = new Vector2(0.333f, 0.666f);
    [SerializeField] private float _dragDistMultiplier = 1;
    [SerializeField] private float _dragLerpSpeed = 1;
    [SerializeField] private float _dragMaxDist = 10;
    
    private Transform _lastAddedBoardTile;
    private Transform _lastAddedBall;

    private Vector2 _dragStart;
    private Vector2 _desiredOffset;
    private float _desiredZoom = 1;
    
    private void Awake()
    {
        _ballGoalCameraZoom.m_ZoomScale = _desiredZoom;
        FocusBall();
    }

    public void AddBoardTile(Transform boardTile)
    {
        _boardTargetGroup.AddMember(boardTile, 1, _tileRadius);
        if(_lastAddedBoardTile)
            _ballGoalTargetGroup.RemoveMember(_lastAddedBoardTile);
        _ballGoalTargetGroup.AddMember(boardTile, 1, _ballGoalRadius);
        _lastAddedBoardTile = boardTile;
    }
    public void AddBall(Transform ball)
    {
        if(_lastAddedBall)
            _ballGoalTargetGroup.RemoveMember(_lastAddedBall);
        _ballGoalTargetGroup.AddMember(ball, 1, _ballGoalRadius);
    }

    private void Update()
    {
        if (_ballGoalCamera.enabled)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                _desiredZoom -= scrollInput * Time.deltaTime * _zoomSpeedInBallCamera;
                _desiredZoom = Mathf.Clamp(_desiredZoom, _zoomMinMax.x, _zoomMinMax.y);
            }
            _ballGoalCameraZoom.m_ZoomScale = Mathf.Lerp(_desiredZoom, _ballGoalCameraZoom.m_ZoomScale, _zoomLerpSpeed);
            
            
            if (Input.GetMouseButtonDown(1))
                _dragStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // _dragStart = Input.mousePosition;
                //TODO: fix world space wiggles with lerp bcz world space pos changes
            if (Input.GetMouseButton(1))
            {
                Vector2 currentDrag = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // Vector2 currentDrag = Input.mousePosition;
                if (Vector2.Distance(_dragStart, currentDrag) > Mathf.Epsilon)
                {
                    _desiredOffset += _dragStart - currentDrag;
                    // _desiredOffset += (_dragStart - currentDrag) * Screen.width * _dragDistMultiplier;
                    _desiredOffset = Vector2.ClampMagnitude(_desiredOffset, _dragMaxDist);
                }
                //TODO: should get harder at the edge? and reset after x seconds?
            }
            _ballGoalCameraOffset.m_Offset = Vector3.Lerp(_desiredOffset, _ballGoalCameraOffset.m_Offset, _dragLerpSpeed);
        }
    }

    public void FocusBall()
    {
        _desiredZoom = 1;
        _desiredOffset = Vector2.zero;
        
        _ballGoalCamera.enabled = true;
        _boardCamera.enabled = false;
    }
    public void FocusBoard()
    {
        _ballGoalCamera.enabled = false;
        _boardCamera.enabled = true;
    }
}