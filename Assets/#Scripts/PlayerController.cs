using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TarodevController
{
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// Right now it only contains movement and jumping, but it should be pretty easy to expand... I may even do it myself
    /// if there's enough interest. You can play and compete for best times here: https://tarodev.itch.io/
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/GqeHHnhHpz
    /// </summary>
    public class PlayerController : BaseUnit, IPlayerController
    {
        // Public for external hooks

        public GameObject AttackObject;
        public Vector3 Velocity { get; private set; }
        public FrameInput Input { get; private set; }
        public bool JumpingThisFrame { get; private set; }
        public bool LandingThisFrame { get; private set; }
        public Vector3 RawMovement { get; private set; }
        public bool Grounded => _colDown;
        public bool Death { get; private set; }
        public bool Dashing { get; private set; }
        public bool Attacking { get; private set; }

        public bool Jumping { get; private set; }
        public bool Damaged { get; private set; }
        private bool invinclible = false;
        [SerializeField] private float invincibleTime;

        private Vector3 _lastPosition;
        private float _currentHorizontalSpeed, _currentVerticalSpeed;

        private float beforeInputX;

        // This is horrible, but for some reason colliders are not fully established when update starts...
        private bool _active;

        [SerializeField] private float regenHp;
        [SerializeField] private float regenHpTime;
        [SerializeField] private float currRegenHpTime;
        [SerializeField] private float attackTime;
        [SerializeField] private Slider hpBar;
        [SerializeField] private Image dashDelayImage;

        void Awake() => Invoke(nameof(Activate), 0.5f);
        void Activate()
        {
            _active = true;
            Death = false;
            Dashing = false;
            Attacking = false;
            Damaged = false;
            beforeInputX = 1f;
        }

        private void Update()
        {
            if (!_active) return;

            base.Update();
            if (Death)
            {
                if (currRegenHpTime < regenHpTime && currHp < maxHp)
                {
                    currRegenHpTime += Time.deltaTime;
                }
                else
                {
                    currRegenHpTime = 0f;
                    if (currHp + regenHp < maxHp)
                    {
                        currHp += regenHp;
                    }
                    else
                    {
                        currHp = maxHp;
                        Death = false;
                    }
                    hpBar.value = currHp / maxHp;
                }
            }


            if (Input.X != 0)
            {
                beforeInputX = Input.X > 0 ? 1f : -1f;
                AttackObject.transform.position = gameObject.transform.position + new Vector3(0.5f * beforeInputX, 0f, 0f);
            }

        }

        protected override void UnitUpdate()
        {
            // Calculate velocity
            Velocity = (transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = transform.position;

            GatherInput();

            RunCollisionChecks();

            CalculateWalk(); // Horizontal movement

            CalculateJumpApex(); // Affects fall speed, so calculate before gravity

            CalculateGravity(); // Vertical movement

            CalculateJump(); // Possibly overrides vertical

            MoveCharacter(); // Actually perform the axis movement
        }

        public float ReturnMaxHp()
        {
            return maxHp;
        }

        public void SetMaxHp(float maxHp)
        {
            float ratio = currHp / maxHp;
            this.maxHp = maxHp;
            currHp = this.maxHp * ratio;
        }

        #region Gather Input

        private void GatherInput()
        {

            Input = new FrameInput
            {
                JumpDown = UnityEngine.Input.GetButtonDown("Jump") && !Death,
                JumpUp = UnityEngine.Input.GetButtonUp("Jump") && !Death,


                Attack = UnityEngine.Input.GetMouseButtonDown(0) && (Attacking == false) && (Grounded == true) && !Death,


                Dash = UnityEngine.Input.GetMouseButtonDown(1) && Time.time > lastDashPressed + _dashDelay && Dashing == false && !Death,

                X = Death || Attacking ? 0f : UnityEngine.Input.GetAxisRaw("Horizontal"),
            };


            if (Input.JumpDown)
            {
                _lastJumpPressed = Time.time;
            }


            if (Input.Attack)
            {
                StartCoroutine(AttackTime());
            }

            if(Input.Dash)
            {
                StartDash();
            }

            //if (Input.Attack)
            //{
            //    Death = !Death;
            //}
        }

        #endregion

        #region Collisions

        [Header("COLLISION")][SerializeField] private Bounds _characterBounds;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private int _detectorCount = 3;
        [SerializeField] private float _detectionRayLength = 0.1f;
        [SerializeField][Range(0.1f, 0.3f)] private float _rayBuffer = 0.1f; // Prevents side detectors hitting the ground

        private RayRange _raysUp, _raysRight, _raysDown, _raysLeft;
        private bool _colUp, _colRight, _colDown, _colLeft;

        private float _timeLeftGrounded;

        // We use these raycast checks for pre-collision information
        private void RunCollisionChecks()
        {
            // Generate ray ranges. 
            CalculateRayRanged();

            // Ground
            LandingThisFrame = false;
            var groundedCheck = RunDetection(_raysDown);
            if (_colDown && !groundedCheck) _timeLeftGrounded = Time.time; // Only trigger when first leaving
            else if (!_colDown && groundedCheck)
            {
                _coyoteUsable = true; // Only trigger when first touching
                LandingThisFrame = true;
            }

            _colDown = groundedCheck;

            // The rest
            _colUp = RunDetection(_raysUp);
            _colLeft = RunDetection(_raysLeft);
            _colRight = RunDetection(_raysRight);

            bool RunDetection(RayRange range)
            {
                return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, _detectionRayLength, _groundLayer));
            }
        }

        private void CalculateRayRanged()
        {
            // This is crying out for some kind of refactor. 
            var b = new Bounds(transform.position, _characterBounds.size);

            _raysDown = new RayRange(b.min.x + _rayBuffer, b.min.y, b.max.x - _rayBuffer, b.min.y, Vector2.down);
            _raysUp = new RayRange(b.min.x + _rayBuffer, b.max.y, b.max.x - _rayBuffer, b.max.y, Vector2.up);
            _raysLeft = new RayRange(b.min.x, b.min.y + _rayBuffer, b.min.x, b.max.y - _rayBuffer, Vector2.left);
            _raysRight = new RayRange(b.max.x, b.min.y + _rayBuffer, b.max.x, b.max.y - _rayBuffer, Vector2.right);
        }


        private IEnumerable<Vector2> EvaluateRayPositions(RayRange range)
        {
            for (var i = 0; i < _detectorCount; i++)
            {
                var t = (float)i / (_detectorCount - 1);
                yield return Vector2.Lerp(range.Start, range.End, t);
            }
        }

        private void OnDrawGizmos()
        {
            // Bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + _characterBounds.center, _characterBounds.size);

            // Rays
            if (!Application.isPlaying)
            {
                CalculateRayRanged();
                Gizmos.color = Color.blue;
                foreach (var range in new List<RayRange> { _raysUp, _raysRight, _raysDown, _raysLeft })
                {
                    foreach (var point in EvaluateRayPositions(range))
                    {
                        Gizmos.DrawRay(point, range.Dir * _detectionRayLength);
                    }
                }
            }

            if (!Application.isPlaying) return;

            // Draw the future position. Handy for visualizing gravity
            Gizmos.color = Color.red;
            var move = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed) * Time.deltaTime;
            Gizmos.DrawWireCube(transform.position + move, _characterBounds.size);
        }

        #endregion

        #region Walk

        [Header("WALKING")][SerializeField] private float _acceleration = 45;
        [SerializeField] private float _moveClamp = 13;
        [SerializeField] private float _deAcceleration = 30f;
        [SerializeField] private float _apexBonus = 2;
        [SerializeField] private float _dashAcceleration = 400f;
        [SerializeField] private float _dashBuffer = 0.1f;
        [SerializeField] private float _dashDelay;

        private float lastDashPressed;
        private float beforeDashX;

        private void CalculateWalk()
        {
          


                // 17:06 대쉬 쿨타임 추가        
            if (Input.X != 0 && lastDashPressed + _dashBuffer <= Time.time)
            {
                // 이전 대쉬 이동 값 반전 여부 확인 및 속도 감속 확인하여 현재 이동 속도 0으로 설정

                // Set horizontal move speed
                _currentHorizontalSpeed += Input.X * _acceleration * Time.deltaTime;

                // clamped by max frame movement
                _currentHorizontalSpeed = Mathf.Clamp(_currentHorizontalSpeed, -_moveClamp, _moveClamp);

                // Apply bonus at the apex of a jump
                var apexBonus = Mathf.Sign(Input.X) * _apexBonus * _apexPoint;
                _currentHorizontalSpeed += apexBonus * Time.deltaTime;
            }
            else
            {
                // No input. Let's slow the character down
                _currentHorizontalSpeed = Mathf.MoveTowards(_currentHorizontalSpeed, 0, _deAcceleration * Time.deltaTime);
            }

            // 16:43 대쉬시 이동 값 추가
            if (Dashing || lastDashPressed + _dashBuffer > Time.time && !Jumping)
            {
                _currentHorizontalSpeed += beforeDashX * _dashAcceleration * Time.deltaTime;
                _currentHorizontalSpeed = Mathf.Clamp(_currentHorizontalSpeed, -_moveClamp * 2, _moveClamp * 2);


                // Apply bonus at the apex of a jump
                var apexBonus = Mathf.Sign(Input.X) * _apexBonus * _apexPoint;
                _currentHorizontalSpeed += apexBonus * Time.deltaTime;


                // 코루틴 시작 0.5 초 


            }


            if (Dashing && lastDashPressed + _dashBuffer < Time.time)
            {
                invinclible = false;
                Dashing = false;
                _currentHorizontalSpeed = 0;
            }

            if (lastDashPressed == 0)
                dashDelayImage.fillAmount = 0;
            else
                dashDelayImage.fillAmount = 1 - ((Time.time - lastDashPressed) / _dashDelay);

            if (_currentHorizontalSpeed > 0 && _colRight || _currentHorizontalSpeed < 0 && _colLeft)
            {
                // Don't walk through walls
                _currentHorizontalSpeed = 0;
            }
            // 16:43 대미지시 이동값 제한 추가
            if (Damaged)
            {
                _currentHorizontalSpeed = 0;
            }

        }

        #endregion

        #region Gravity

        [Header("GRAVITY")][SerializeField] private float _fallClamp = -40f;
        [SerializeField] private float _minFallSpeed = 80f;
        [SerializeField] private float _maxFallSpeed = 120f;
        private float _fallSpeed;

        private void CalculateGravity()
        {
            if (_colDown)
            {
                // Move out of the ground
                if (_currentVerticalSpeed < 0) _currentVerticalSpeed = 0;
            }
            else
            {
                // Add downward force while ascending if we ended the jump early
                var fallSpeed = _endedJumpEarly && _currentVerticalSpeed > 0 ? _fallSpeed * _jumpEndEarlyGravityModifier : _fallSpeed;

                // Fall
                _currentVerticalSpeed -= fallSpeed * Time.deltaTime;

                // Clamp
                if (_currentVerticalSpeed < _fallClamp) _currentVerticalSpeed = _fallClamp;
            }
        }

        #endregion

        #region Jump

        [Header("JUMPING")][SerializeField] private float _jumpHeight = 30;
        [SerializeField] private float _jumpApexThreshold = 10f;
        [SerializeField] private float _coyoteTimeThreshold = 0.1f;
        [SerializeField] private float _jumpBuffer = 0.1f;
        [SerializeField] private float _jumpEndEarlyGravityModifier = 3;
        private bool _coyoteUsable;
        private bool _endedJumpEarly = true;
        private float _apexPoint; // Becomes 1 at the apex of a jump
        private float _lastJumpPressed;
        private bool CanUseCoyote => _coyoteUsable && !_colDown && _timeLeftGrounded + _coyoteTimeThreshold > Time.time;
        private bool HasBufferedJump => _colDown && _lastJumpPressed + _jumpBuffer > Time.time;

        private void CalculateJumpApex()
        {
            if (!_colDown)
            {
                // Gets stronger the closer to the top of the jump
                _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
                _fallSpeed = Mathf.Lerp(_minFallSpeed, _maxFallSpeed, _apexPoint);
            }
            else
            {
                _apexPoint = 0;
            }
        }

        private void CalculateJump()
        {
            // Jump if: grounded or within coyote threshold || sufficient jump buffer
            if ((Input.JumpDown && CanUseCoyote || HasBufferedJump) && !Damaged && !Attacking)
            {
                _currentVerticalSpeed = _jumpHeight;
                _endedJumpEarly = false;
                _coyoteUsable = false;
                _timeLeftGrounded = float.MinValue;
                JumpingThisFrame = true;
                Jumping = true;
            }
            else
            {
                JumpingThisFrame = false;
            }

            // End the jump early if button released
            if (!_colDown && Input.JumpUp && !_endedJumpEarly && Velocity.y > 0)
            {
                // _currentVerticalSpeed = 0;
                _endedJumpEarly = true;
                Jumping = false;
            }

            if (_colUp)
            {
                if (_currentVerticalSpeed > 0) _currentVerticalSpeed = 0;
            }
        }

        #endregion

        #region Move

        [Header("MOVE")]
        [SerializeField, Tooltip("Raising this value increases collision accuracy at the cost of performance.")]
        private int _freeColliderIterations = 10;

        // We cast our bounds before moving to avoid future collisions
        private void MoveCharacter()
        {



            var pos = transform.position;
            RawMovement = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed); // Used externally
            var move = RawMovement * Time.deltaTime;
            var furthestPoint = pos + move;

            // check furthest movement. If nothing hit, move and don't do extra checks
            var hit = Physics2D.OverlapBox(furthestPoint, _characterBounds.size, 0, _groundLayer);
            if (!hit)
            {
                transform.position += move;
                return;
            }

            // otherwise increment away from current pos; see what closest position we can move to
            var positionToMoveTo = transform.position;
            for (int i = 1; i < _freeColliderIterations; i++)
            {
                // increment to check all but furthestPoint - we did that already
                var t = (float)i / _freeColliderIterations;
                var posToTry = Vector2.Lerp(pos, furthestPoint, t);

                if (Physics2D.OverlapBox(posToTry, _characterBounds.size, 0, _groundLayer))
                {
                    transform.position = positionToMoveTo;

                    // We've landed on a corner or hit our head on a ledge. Nudge the player gently
                    if (i == 1)
                    {
                        if (_currentVerticalSpeed < 0) _currentVerticalSpeed = 0;
                        var dir = transform.position - hit.transform.position;
                        transform.position += dir.normalized * move.magnitude;
                    }

                    return;
                }

                positionToMoveTo = posToTry;
            }
        }

        #endregion

        #region CoRoutine

        IEnumerator ChangeLayer()
        {
            gameObject.layer = LayerMask.NameToLayer("Dash");
            yield return new WaitForSeconds(0.5f);
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        private void StartDash()
        {
            Dashing = true;
            lastDashPressed = Time.time;
            if (beforeInputX == 0)
                beforeInputX = 1;
            beforeDashX = beforeInputX;
            invinclible = true;
            StartCoroutine(ChangeLayer());
        }

        IEnumerator AttackTime()
        {
            Attacking = true;

            AttackObject.SetActive(true);

            yield return new WaitForSeconds(attackTime);

            AttackObject.SetActive(false);

            Attacking = false;
        }

        #endregion

        public override void Damage(int damage)
        {
            if (Death || invinclible)
                return;
            base.Damage(damage);
            invinclible = true;
            Damaged = true;
            hpBar.value = currHp / maxHp;
            StartCoroutine(CamAnim());
            StartCoroutine(HitDelayTime());
            StartCoroutine(WaitInvincibleTime());

            IEnumerator CamAnim()
            {
                for (int i = 0; i < 20; i++)
                {
                    Camera.main.transform.position = new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, -10);
                    yield return new WaitForSeconds(0.01f);
                }
            }

            IEnumerator HitDelayTime()
            {
                yield return new WaitForSeconds(0.1f);
                Damaged = false;
            }

            IEnumerator WaitInvincibleTime()
            {
                yield return new WaitForSeconds(invincibleTime);
                invinclible = false;
            }
        }

        public override void UnitDeath()
        {
            Death = true;
            currHp = 0;
            GameManager.Instance.GameOver();
        }
    }
}