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


    public Color onCooldownColor;
    public float colorFadeTime;


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
            ui.SetActive(true);
            TurnManager.Instance.OnMyTurnStartedEvent.AddListener(() => OnTurnGranted());
        }
    }

    public void OnTurnGranted()
    {
        cCooldown1 -= 1;

        if (cCooldown1 == 0)
        {
            StartCoroutine(FadeColor(image1, false));
            text1.text = "";
        }
        else if (cCooldown1 > 0)
        {
            text1.text = Mathf.Clamp(cCooldown1, 0, 50).ToString();
        }


        cCooldown2 -= 1;

        if (cCooldown2 == 0)
        {
            StartCoroutine(FadeColor(image2, false));
            text2.text = "";
        }
        else if (cCooldown2 > 0)
        {
            text2.text = Mathf.Clamp(cCooldown2, 0, 50).ToString();
        }
    }


    public void SetupUI(Sprite sprite1, int _cooldown1, Sprite sprite2, int _cooldown2)
    {
        gameObject.SetActive(true);

        image1.sprite = sprite1;
        image2.sprite = sprite2;

        cooldown1 = _cooldown1;
        cooldown2 = _cooldown2;
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

            StartCoroutine(FadeColor(image1, true));
        }
        else
        {
            cCooldown2 = cooldown2;
            text2.text = Mathf.Clamp(cCooldown2, 0, 50).ToString();

            StartCoroutine(FadeColor(image2, true));
        }
    }



    private IEnumerator FadeColor(Image offCooldownImage, bool onCooldown)
    {
        float elapsedTime = 0;

        while (true)
        {
            yield return null;

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / colorFadeTime);

            if (onCooldown)
            {
                offCooldownImage.color = Color.Lerp(offCooldownImage.color, onCooldownColor, t);

                if (offCooldownImage.color == onCooldownColor)
                {
                    yield break;
                }
            }
            else
            {
                offCooldownImage.color = Color.Lerp(offCooldownImage.color, Color.white, t);

                if (offCooldownImage.color == Color.white)
                {
                    yield break;
                }
            }
        }
    }
}
