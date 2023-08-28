using UnityEngine;

public class CollideWeapon : MonoBehaviour
{
    [field: SerializeField] public float collDamageValue { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.CompareTag(collision.tag))
        {
            BaseUnit targetUnit = collision.GetComponent<BaseUnit>();
            if (targetUnit)
            {
                targetUnit.Damage((int)collDamageValue);
            }
        }
    }

    public void SetDamage(float damage)
    {
        collDamageValue = damage;
    }
}
