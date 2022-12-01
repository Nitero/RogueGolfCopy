using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance { get { return _instance; } }
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        [SerializeField] private AudioSource _music;
        [SerializeField] private AudioSource _sfx;
        [Space]
        [SerializeField] private AudioClip _menuMusic;
        [SerializeField] private AudioClip _gameplayMusic;
        [Space]
        [SerializeField] private AudioClip _clickUIButton;
        [SerializeField] private AudioClip _startUIDrag;
        [SerializeField] private AudioClip _endUIDrag;
        [Space]
        [SerializeField] private AudioClip _ballShoot;
        [SerializeField] private AudioClip _ballCollide;
        [SerializeField] private AudioClip _ballReady;
        [SerializeField] private AudioClip _ballScore;
        [SerializeField] private AudioClip _ballFall;
        [SerializeField] private AudioClip _ballRespawn;
        [SerializeField] private AudioClip _ballCollectReward;
        [SerializeField] private AudioClip _ballSelect;
        [SerializeField] private AudioClip _gameOver;
        [SerializeField] private AudioClip _tilePlaced;

        private Dictionary<Sound, AudioClip> _sounds;

        public enum Sound
        {
            None,
            
            MenuMusic,
            GameplayMusic,
            
            ClickUIButton,
            StartUIDrag,
            EndUIDrag,
            
            BallShoot,
            BallCollide,
            BallReady,
            BallScore,
            BallFall,
            BallRespawn,
            BallCollectReward,
            BallSelect,
            GameOver,
            TilePlaced,
        }

        private void Start()
        {
            _sounds = new Dictionary<Sound, AudioClip>()
            {
                {Sound.MenuMusic, _menuMusic},
                {Sound.GameplayMusic, _gameplayMusic},
                {Sound.ClickUIButton, _clickUIButton},
                {Sound.StartUIDrag, _startUIDrag},
                {Sound.EndUIDrag, _endUIDrag},
                {Sound.BallShoot, _ballShoot},
                {Sound.BallCollide, _ballCollide},
                {Sound.BallReady, _ballReady},
                {Sound.BallScore, _ballScore},
                {Sound.BallFall, _ballFall},
                {Sound.BallRespawn, _ballRespawn},
                {Sound.BallCollectReward, _ballCollectReward},
                {Sound.BallSelect, _ballSelect},
                {Sound.GameOver, _gameOver},
                {Sound.TilePlaced, _tilePlaced},
            };
        }
        
        public void Play(Sound sound)
        {
            if(_sounds.ContainsKey(sound))
                _sfx.PlayOneShot(_sounds[sound], 1f);
            else
                Debug.LogWarning("No sound for " + sound);
        }
        
        public void PlayMusic(Sound sound)
        {
            _music.clip = _sounds[sound];
            _music.Play();
        }
    }
}