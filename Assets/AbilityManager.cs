using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public GameObject ui;



    public int cooldown1;
    public int cCooldown1;
    public Image image1;
    public TextMeshProUGUI text1;

    public int cooldown2;
    public int cCooldown2;
    public Image image2;
    public TextMeshProUGUI text2;


    [HideInInspector]
    public UnityEvent ability1Activate;
    [HideInInspector]
    public UnityEvent ability2Activate;




    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (TurnManager.Instance != null)
        {
            ui.gameObject.SetActive(true);
            TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => OnTurnGranted());
        }
    }

    public void OnTurnGranted()
    {
        cCooldown1 -= 1;
        text1.text = Mathf.Clamp(cCooldown1, 0, 50).ToString();

        cCooldown2 -= 1;
        text2.text = Mathf.Clamp(cCooldown2, 0, 50).ToString();
    }


    public void SetupUI(Sprite sprite1, int _cooldown1, Sprite sprite2, int _cooldown2)
    {
        gameObject.SetActive(true);

        image1.sprite = sprite1;
        image2.sprite = sprite2;

        cooldown1 = _cooldown1;
        cooldown2 = _cooldown2;

        text1.text = Mathf.Clamp(cCooldown1, 0, 50).ToString();
        text2.text = Mathf.Clamp(cCooldown2, 0, 50).ToString();
    }

    public void TryUseAbility(bool first)
    {
        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }

        if (first)
        {
            if (cCooldown1 > 0)
            {
                return;
            }
            ability1Activate.Invoke();
        }
        else
        {
            if (cCooldown2 > 0)
            {
                return;
            }
            ability2Activate.Invoke();
        }
    }



    public void ConfirmUseAbility(bool first)
    {
        if (first)
        {
            cCooldown1 = cooldown1;
            text1.text = Mathf.Clamp(cCooldown1, 0, 50).ToString();
        }
        else
        {
            cCooldown2 = cooldown2;
            text2.text = Mathf.Clamp(cCooldown2, 0, 50).ToString();
        }
    }
}
