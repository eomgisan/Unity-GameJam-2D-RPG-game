using System.Collections;
using TarodevController;
using UnityEngine;

public class MonsterMovementTowardsPlayer : BaseUnit, IMonsterController
{
    public Transform target; // 유저의 위치를 받아오기 위한 변수
    public float moveSpeed = 5f; // 몬스터의 이동 속도
    private float lastX;
    public Vector3 Velocity { get; private set; }
    public FrameInput Input { get; private set; }
    public bool Damaged { get; private set; }

    private void Start()
    {
        if (!target)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }


        Vector3 moveDirection = (target.position - transform.position).normalized;
        if (moveDirection.x > 0)
        {
            base.ChangeDirection();
        }
    }

    private void Update()
    {
        if (Damaged)
            return;
        base.Update();

    }

    protected override void UnitUpdate()
    {
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 moveDirection = (target.position - transform.position).normalized;
        if ((lastX > 0 && moveDirection.x < 0) || (lastX < 0 && moveDirection.x > 0))
            ChangeDirection();
        lastX = moveDirection.x;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime * GameManager.Instance.unitTimeScale);
    }

    public override void Damage(int damage)
    {
        Damaged = true;
        base.Damage(damage);
        if (!gameObject.activeSelf)
            return;

        StartCoroutine(damageWait());

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