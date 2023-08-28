using System.Collections;
using System.Collections.Generic;
using TarodevController;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{  //게임매니저의 인스턴스를 담는 전역변수(static 변수이지만 이해하기 쉽게 전역변수라고 하겠다).
    //이 게임 내에서 게임매니저 인스턴스는 이 instance에 담긴 녀석만 존재하게 할 것이다.
    //보안을 위해 private으로.
    private static GameManager instance = null;
    private bool isStarted = false;
    public bool isPause = false;
    public bool isTerraforming = false;
    public float unitTimeScale = 1;
    [SerializeField] private GameObject popUp;
    public float popUpDelay;
    public float currPopUpDelay;

    [SerializeField] private TextMeshProUGUI surviveText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Animator startLogo;
    [SerializeField] private GameObject endLogo;

    public float Gold
    {
        get
        {
            return gold;
        }
        set
        {
            gold = value;
            goldText.text = gold.ToString();
        }
    }
    public float gold;
    [SerializeField] private TextMeshProUGUI dropGoldText;
    public float dropGold;
    public float dropGoldChance;

    private float lastValue;
    [SerializeField] private TextMeshProUGUI attackHealthText;
    [SerializeField] private PlayerController player;
    private float playerMaxHealth;
    private float playerAttack;
    private float calculAttackNHealth;
    private CollideWeapon playerWeapon;
    [SerializeField] private List<MonsterAnimator> enemiesAnim = new();
    [SerializeField] private int incDamage;
    [SerializeField] private int totalIncDamage;
    [SerializeField] private int incDamageCost;

    void Awake()
    {
        instance = this;

        SetTimeScale(0.5f);
        SetGoldDropChance(0.5f);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerMaxHealth = player.ReturnMaxHp();
        playerWeapon = player.AttackObject.GetComponent<CollideWeapon>();
        playerAttack = playerWeapon.collDamageValue;
        calculAttackNHealth = playerMaxHealth + playerAttack;
        SetAttackHealthRatio(0.17f);
        Gold = 0;
        isPause = true;
    }

    //게임 매니저 인스턴스에 접근할 수 있는 프로퍼티. static이므로 다른 클래스에서 맘껏 호출할 수 있다.
    public static GameManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    public void StartLogo()
    {
        startLogo.SetTrigger("Start");
        StartCoroutine(StartLogo());
        IEnumerator StartLogo()
        {
            yield return new WaitForSeconds(1f);
            startLogo.transform.parent.gameObject.SetActive(false);
            isStarted = true;
            isPause = false;
        }
    }

    public void IncreaseDamage()
    {
        if (Gold >= incDamageCost)
        {
            Gold -= incDamageCost;
            totalIncDamage += incDamage;
            playerWeapon.SetDamage(totalIncDamage + playerAttack);
        }
    }

    public void SetAttackHealthRatio(float ratio)
    {
        if (ratio > 0.9)
            ratio = 0.9f;
        else if (ratio < 0.1)
            ratio = 0.1f;
        playerAttack = calculAttackNHealth * ratio;
        playerWeapon.SetDamage(totalIncDamage + playerAttack);
        playerMaxHealth = calculAttackNHealth - playerAttack;
        player.SetMaxHp(playerMaxHealth);
        attackHealthText.text = $"Attack: {Mathf.Floor(playerWeapon.collDamageValue)}, Health: {Mathf.Floor(playerMaxHealth)}";
    }

    public void SetTimeScale(float currUnitTimeScale)
    {
        if (currUnitTimeScale == 0f)
            currUnitTimeScale = 0.5f;
        else if (currUnitTimeScale == 0.5f)
            currUnitTimeScale = 1f;
        else if (currUnitTimeScale == 1f)
            currUnitTimeScale = 2f;
        unitTimeScale = currUnitTimeScale;
        if (lastValue != unitTimeScale)
            surviveText.text = $"{Mathf.Floor(currUnitTimeScale * 10) / 10} /S";
        lastValue = unitTimeScale;
        foreach (MonsterAnimator anim in enemiesAnim)
        {
            if (!anim || !anim._anim)
                continue;
            anim._anim.speed = unitTimeScale;
        }
    }


    public void EnableTerraforming()
    {
        this.isTerraforming = true;
    }
    public void DisableTerraforming()
    {
        this.isTerraforming = false;
    }
    public void EnablePause()
    {
        isPause = true;
    }
    public void DisablePause()
    {
        isPause = false;
    }

    public void GameOver()
    {

    }

    public void SetGoldDropChance(float value)
    {
        if (value == 0)
        {
            dropGold = 5;
            dropGoldChance = 0.5f;
        }
        else if (value == 0.5)
        {
            dropGold = 10;
            dropGoldChance = 0.25f;
        }
        else if (value == 1)
        {
            dropGold = 100;
            dropGoldChance = 0.05f;
        }
        dropGoldText.text = $"{dropGold}G / {dropGoldChance * 100}%";

    }

    public void RandomGoldDrop()
    {
        int maxValue = 100;
        float randomValue = (float)Random.Range(0, maxValue) / maxValue;
        print($"{randomValue} {dropGoldChance} / Get! {randomValue <= dropGoldChance}");
        if (randomValue <= dropGoldChance)
        {
            Gold += dropGold;
        }
    }

    public IEnumerator SurviveGold()
    {
        yield return new WaitUntil(() => !isPause && !isTerraforming);
        if (unitTimeScale == 0.5f)
            Gold += 0.5f;
        if (unitTimeScale == 1)
            Gold += 1f;
        else if (unitTimeScale == 2)
            Gold += 2f;
        yield return new WaitForSeconds(1f);
        StartCoroutine(SurviveGold());
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SurviveGold());
    }

    // Update is called once per frame
    void Update()
    {

        if (!isStarted)
            return;

        if (currPopUpDelay > 0)
        {
            currPopUpDelay -= Time.deltaTime;
        }

        if (Input.GetKeyDown("escape") && !isTerraforming && currPopUpDelay <= 0)
        {
            TryOpenPopup();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void OpenPopupWithBtn()
    {
        if (!isTerraforming && currPopUpDelay <= 0)
        {
            TryOpenPopup();
        }
    }

    private void TryOpenPopup()
    {
        isPause = !isPause;
        popUp.SetActive(isPause);

        if (!isPause)
        {
            currPopUpDelay = popUpDelay;
        }
        else
        {
            enemiesAnim.Clear();
            var obj = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var item in obj)
            {
                enemiesAnim.Add(item.GetComponent<MonsterAnimator>());
            }
        }
    }

    public void GameClear()
    {
        endLogo.SetActive(true);
        StartCoroutine(EndDelay());

        IEnumerator EndDelay()
        {
            unitTimeScale = 0;
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
