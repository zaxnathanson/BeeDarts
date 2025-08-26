using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("Reticle")]
    [SerializeField] Image reticleImage;

    [Header("Too Close Indicator")]
    [SerializeField] private GameObject tooCloseIndicator;
    [SerializeField] private Image tooCloseImage;
    [SerializeField] private Sprite[] tooCloseSprites = new Sprite[2];
    [SerializeField] private float animationSpeed = 0.5f;

    [Header("Pulse Animation")]
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Rock Animation")]
    [SerializeField] private float rockAngle = 10f;
    [SerializeField] private float rockSpeed = 3f;

    [Header("Hexagon Transition")]
    [SerializeField] private GameObject hexagonTransitionPanel;
    [SerializeField] private Image hexagonImage;
    [SerializeField] private int hexGridRadius = 8;
    [SerializeField] private float hexSize = 60f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float hexTransitionScale = 1.6f;

    private Coroutine animationCoroutine;
    private Coroutine transitionCoroutine;
    private bool isTransitioning = false;
    private Vector3 originalScale;
    private Image[] hexagonImages;

    [Header("Pause Menu Settings")]
    [SerializeField] private GameObject pauseMenu;

    private bool pauseMenuActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        if (tooCloseIndicator != null)
        {
            tooCloseIndicator.SetActive(false);
            originalScale = tooCloseIndicator.transform.localScale;
        }

        if (hexagonTransitionPanel != null)
            hexagonTransitionPanel.SetActive(false);
    }

    private void Start()
    {
        SetupHexagonGrid();

        StartSceneTransition(-1, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && pauseMenu != null)
        {
            TriggerPauseMenu();
        }

        if (pauseMenuActive && Input.GetKeyDown(KeyCode.Q))
        {
            TriggerPauseMenu();
            StartSceneTransition(0, false);
        }
    }

    public void HideAllUI()
    {

        // hide all child GameObjects
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != null && child.gameObject != null && child.gameObject.name != "PauseMenu")
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void ShowAllUI()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != null && child.gameObject != null && child.gameObject.name != "PauseMenu" && child.gameObject.name != "TooClose")
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private void TriggerPauseMenu()
    {
        if (!pauseMenuActive)
        {
            pauseMenuActive = true;

            pauseMenu.SetActive(true);
            HideAllUI();
            Time.timeScale = 0;
        }
        else
        {
            pauseMenuActive = false;

            pauseMenu.SetActive(false);
            ShowAllUI();
            Time.timeScale = 1f;
        }
    }

    private void SetupHexagonGrid()
    {
        if (hexagonTransitionPanel == null || hexagonImage == null)
        {
            Debug.LogError("Missing hexagon panel or image reference.");
            return;
        }

        // calculate total hexagons using axial coordinates
        int totalHexagons = 1 + (6 * hexGridRadius * (hexGridRadius + 1) / 2);

        hexagonImages = new Image[totalHexagons];
        int hexIndex = 0;

        // generate hexes using axial coordinates (same as your HexRenderer)
        for (int q = -hexGridRadius; q <= hexGridRadius; q++)
        {
            int r1 = Mathf.Max(-hexGridRadius, -q - hexGridRadius);
            int r2 = Mathf.Min(hexGridRadius, -q + hexGridRadius);

            for (int r = r1; r <= r2; r++)
            {
                GameObject hexGO = Instantiate(hexagonImage.gameObject, hexagonTransitionPanel.transform);
                hexagonImages[hexIndex] = hexGO.GetComponent<Image>();

                RectTransform hexRect = hexGO.GetComponent<RectTransform>();

                // set position
                hexRect.anchoredPosition = HexToUI(q, r, hexSize);

                // set size (square to prevent stretching)
                hexRect.sizeDelta = new Vector2(hexSize, hexSize);

                // preserve aspect ratio
                hexGO.GetComponent<Image>().preserveAspect = true;

                // start with zero scale
                hexRect.localScale = Vector3.zero;

                hexIndex++;
            }
        }
    }

    private Vector2 HexToUI(int q, int r, float hexSize)
    {
        // convert axial coordinates to UI position for pointy-top hexagons
        float x = hexSize * Mathf.Sqrt(3f) * (q + r * 0.5f);
        float y = hexSize * 3f / 2f * r;
        return new Vector2(x, y);
    }

    public void ChangeReticleColor(Color newColor)
    {
        reticleImage.color = newColor;
    }

    public void ShowTooCloseIndicator(bool show)
    {
        if (tooCloseIndicator == null) return;

        tooCloseIndicator.SetActive(show);

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (show)
        {
            animationCoroutine = StartCoroutine(AnimateTooCloseIndicator());
        }
        else
        {
            tooCloseIndicator.transform.localScale = originalScale;
            tooCloseIndicator.transform.rotation = Quaternion.identity;
        }
    }

    public void StartSceneTransition(int buildIndex, bool isTransitionIn = false)
    {
        if (isTransitioning) return;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(HexagonTransition(buildIndex, isTransitionIn));
    }

    private IEnumerator HexagonTransition(int buildIndex, bool isTransitionIn)
    {
        if (hexagonTransitionPanel == null || hexagonImages == null) yield break;

        isTransitioning = true;
        hexagonTransitionPanel.SetActive(true);

        float duration = 1f / transitionSpeed;
        float maxDelay = 0.8f;
        float elapsedTime = 0f;

        while (elapsedTime < duration + maxDelay)
        {
            elapsedTime += Time.deltaTime;

            for (int i = 0; i < hexagonImages.Length; i++)
            {
                if (hexagonImages[i] == null) continue;

                // calculate distance from center for wave effect
                float distance = hexagonImages[i].rectTransform.anchoredPosition.magnitude;
                float normalizedDistance = distance / (hexGridRadius * hexSize);

                float delay = (1f - normalizedDistance) * maxDelay;

                float hexagonTime = Mathf.Clamp01((elapsedTime - delay) / duration);

                float scale = isTransitionIn ?
                    Mathf.Lerp(hexTransitionScale, 0f, hexagonTime) :
                    Mathf.Lerp(0f, hexTransitionScale, hexagonTime);

                hexagonImages[i].transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        // ensure final state
        float finalScale = isTransitionIn ? 0f : hexTransitionScale;
        for (int i = 0; i < hexagonImages.Length; i++)
        {
            if (hexagonImages[i] != null)
                hexagonImages[i].transform.localScale = Vector3.one * finalScale;
        }

        if (isTransitionIn)
            hexagonTransitionPanel.SetActive(false);

        isTransitioning = false;
        transitionCoroutine = null;

        // actually switch the scenes
        if (buildIndex > -1)
        {
            if (ShakeManager.Instance != null)
            {
                ShakeManager.Instance.StopShake();
            }

            GameManager.Instance.LoadScene(buildIndex);
        }
    }

    private IEnumerator AnimateTooCloseIndicator()
    {
        if (tooCloseImage == null || tooCloseSprites == null || tooCloseSprites.Length < 2)
        {
            Debug.LogWarning("Too close indicator not properly configured!");
            yield break;
        }

        int currentFrame = 0;
        float pulseTime = 0f;
        float spriteTimer = 0f;

        while (true)
        {
            spriteTimer += Time.deltaTime;
            if (spriteTimer >= animationSpeed)
            {
                tooCloseImage.sprite = tooCloseSprites[currentFrame];
                currentFrame = 1 - currentFrame;
                spriteTimer = 0f;
            }

            pulseTime += Time.deltaTime;

            // pulse effect
            float scaleFactor = 1f + (pulseScale - 1f) * Mathf.Sin(pulseTime * pulseSpeed) * 0.5f;
            tooCloseIndicator.transform.localScale = originalScale * scaleFactor;

            // rock effect
            float rotationZ = Mathf.Sin(pulseTime * rockSpeed) * rockAngle;
            tooCloseIndicator.transform.rotation = Quaternion.Euler(0, 0, rotationZ);

            yield return null;
        }
    }
}