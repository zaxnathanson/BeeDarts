using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;

    [Header("Reticle")]
    [SerializeField] Image reticleImage;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    public void ChangeReticleColor(Color newColor)
    {
        reticleImage.color = newColor;
    }
}
