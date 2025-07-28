using UnityEngine;

public class SpriteSwitcher : MonoBehaviour
{
    [Header("Sprites")]

    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;

    [Header("Settings")]

    [SerializeField] private float switchInterval = 0.5f;

    private bool first = true;

    private SpriteRenderer spriteRenderer;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
            spriteRenderer.sprite = sprite1;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            SwitchSprite();
            timer = 0f;
        }
    }

    private void SwitchSprite()
    {
        first = !first;
        spriteRenderer.sprite = first ? sprite1 : sprite2;
    }
}