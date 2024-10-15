using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

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
    public TextMeshProUGUI textCharges1;

    public int maxCharges1;
    public int cCharges1;


    public int cooldown2;
    public int cCooldown2;
    public Image image2;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI textCharges2;

    public int maxCharges2;
    public int cCharges2;


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
        if (cCharges1 != maxCharges1 || (maxCharges1 == 1 && cCooldown1 > 0))
        {
            cCooldown1 -= 1;

            if (cCooldown1 == 0)
            {
                if (maxCharges1 != 1)
                {
                    cCharges1 += 1;
                    textCharges1.text = cCharges1 > 1 ? "+" + (cCharges1 - 1).ToString() : "";
                    cCooldown1 = cooldown1;
                }


                StartCoroutine(FadeColor(image1, false));
                text1.text = "";
            }
            else
            {
                text1.text = Mathf.Clamp(cCooldown1, 0, 50).ToString();
            }
        }



        if (cCharges2 != maxCharges2 || (maxCharges2 == 1 && cCooldown2 > 0))
        {
            cCooldown2 -= 1;

            if (cCooldown2 == 0)
            {
                if (maxCharges2 != 1)
                {
                    cCharges2 += 1;
                    textCharges2.text = cCharges2 > 1 ? "+" + (cCharges2 - 1).ToString() : "";
                    cCooldown2 = cooldown2;
                }


                StartCoroutine(FadeColor(image2, false));
                text2.text = "";
            }
            else
            {
                text2.text = Mathf.Clamp(cCooldown2, 0, 50).ToString();
            }
        }
    }


    public void SetupUI(Sprite sprite1, int _cooldown1, int _maxCharges1, Sprite sprite2, int _cooldown2, int _maxCharges2)
    {
        gameObject.SetActive(true);

        image1.sprite = sprite1;
        image2.sprite = sprite2;



        cooldown1 = _cooldown1;

        maxCharges1 = _maxCharges1;

        if (maxCharges1 != 1)
        {
            cCooldown1 = cooldown1;
            text1.text = cCooldown1.ToString();
        }
        cCharges1 = 1;
        text1.text = "";
        textCharges1.text = "";



        cooldown2 = _cooldown2;

        maxCharges2 = _maxCharges2;

        if(maxCharges2 != 1)
        {
            cCooldown2 = cooldown2;
            text2.text = cCooldown2.ToString();
        }
        cCharges2 = 1;
        text2.text = "";
        textCharges2.text = "";
    }

    public void TryUseAbility(bool first)
    {
        if (TurnManager.Instance.isMyTurn == false)
        {
            return;
        }

        if (first)
        {
            if (cCharges1 == 0 || (maxCharges1 == 1 && cCooldown1 > 0))
            {
                return;
            }
            ability1Activate.Invoke();
        }
        else
        {
            if (cCharges2 == 0 || (maxCharges2 == 1 && cCooldown2 > 0))
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
            if (maxCharges1 == 1)
            {
                cCooldown1 = cooldown1;

                text1.text = Mathf.Clamp(cCooldown1, 0, 50).ToString();

                StartCoroutine(FadeColor(image1, true));
            }
            else
            {
                cCharges1 -= 1;
                textCharges1.text = cCharges1 > 1 ? "+" + (cCharges1 - 1).ToString() : "";

                text1.text = cooldown1.ToString();

                if (cCharges1 == 0)
                {
                    StartCoroutine(FadeColor(image1, true));
                }
            }
        }
        else
        {
            if (maxCharges2 == 1)
            {
                cCooldown2 = cooldown2;

                text2.text = Mathf.Clamp(cCooldown2, 0, 50).ToString();

                StartCoroutine(FadeColor(image2, true));
            }
            else
            {
                cCharges2 -= 1;
                textCharges2.text = cCharges2 > 1 ? "+" + (cCharges2 - 1).ToString() : "";

                text2.text = cooldown2.ToString();

                if (cCharges2 == 0)
                {
                    StartCoroutine(FadeColor(image2, true));
                }
            }
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


#if UNITY_EDITOR
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F5))
        {
            //reset cooldown

            cCooldown1 = 0;

            if (cCooldown1 == 0)
            {
                StartCoroutine(FadeColor(image1, false));
                text1.text = "";
            }
            else if (cCooldown1 > 0)
            {
                text1.text = Mathf.Clamp(cCooldown1, 0, 50).ToString();
            }


            cCooldown2 = 0;

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
    }

#endif
}
