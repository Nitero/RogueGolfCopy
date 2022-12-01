using Controllers;
using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class BallController : MonoBehaviour
{
    private GameController _game;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Transform _visuals;
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private ParticleSystem _readyToShootVFX;
    [Space]
    [SerializeField] private float _fallVelocityDamp = 0.98f;
    [SerializeField] private float _goalSuckStrength = 0.5f;
    [SerializeField] private float _goalVelocityDamp = 1f;
    [Space]
    [SerializeField] private float _sandVelocityDamp = 0.99f;
    [SerializeField] private float _boostVelocity = 1f;
    [Space]
    [SerializeField] private AnimationCurve _velocityToDragCurve;
    [SerializeField] private float _velocityToStopAt = 0.01f;
    [Space]
    [SerializeField] private float _fallSpeed = 1f;
    [SerializeField] private float _fakeHeightToScale = 1f;
    [Space]
    [SerializeField] private int _preventEndlessBoostSameWallHitMax = 10;

    public Rigidbody2D RigidBody => _rb;
    
    private BallConfig.BallType _ballType;
    
    private bool _justSpawned;
    private float _fakeHeight;
    private bool _canBeShot;
    public bool CanBeShot => _canBeShot;
    private Sequence _bounceTweenSequence;
    private bool _didHitGroundLast;
    
    private int _wentThroughWalls;
    private int _samePositionWallHitCounter;
    private Vector2 _lastWallHitPosition;

    public BallController Initialize(GameController game, BallConfig.BallType ballType)
    {
        _game = game;
        _ballType = ballType;
        ResetValues();
        return this;
    }

    private void ResetValues()
    {
        _fakeHeight = 0;
        _canBeShot = true;
        _didHitGroundLast = true;
        _wentThroughWalls = 0;
        _samePositionWallHitCounter = 0;
    }

    private void Update()
    {
        
    }
    
    
    private void FixedUpdate()
    {
        float fakeHeightLast = _fakeHeight;
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, 0.01f, LayerMask.GetMask("Ground"));
        RaycastHit2D hitGoal = Physics2D.Raycast(transform.position, Vector2.down, 0.01f, LayerMask.GetMask("Goal"));
        if (hitGround.collider == null || hitGoal.collider != null)
        {
            if (hitGround.collider == null && _ballType.DoesntFallUntilStops && _rb.velocity.magnitude > 0)
                _fakeHeight = 0;
            else
            {
                _fakeHeight -= _fallSpeed * Time.deltaTime;
                _rb.velocity *= _fallVelocityDamp;
            }
            
            if (hitGoal.collider != null)
            {
                Vector2 goalDirection = hitGround.transform.position - transform.position;
                // _rb.velocity = (goalDirection.normalized * _goalSuckStrength + _rb.velocity.normalized * (1-_goalSuckStrength)) * _rb.velocity.magnitude;
                //should keep velocity but only apply change to left/right? not backwards? bcz now too easy
                _rb.velocity *= _goalVelocityDamp;
                _rb.AddForce(goalDirection.normalized * _goalSuckStrength, ForceMode2D.Impulse);
            }
        }
        else
            // _fakeHeight += _riseSpeed * Time.deltaTime;
            _fakeHeight = 0;
        
        
        _rb.drag = _velocityToDragCurve.Evaluate(_rb.velocity.magnitude);
        if (_rb.velocity.magnitude <= _velocityToStopAt)
            _rb.velocity = Vector2.zero;

        if (hitGround.collider != null && !_canBeShot)
        {
            if (hitGround.transform.CompareTag("Sand"))
                _rb.velocity *= _sandVelocityDamp;//TODO: multiply with delta time?
            if (hitGround.transform.CompareTag("Boost0"))
                _rb.AddForce(Vector2.up * _boostVelocity);
            if (hitGround.transform.CompareTag("Boost1"))
                _rb.AddForce(Vector2.right * _boostVelocity);
            if (hitGround.transform.CompareTag("Boost2"))
                _rb.AddForce(Vector2.down * _boostVelocity);
            if (hitGround.transform.CompareTag("Boost3"))
                _rb.AddForce(Vector2.left * _boostVelocity);
        }
        
        if (_fakeHeight == 0 && fakeHeightLast < 0)//fakeHeightLast < _fakeHeight && !_didHitGroundLast)
        {
            _bounceTweenSequence?.Kill(true);
            _bounceTweenSequence = DOTween.Sequence();
            _bounceTweenSequence.Append(_visuals.DOScale(Vector3.one * 1.5f, 0.125f).SetEase(Ease.OutQuad));
            _bounceTweenSequence.Append(_visuals.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBounce));
            //TODO: bigger & longer the lower had fallen
        }

        if (_fakeHeight < -0.125f && fakeHeightLast > -0.125f)
            AudioManager.Instance.Play(AudioManager.Sound.BallFall);

        // _fakeHeight = Mathf.Clamp(_fakeHeight, -1, 0);

        transform.localScale = Vector3.one * _ballType.SizeModifier * (1+_fakeHeight) * _fakeHeightToScale;
        _trail.widthMultiplier = transform.localScale.x;

        if (_fakeHeight <= -1f)
        {
            if (hitGoal.collider != null)
                _game.ScoredGoal();
            else
                _game.FellOff();
            return;
        }

        if (!_canBeShot && _fakeHeight == 0 && _rb.velocity.magnitude == 0 && _didHitGroundLast)
            SetCanShoot();
        
        _didHitGroundLast = hitGround.collider != null;

        //TODO: move non physics stuff back to update
        //TODO: add spinning sprite
        //TODO: add squash based on velocity and hits to _visuals
    }

    public void Shoot()
    {
        _canBeShot = false;
    }
    private void SetCanShoot()
    {
        _canBeShot = true;
        _readyToShootVFX.Play();
        _game.StoppedBall();
    }

    public void SetBallType(BallConfig.BallType ballType)
    {
        _ballType = ballType;
        _collider.isTrigger = ballType.GoThroughXWalls > 0;
        gameObject.layer = LayerMask.NameToLayer(ballType.CantFallOffEdge ? "InvisWallBall" : "Default");
        ResetValues();
    }
    public float GetSpeed()
    {
        return _ballType.SpeedModifier;
    }
    public float GetSize()
    {
        return _ballType.SizeModifier;
    }
    public bool GoesThroughWalls()
    {
        return _ballType.GoThroughXWalls > 0;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("RewardBall") &&
            !other.gameObject.CompareTag("RewardClub") &&
            !other.gameObject.CompareTag("RewardMystery"))
            return;
        _game.AddReward(other.gameObject.tag);
        Destroy(other.gameObject);
        AudioManager.Instance.Play(AudioManager.Sound.BallCollectReward);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Wall"))
            return;
        
        _wentThroughWalls++;
        if (_wentThroughWalls >= _ballType.GoThroughXWalls)
            _collider.isTrigger = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        AudioManager.Instance.Play(AudioManager.Sound.BallCollide);
        if (Vector2.Distance(collision.contacts[0].point, _lastWallHitPosition) <= 0.1f)
        {
            _samePositionWallHitCounter++;
            if (_samePositionWallHitCounter > _preventEndlessBoostSameWallHitMax)
            {
                _rb.velocity = Vector2.zero;
                SetCanShoot();
            }
        }
        _lastWallHitPosition = collision.contacts[0].point; 
            
        if (_ballType.BreakWallAfterXHits == 0)
            return;
        
        if (_game.HitWalls.ContainsKey(collision.collider))
            _game.HitWalls[collision.collider]++;
        else
            _game.HitWalls.Add(collision.collider, 1);

        if (_game.HitWalls[collision.collider] >= _ballType.BreakWallAfterXHits)
            Destroy(collision.gameObject);
    }
}
