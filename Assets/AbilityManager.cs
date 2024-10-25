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

    public string[] abilityInfo;

    public Image image0;

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

    public AudioController audioController1;
    public AudioController audioController2;




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

            SettingsManager.SingleTon.AddAudioController(audioController1);
            SettingsManager.SingleTon.AddAudioController(audioController2);
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
                    textCharges1.text = maxCharges1 > 1 ? "x" + cCharges1.ToString() : "";
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
                    textCharges2.text = maxCharges2 > 1 ? "x" + cCharges2.ToString() : "";
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


    public void SetupUI(Sprite sprite1, int _cooldown1, int _maxCharges1, Sprite sprite2, int _cooldown2, int _maxCharges2, Sprite sprite0, string[] _abilityInfo, AudioClip[] clips)
    {
        gameObject.SetActive(true);

        image0.sprite = sprite0;
        image1.sprite = sprite1;
        image2.sprite = sprite2;



        cooldown1 = _cooldown1;

        maxCharges1 = _maxCharges1;

        cCharges1 = 1;
        text1.text = "";
        textCharges1.text = maxCharges1 > 1 ? ("x" + cCharges1.ToString()) : "";

        if (maxCharges1 != 1)
        {
            cCooldown1 = cooldown1;
            text1.text = cCooldown1.ToString();
        }
        if (audioController1 != null)
        {
            audioController1.clips = new AudioClip[1] { clips[0] };
            audioController1.Init();
        }




        cooldown2 = _cooldown2;

        maxCharges2 = _maxCharges2;

        cCharges2 = 1;
        text2.text = "";
        textCharges2.text = maxCharges2 > 1 ? ("x" + cCharges2.ToString()) : "";

        if (maxCharges2 != 1)
        {
            cCooldown2 = cooldown2;
            text2.text = cCooldown2.ToString();
            textCharges2.text = "";
        }

        if (audioController2 != null)
        {
            audioController2.clips = new AudioClip[1] { clips[1] };
            audioController2.Init();
        }


        abilityInfo = _abilityInfo;

        ClickableCollider clickableImage0 = image0.GetComponent<ClickableCollider>();
        ClickableCollider clickableImage1 = image1.GetComponent<ClickableCollider>();
        ClickableCollider clickableImage2 = image2.GetComponent<ClickableCollider>();

        clickableImage0.OnMouseEnterEvent.AddListener(() => TroopInfoManager.Instance.ShowAbility(abilityInfo[0]));
        clickableImage1.OnMouseEnterEvent.AddListener(() => TroopInfoManager.Instance.ShowAbility(abilityInfo[1]));
        clickableImage2.OnMouseEnterEvent.AddListener(() => TroopInfoManager.Instance.ShowAbility(abilityInfo[2]));

        clickableImage0.OnMouseExitEvent.AddListener(() => TroopInfoManager.Instance.ReselectTower());
        clickableImage1.OnMouseExitEvent.AddListener(() => TroopInfoManager.Instance.ReselectTower());
        clickableImage2.OnMouseExitEvent.AddListener(() => TroopInfoManager.Instance.ReselectTower());
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
            if (audioController1 != null)
            {
                audioController1.Play();
            }

            if (maxCharges1 == 1)
            {
                cCooldown1 = cooldown1;

                text1.text = Mathf.Clamp(cCooldown1, 0, 50).ToString();

                StartCoroutine(FadeColor(image1, true));
            }
            else
            {
                cCharges1 -= 1;
                textCharges1.text = maxCharges1 > 1 ? "x" + cCharges1.ToString() : "";

                text1.text = cooldown1.ToString();

                if (cCharges1 == 0)
                {
                    StartCoroutine(FadeColor(image1, true));
                }
            }
        }
        else
        {
            if (audioController2 != null)
            {
                audioController2.Play();
            }

            if (maxCharges2 == 1)
            {
                cCooldown2 = cooldown2;

                text2.text = Mathf.Clamp(cCooldown2, 0, 50).ToString();

                StartCoroutine(FadeColor(image2, true));
            }
            else
            {
                cCharges2 -= 1;
                textCharges2.text = maxCharges2 > 1 ? "x" + cCharges2.ToString() : "";

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
}
