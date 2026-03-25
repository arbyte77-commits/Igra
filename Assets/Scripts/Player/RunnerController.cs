using System.Collections;
using UnityEngine;
using BeachRunner.Core;
using BeachRunner.World;
using BeachRunner.Audio;

namespace BeachRunner.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
    public class RunnerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float laneOffsetY = 1.8f;
        [SerializeField] private float laneShiftSpeed = 14f;

        [Header("Sliding")]
        [SerializeField] private float slideDuration = 0.65f;
        [SerializeField] private Vector2 slideColliderSize = new(0.7f, 0.6f);
        [SerializeField] private Vector2 defaultColliderSize = new(0.7f, 1.2f);

        [Header("FX")]
        [SerializeField] private ParticleSystem jumpDust;
        [SerializeField] private ParticleSystem hitFx;

        private Rigidbody2D _rb;
        private Collider2D _col;
        private Animator _animator;
        private int _lane = 1; // 0 low, 1 mid, 2 high
        private bool _grounded = true;
        private bool _sliding;
        private bool _shielded;

        private readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private readonly int IsSlidingHash = Animator.StringToHash("IsSliding");
        private readonly int HitHash = Animator.StringToHash("Hit");

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (GameManager.Instance.State != GameState.Playing) return;
            RunnerSpeedRuntime.Tick();
            HandleVerticalLane();
        }

        public void Jump()
        {
            if (!_grounded || _sliding || GameManager.Instance.State != GameState.Playing) return;

            _rb.velocity = new Vector2(_rb.velocity.x, 0f);
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _grounded = false;
            _animator.SetBool(IsGroundedHash, false);
            if (jumpDust != null) jumpDust.Play();
            AudioBus.Instance.PlayJump();
        }

        public void Slide()
        {
            if (!_grounded || _sliding || GameManager.Instance.State != GameState.Playing) return;
            StartCoroutine(SlideRoutine());
        }

        public void ShiftLane(int direction)
        {
            if (GameManager.Instance.State != GameState.Playing) return;
            _lane = Mathf.Clamp(_lane + direction, 0, 2);
        }

        private void HandleVerticalLane()
        {
            var targetY = (_lane - 1) * laneOffsetY;
            var current = transform.position;
            transform.position = Vector3.Lerp(current, new Vector3(current.x, targetY, 0f), Time.deltaTime * laneShiftSpeed);
        }

        private IEnumerator SlideRoutine()
        {
            _sliding = true;
            _animator.SetBool(IsSlidingHash, true);
            _col.offset = new Vector2(_col.offset.x, -0.25f);
            if (_col is CapsuleCollider2D capsule) capsule.size = slideColliderSize;

            yield return new WaitForSeconds(slideDuration);

            _sliding = false;
            _animator.SetBool(IsSlidingHash, false);
            _col.offset = new Vector2(_col.offset.x, 0f);
            if (_col is CapsuleCollider2D cap) cap.size = defaultColliderSize;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.CompareTag("Ground"))
            {
                _grounded = true;
                _animator.SetBool(IsGroundedHash, true);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Pickup pickup))
            {
                pickup.Collect(this);
                return;
            }

            if (!other.CompareTag("Obstacle")) return;

            if (_shielded)
            {
                _shielded = false;
                Destroy(other.gameObject);
                return;
            }

            _animator.SetTrigger(HitHash);
            if (hitFx != null) hitFx.Play();
            AudioBus.Instance.PlayHit();
            GameManager.Instance.GameOver();
            CameraShake.Instance.Shake(0.15f, 0.28f);
        }

        public void ApplyShield(float duration)
        {
            StartCoroutine(ShieldRoutine(duration));
        }

        private IEnumerator ShieldRoutine(float duration)
        {
            _shielded = true;
            yield return new WaitForSeconds(duration);
            _shielded = false;
        }
    }
}
