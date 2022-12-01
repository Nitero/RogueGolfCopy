using System.Collections;
using System.Collections.Generic;
using Controllers;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShootInput : MonoBehaviour
{
    [SerializeField] private ShootConfig _config;
    [SerializeField] private GameController _game;
    [SerializeField] private RoundController _rounds;
    [SerializeField] private LineRenderer _trajectoryLine;
    [SerializeField] private ShootInputUI _shootUI;

    private float _dragDistToShotStrength;
    private Vector2 _aimDir;
    private bool _canRelease;

    private bool _wasTouchingLastFixed;
    private bool _isTouching;
    private Vector2 _touchA;
    private Vector2 _touchB;

    private Vector2 _joystickOuterStart;
    private Vector2 _joystickInnerStart;
    private List<Vector3> _trajectory = new List<Vector3>();
    private List<Vector3> _trajectorySubdivided = new List<Vector3>();

    public void Initialize()
    {
        _isTouching = false;
        _wasTouchingLastFixed = false;
        _canRelease = false;
    }
    
    private void Update()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                var mousePos = Input.mousePosition;
                _touchA = mousePos;

                _isTouching = true;
                if(_game.CanShootBall())
                    AudioManager.Instance.Play(AudioManager.Sound.StartUIDrag);
            }
            if (Input.GetMouseButton(0) && _isTouching)
            {
                _isTouching = true;
                var mousePos = Input.mousePosition;
                _touchB = mousePos;
            }
            else
                _isTouching = false;
            if (Input.GetMouseButtonUp(0) && _game.CanShootBall())
                AudioManager.Instance.Play(AudioManager.Sound.EndUIDrag);
        }
        else if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began && !IsPointerOverUIObject())
            {
                _touchA = t.position;
                _isTouching = true;
            }
            if (t.phase == TouchPhase.Moved && _isTouching)
            {
                _isTouching = true;
                _touchB = t.position;
            }
            if (t.phase == TouchPhase.Ended)
                _isTouching = false;
        }
        
        if (_game.CanShootBall() && _isTouching)
        {
            float dragDist = Vector2.Distance(_touchB, _touchA);
            dragDist = Mathf.Clamp(dragDist, _config.DragDistMinMax.x, _config.DragDistMinMax.y);
            _dragDistToShotStrength = dragDist.Remap(_config.DragDistMinMax.x, _config.DragDistMinMax.y, _config.ShootStrMinMax.x, _config.ShootStrMinMax.y + _rounds.CombinedClub.AddedShootStrength);

            if(dragDist >= _config.DragDistOutBeforeCanRelease)
                _canRelease = true;

            // Could cancel now by lifting finger
            if (_canRelease && dragDist <= _config.DragCancelDist)
            {
                _shootUI.ShowCross();
                HideTrajectory();
            }
            else
            {
                _shootUI.HideCross();
                DrawTrajectory(-_aimDir);
            }
            
            _shootUI.ShowDragIndication(_touchA, _touchB);
        }
        else
            HideTrajectory();
    }
    
    private void FixedUpdate()
    {
        if (!_game.CanShootBall())
            return;
        
        if (_isTouching)
        {
            var dragOffset = _touchB - _touchA;
            _aimDir = dragOffset.normalized;
            _wasTouchingLastFixed = true;
        }
        else
        {
            // released after drag
            if (_wasTouchingLastFixed)
            {
                float dragDist = Vector2.Distance(_touchB, _touchA);
                dragDist = Mathf.Clamp(dragDist, _config.DragDistMinMax.x, _config.DragDistMinMax.y);

                // didn't drag far enough, reset shot
                if (dragDist <= _config.DragCancelDist)
                    _shootUI.HideDragIndication(false);
                else
                {
                    _wasTouchingLastFixed = false;
                    _shootUI.HideDragIndication(true);
                    StartCoroutine(ShootBallAfterDelay());
                }

                _canRelease = false;
            }
        }
    }

    private IEnumerator ShootBallAfterDelay()
    {
        yield return new WaitForSeconds(_config.DragIndicationBallHitTime);
        
        _game.CurrentBall.RigidBody.velocity = _game.CurrentBall.RigidBody.velocity * _config.KeptVelocityOnShot;
        _game.CurrentBall.RigidBody.AddForce(-_aimDir * _dragDistToShotStrength * _game.CurrentBall.GetSpeed(), ForceMode2D.Impulse);
        _game.ShotBall();
    }
    
    private void HideTrajectory()
    {
        _trajectoryLine.SetVertexCount(0);
    }
    
    private void DrawTrajectory(Vector2 aimDirection)
    {
        _trajectory = new List<Vector3>(_trajectory.Capacity);
        
        float distance = (_config.TrajectoryDistance + _rounds.CombinedClub.AddedTrajectoryDistance) * _dragDistToShotStrength * _game.CurrentBall.GetSpeed();
        Vector2 pos = _game.CurrentBall.transform.position;
        _trajectory.Add(pos);
        Vector2 dir = aimDirection.normalized;
        for (var i = 0; i < _config.TrajectoryBounces+_rounds.CombinedClub.ShowXTrajectoryBounces+1; i++)
        {
            pos += dir * 0.01f;
            RaycastHit2D hit = Physics2D.CircleCast(pos, 0.5f*_game.CurrentBall.GetSize(), dir, distance, LayerMask.GetMask("Wall")); // ~(LayerMask.GetMask("Player")) everything but player

            if (_game.CurrentBall.GoesThroughWalls())
                hit.distance = distance;
            
            float traveledDistance = hit.collider == null ? distance : hit.distance;
            distance -= traveledDistance;
            
            pos += dir * traveledDistance;
            _trajectory.Add(pos);
            
            if (hit.collider != null && !_game.CurrentBall.GoesThroughWalls())
                dir = Vector2.Reflect(dir, hit.normal);
            
            if (distance <= 0)
                break;
        }

        _trajectorySubdivided = new List<Vector3>(_trajectorySubdivided.Capacity);
        for (var i = 0; i < _trajectory.Count - 1; i++)
        {
            Vector3 position = _trajectory[i];
            Vector3 direction = _trajectory[i+1] - _trajectory[i];
            float steps = Vector3.Magnitude(direction) * _config.TrajectorySubdivisionsByDistance;
            direction /= steps;
            for (int j = 0; j < steps; j++)
                _trajectorySubdivided.Add(position + direction * j);
        }

        //TODO: fix dotted material -> ensure each line has the same amount of verts?
        //https://answers.unity.com/questions/733592/dotted-line-with-line-renderer.html https://gamedev.stackexchange.com/questions/118814/unity-make-dotted-line-renderer
        // _line.materials[0].mainTextureScale = new Vector3(_trajectoryDistance, 1, 1);
        _trajectoryLine.SetVertexCount(_trajectorySubdivided.Count);
        for (int i = 0; i < _trajectorySubdivided.Count; i++)
            _trajectoryLine.SetPosition(i, _trajectorySubdivided[i]);
        _trajectoryLine.widthMultiplier = _game.CurrentBall.transform.localScale.x;
    }
    
    private bool IsPointerOverUIObject() //credit: https://answers.unity.com/questions/1073979/android-touches-pass-through-ui-elements.html
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}