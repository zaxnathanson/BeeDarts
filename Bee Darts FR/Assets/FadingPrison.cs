using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FadingPrison : MonoBehaviour
{
    public List<GameObject> objectsToFade;
    public float fadeDuration = 5f;

    private void Start()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    [ContextMenu("Fade Out")]
    public void StartFadeOut()
    {
        StartCoroutine(FadeOutAll());
    }

    IEnumerator FadeOutAll()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float blend = timer / fadeDuration;

            foreach (GameObject obj in objectsToFade)
            {
                if (obj != null)
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        Color color = renderer.material.color;
                        color.a = Mathf.Lerp(1f, 0f, blend);
                        renderer.material.color = color;
                    }
                }
            }

            yield return null;
        }

        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }
}