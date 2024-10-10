using System.Collections;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class UnselectedFade : MonoBehaviour
{
    public Image[] images;

    public Color deselectedColor;

    public float colorFadeTime;


    public void SelectNew(Image newSelectedImage)
    {
        StopAllCoroutines();

        StartCoroutine(FadeColor(newSelectedImage));
    }


    private IEnumerator FadeColor(Image newSelectedImage)
    {
        float elapsedTime = 0;

        while (newSelectedImage.color != Color.white)
        {
            yield return null;

            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / colorFadeTime);


            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == newSelectedImage)
                {
                    images[i].color = Color.Lerp(images[i].color, Color.white, t);
                    continue;
                }

                images[i].color = Color.Lerp(images[i].color, deselectedColor, t);
            }
        }
    }
}