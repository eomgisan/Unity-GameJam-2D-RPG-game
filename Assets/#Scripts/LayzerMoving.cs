using System.Collections;
using UnityEngine;

public class LayzerMoving : MonoBehaviour
{
    // Start is called before the first frame update

    public Sprite[] imageLayzer;



    public float readyTime;
    public float duringTime;
    public int _imageIndex;

    public bool RealIsColl;
    public int RealDamege;



    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    void Start()
    {
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _collider = gameObject.GetComponent<Collider2D>();
        _imageIndex = 0;
        StartCoroutine(layzerFire());


    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator layzerFire()
    {

        yield return new WaitUntil(() => GameManager.Instance.isPause == false && GameManager.Instance.isTerraforming == false);
        for (int i = 0; i < 3; i++)
        {
            _spriteRenderer.sprite = imageLayzer[_imageIndex++];
            // 안보여주기 쉬기'
            yield return new WaitForSeconds(0.3f);
            //;

        }

        _collider.enabled = true;

        for (int i = 0; i < 3; i++)
        {

            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.sprite = imageLayzer[_imageIndex++];
        }

        Destroy(gameObject);


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (RealIsColl && !gameObject.CompareTag(collision.tag))
        {
            BaseUnit targetUnit = collision.GetComponent<BaseUnit>();
            if (targetUnit)
            {
                targetUnit.Damage(RealDamege);
                _collider.enabled = false;
            }
        }
    }
}
