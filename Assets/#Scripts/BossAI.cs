using System.Collections;
using UnityEngine;

public class BossAI : BaseUnit
{

    //-----------------보스용
    public GameObject[] _AttackObject;

    public int _AttackMod = 0;
    public float _delayTime = 2f;
    public float _delayLayzerTime = 25f;

    public bool isDeath;
    public bool isHurt;
    public bool isDone;
    public Vector3 genPos;

    private float _beforeAttackTime;
    private float _beforeLayzerTime;

    //----- animator 및 파티클 용
    public GameObject animationObj;
    public GameObject particleObj;

    private Animator _anim;
    private ParticleSystem _hurtParticle;

    void Start()
    {
        _beforeAttackTime = Time.time;
        _anim = animationObj.GetComponent<Animator>();
        _hurtParticle = particleObj.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame





    protected override void UnitUpdate()
    {
        // 쿨타임 아직 안되었으면 아무것도 안함
        if (Time.time <= (_delayTime / GameManager.Instance.unitTimeScale) + _beforeAttackTime)
        {
            return;
        }

        // 쿨타임 되었을 경우
        _AttackMod = Random.Range(0, 4);

        while (_delayLayzerTime + _beforeLayzerTime < Time.time && _AttackMod == 3)
        {
            _AttackMod = Random.Range(0, 4);
        }

        _anim.SetInteger("AttackMod", _AttackMod);
        _anim.SetTrigger("doAttack");


        switch (_AttackMod)
        {
            case 0:
                // 날아다니는 적 소환
                Instantiate(_AttackObject[0], genPos, _AttackObject[1].transform.rotation);
                break;
            case 1:
                Instantiate(_AttackObject[1], genPos, _AttackObject[2].transform.rotation);
                // 걸어다니는 적 소환
                break;

            case 2:
                // 똥 뿌리기 (세로 공격)
                StartCoroutine(genDDONG());
                break;
            case 3:
                // 레이저 공격 ( 가로 공격 )

                StartCoroutine(layzerAttck());
                _beforeLayzerTime = Time.time;

                break;
        }

        _beforeAttackTime = Time.time;
    }

    IEnumerator genDDONG()
    {

        _AttackObject[2].SetActive(true);
        yield return new WaitForSeconds(2f);
        _AttackObject[2].SetActive(false);
    }

    IEnumerator layzerAttck()
    {
        yield return new WaitUntil(() => GameManager.Instance.isPause == false && GameManager.Instance.isTerraforming == false);


        for (int count = 1; count <= 3; count++)
        {
            float yPos = Random.Range(-4, 14f);
            Instantiate(_AttackObject[3], new Vector3(0, yPos, 0f), _AttackObject[3].transform.rotation);
            yield return new WaitForSeconds(0.5f);
        }




    }

    public override void Damage(int damage)
    {
        currHp -= damage;
        _anim.SetTrigger("Hurt");

        _hurtParticle.Play();

        if (currHp <= 0)
        {
            UnitDeath();
        }
    }

    public override void UnitDeath()
    {
        _anim.SetTrigger("Death");
        GameManager.Instance.GameClear();


    }
}
