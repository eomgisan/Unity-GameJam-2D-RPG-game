using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TarodevController;
using UnityEngine;

public class MonsterMovement : BaseUnit, IMonsterController
{
    public float moveSpeed = 3.0f; // 몬스터 이동 속도
    public float patrolDistance = 5.0f; // 좌우로 움직이는 거리

    private bool isMovingRight = true; // 시작 시 오른쪽으로 이동

    public Vector3 Velocity { get; private set; }
    public FrameInput Input { get; private set; }
    public bool Damaged { get; private set; }

    //===========
    private RayRange _raysRight, _raysDown, _raysLeft;
    public bool _colRight, _colDown, _colLeft;
    public LayerMask _groundLayer;
    public float _currentVerticalSpeed, _currentHorizontalSpeed;
    public float _fallSpeed;
    public float _fallClamp;
    public float _changeDirTime;
    private float _beforeChangeDirTime;

    private void Start()
    {
        isMovingRight = false;
        base.ChangeDirection();
    }

    private void Update()
    {
        if (Damaged)
            return;
        base.Update();
    }

    protected override void UnitUpdate()
    {
        CheckCollision();

        Move();
    }

    private void CheckCollision()
    {
        // ???? ?? ?? ??? ????
        CalculateRayRanged();


    }
    private void CalculateRayRanged()
    {
        // This is crying out for some kind of refactor. 
        var b = new Bounds(transform.position, new Vector3(1, 1, 1));
        float _rayBuffer = 0.1f;
        _raysDown = new RayRange(b.min.x + _rayBuffer, b.min.y, b.max.x - _rayBuffer, b.min.y, Vector2.down);
        _raysLeft = new RayRange(b.min.x, b.min.y + _rayBuffer, b.min.x, b.max.y - _rayBuffer, Vector2.left);
        _raysRight = new RayRange(b.max.x, b.min.y + _rayBuffer, b.max.x, b.max.y - _rayBuffer, Vector2.right);

        _colDown = RunDetection(_raysDown);
        _colLeft = RunDetection(_raysLeft);
        _colRight = RunDetection(_raysRight);
        bool RunDetection(RayRange range)
        {
            return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, 0.1f, _groundLayer));
        }
    }


    private IEnumerable<Vector2> EvaluateRayPositions(RayRange range)
    {
        for (var i = 0; i < 3; i++)
        {
            var t = (float)i / (3 - 1);
            yield return Vector2.Lerp(range.Start, range.End, t);
        }
    }

    private void Move()
    {

        _currentHorizontalSpeed = isMovingRight ? 2f : -2f;
        if (_colDown)
        {
            // Move out of the ground
            _currentVerticalSpeed = 0;
        }
        else
        {
            // Add downward force while ascending if we ended the jump early
            var fallSpeed = _fallSpeed;

            // Fall
            _currentVerticalSpeed -= fallSpeed * Time.deltaTime;
            _currentHorizontalSpeed = 0f;

            // Clamp
            if (_currentVerticalSpeed < -_fallClamp) _currentVerticalSpeed = -_fallClamp;
        }


        if (((_colLeft && !isMovingRight) || (_colRight && isMovingRight)) && _changeDirTime + _beforeChangeDirTime < Time.time)
        {
            _currentHorizontalSpeed = 0f;
            ChangeDirection();
        }


        var move = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed) *
                   Time.deltaTime;
        transform.position += move;

        return;
    }

    protected override void ChangeDirection()
    {
        base.ChangeDirection();
        isMovingRight = !isMovingRight;
    }

    public override void Damage(int damage)
    {
        base.Damage(damage);
        if (gameObject.activeSelf)
            StartCoroutine(damageWait());
        Damaged = true;

        IEnumerator damageWait()
        {
            yield return new WaitForSeconds(1f);
            Damaged = false;
        }
    }

    public override void UnitDeath()
    {
        GameManager.Instance.RandomGoldDrop();
        base.UnitDeath();
    }
}
