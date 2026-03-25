using UnityEngine;

namespace BeachRunner.Audio
{
    public class AudioBus : MonoBehaviour
    {
        public static AudioBus Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Clips")]
        [SerializeField] private AudioClip beachMusic;
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private AudioClip coinClip;
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip buttonClip;
        [SerializeField] private AudioClip gameOverClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (beachMusic != null)
            {
                musicSource.clip = beachMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        public void PlayJump() => Play(jumpClip, 0.85f);
        public void PlayCoin() => Play(coinClip, 1f);
        public void PlayHit() => Play(hitClip, 1f);
        public void PlayButton() => Play(buttonClip, 0.9f);
        public void PlayGameOver() => Play(gameOverClip, 1f);

        private void Play(AudioClip clip, float volume)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}
