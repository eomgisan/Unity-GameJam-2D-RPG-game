using UnityEngine;
using Random = UnityEngine.Random;

namespace TarodevController
{
    /// <summary>
    /// This is a pretty filthy script. I was just arbitrarily adding to it as I went.
    /// You won't find any programming prowess here.
    /// This is a supplementary script to help with effects and animation. Basically a juice factory.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _anim;
        [SerializeField] private AudioSource _source;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private ParticleSystem _jumpParticles, _launchParticles;
        [SerializeField] private ParticleSystem _moveParticles, _landParticles;
        [SerializeField] private AudioClip[] _footsteps;
        [SerializeField] private float _maxTilt = .1f;
        [SerializeField] private float _tiltSpeed = 1;
        [SerializeField, Range(1f, 3f)] private float _maxIdleSpeed = 2;
        [SerializeField] private float _maxParticleFallSpeed = -40;

        private IPlayerController _player;
        private bool _playerGrounded;
        private bool _isDashing;
        private ParticleSystem.MinMaxGradient _currentGradient;
        private Vector2 _movement;

        private bool _isDeath = false;



        void Awake() => _player = GetComponentInParent<IPlayerController>();

        void Update()
        {

            if (_player == null) return;

            // Flip the sprite
            if (_player.Input.X != 0)
            {
                transform.localScale = new Vector3(_player.Input.X > 0 ? 1 : -1, 1, 1);
            }

            // Lean while running
            var targetRotVector = new Vector3(0, 0, Mathf.Lerp(-_maxTilt, _maxTilt, Mathf.InverseLerp(-1, 1, _player.Input.X)));
            _anim.transform.rotation = Quaternion.RotateTowards(_anim.transform.rotation, Quaternion.Euler(targetRotVector), _tiltSpeed * Time.deltaTime);

            // Speed up idle while running
            //_anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, Mathf.Abs(_player.Input.X)));



            // Splat
            if (_player.LandingThisFrame)
            {
                _anim.SetTrigger(GroundedKey);
                _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
            }



            ChangeAnim();


            // Detect ground color
            var groundHit = Physics2D.Raycast(transform.position, Vector3.down, 2, _groundMask);
            if (groundHit && groundHit.transform.TryGetComponent(out SpriteRenderer r))
            {
                _currentGradient = new ParticleSystem.MinMaxGradient(r.color * 0.9f, r.color * 1.2f);
                SetColor(_moveParticles);
            }

            _movement = _player.RawMovement; // Previous frame movement is more valuable
        }


        // 함수 추가
        private void ChangeAnim()
        {
            if (!_isDeath)
            {
                //Run
                if (Mathf.Abs(_player.Input.X) > Mathf.Epsilon && !_isDashing)
                {
                    // Reset timer
                    _delayToIdle = 0.05f;
                    _anim.SetInteger("AnimState", 1);
                }


                if (_player.Death)
                {
                    _anim.SetBool(DeathKey, true);
                    _isDeath = true;
                }


                // Hurt TODO : PlayerContorller 에서 Dash 하면 이동시키는거 추가하기
                else if (_player.Damaged)
                {
                    _anim.SetTrigger(HurtKey);

                }

                // Attack 
                else if (_player.Input.Attack && !_isDashing && _playerGrounded)
                {
                    _anim.SetTrigger((AttackKey));


                }

                // Dash
                else if (_player.Input.Dash)
                {
                    _anim.SetTrigger(DashKey);

                }

                // Jump effects
                else if (_player.JumpingThisFrame)
                {
                    _anim.SetTrigger(JumpKey);
                    _anim.ResetTrigger(GroundedKey);

                    // Only play particles when grounded (avoid coyote)
                    if (_player.Grounded)
                    {
                        SetColor(_jumpParticles);
                        SetColor(_launchParticles);
                        _jumpParticles.Play();
                    }
                }

                // Play landing effects and begin ground movement effects
                else if (!_playerGrounded && _player.Grounded)
                {
                    _playerGrounded = true;
                    _anim.SetBool(GroundedKey, true);

                    _moveParticles.Play();
                    _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, _maxParticleFallSpeed, _movement.y);
                    SetColor(_landParticles);
                    _landParticles.Play();
                }
                else if (_playerGrounded && !_player.Grounded)
                {
                    _playerGrounded = false;
                    _anim.SetBool(GroundedKey, false);
                    _moveParticles.Stop();
                }


                //Idle
                else
                {
                    // Prevents flickering transitions to idle
                    _delayToIdle -= Time.deltaTime;
                    if (_delayToIdle < 0)
                        _anim.SetInteger("AnimState", 0);
                }

            }
            else
            {
                
                if (!_player.Death)
                {
                    _anim.SetBool(DeathKey, false);
                    _isDeath = false;
                }
            }



        }
        private void OnDisable()
        {
            _moveParticles.Stop();
        }

        private void OnEnable()
        {
            _moveParticles.Play();
        }

        void SetColor(ParticleSystem ps)
        {
            var main = ps.main;
            main.startColor = _currentGradient;
        }

        #region Animation Keys


        //private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
        private static readonly int GroundedKey = Animator.StringToHash("isGrounded");
        private static readonly int JumpKey = Animator.StringToHash("Jump");

        private static readonly int AttackKey = Animator.StringToHash("Attack");
        private static readonly int HurtKey = Animator.StringToHash("Hurt");
        private static readonly int DashKey = Animator.StringToHash("Dash");
        private static readonly int DeathKey = Animator.StringToHash("isDeath");



        // 쿨타임 관련 변수들



        private float _delayToIdle;

        #endregion
    }
}