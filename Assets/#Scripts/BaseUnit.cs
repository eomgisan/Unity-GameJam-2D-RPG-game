using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    [SerializeField] protected float maxHp;
    [SerializeField] protected float currHp;
    [SerializeField] private bool isCollDamage;
    [SerializeField] protected float collDamageValue;
    [SerializeField] SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        Init();

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (GameManager.Instance.isPause || GameManager.Instance.isTerraforming)
        {
            return;
        }

        UnitUpdate();
    }

    private void Init()
    {
        currHp = maxHp;
    }

    public virtual void Damage(int damage)
    {
        currHp -= damage;
        if (currHp <= 0)
        {
            UnitDeath();
        }
    }


    public virtual void UnitDeath()
    {

        gameObject.SetActive(false);
    }

    protected virtual void UnitUpdate()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollDamage && !gameObject.CompareTag(collision.tag))
        {
            BaseUnit targetUnit = collision.GetComponent<BaseUnit>();
            if (targetUnit)
            {
                targetUnit.Damage((int)collDamageValue);
            }
        }
    }

    protected virtual void ChangeDirection()
    {
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }
}
