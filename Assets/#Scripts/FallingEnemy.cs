using System.Collections.Generic;
using System.Linq;
using TarodevController;
using UnityEngine;

public class FallingEnemy : BaseUnit
{
    public float moveSpeed = 3.0f; // 몬스터 이동 속도
    //===========
    private RayRange _raysRight, _raysDown, _raysLeft;
    public bool _colDown;
    public LayerMask _groundLayer;
    public LayerMask _playerLayer;
    public float _currentVerticalSpeed;
    public float _fallSpeed;
    public float _fallClamp;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }


    protected override void UnitUpdate()
    {
        CheckCollision();

        Move();
    }

    private void CheckCollision()
    {
        // 레이 좌 우 아래 선언
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

        _colDown = LandDetection(_raysDown) || PlayerDetection(_raysDown);


        bool LandDetection(RayRange range)
        {
            return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, 0.1f, _groundLayer));
        }

        bool PlayerDetection(RayRange range)
        {
            return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, 0.1f, _playerLayer));
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


        if (_colDown)
        {
            // Move out of the ground
            Destroy(gameObject, 0.05f);
        }
        else
        {
            // Add downward force while ascending if we ended the jump early
            var fallSpeed = _fallSpeed;

            // Fall
            _currentVerticalSpeed -= fallSpeed * Time.deltaTime;


            // Clamp
            if (_currentVerticalSpeed < -_fallClamp) _currentVerticalSpeed = -_fallClamp;
        }


        var move = new Vector3(0f, _currentVerticalSpeed) * Time.deltaTime;
        transform.position += move;

        return;
    }

}
