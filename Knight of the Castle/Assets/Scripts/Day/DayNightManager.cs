using System.Collections;
using UnityEngine;
using TMPro;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager Instance { get; private set; }

    public enum GamePhase { PrepPhase, DayPhase, PlayerDied }
    public enum Season { SPRING_SEASON, SUMMER_SEASON, AUTUMN_SEASON, WINTER_SEASON }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI prepTimerText; // Shown ONLY in Prep Phase
    [SerializeField] private TextMeshProUGUI dayTimerText;  // Shown ONLY in Day Phase
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI seasonText;
    [SerializeField] private TextMeshProUGUI bannerText;    // Floating "DAY HAS STARTED NOW!" banner
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private GameObject defensePlacementUI; // Slot bar widget

    [Header("Environment Toggle GameObjects")]
    [SerializeField] private GameObject dayVisuals;
    [SerializeField] private GameObject nightVisuals;
    [SerializeField] private GameObject springVisuals;
    [SerializeField] private GameObject summerVisuals;
    [SerializeField] private GameObject autumnVisuals;
    [SerializeField] private GameObject winterVisuals;

    [Header("Timer Configurations (In Seconds)")]
    [Tooltip("Time given to player to build defenses before the day starts.")]
    [SerializeField] private float prepDuration = 60f; // 1 minute prep
    [Tooltip("Duration of the active day phase.")]
    [SerializeField] private float dayDuration = 600f; // 10 minutes day

    [Header("Season Configurations")]
    [SerializeField] private int daysPerSeason = 5; // Changes season every 5 days

    [Header("Polish & Effect Settings")]
    [SerializeField] private float lowTimeThreshold = 10f; // Seconds left to trigger red pulse
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private float punchScaleAmount = 1.3f;
    [SerializeField] private float punchDuration = 0.25f;

    [Header("Economy & Database")]
    [SerializeField] private int coins = 100;
    [SerializeField] private DefenseDatabaseSO defenseDatabase;

    [Header("Current Progress State (Read Only)")]
    [SerializeField] private GamePhase currentPhase;
    [SerializeField] private Season currentSeason = Season.SPRING_SEASON;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private float timeRemaining;

    [Header("Checkpoint System")]
    [SerializeField] private int checkpointDay = 1;
    [SerializeField] private Season checkpointSeason = Season.SPRING_SEASON;

    [Header("Low-Poly Foliage Control")]
    [SerializeField] private Material grassMaterial;
    [SerializeField] private Material groundMaterial;
    [SerializeField] private float seasonTransitionSpeed = 1.0f;

    private float targetSeasonValue = 0f;
    private float currentSeasonValue = 0f;

    private Coroutine timerCoroutine;
    private Vector3 prepTimerOriginalScale;
    private Vector3 dayTimerOriginalScale;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (prepTimerText != null) prepTimerOriginalScale = prepTimerText.transform.localScale;
        if (dayTimerText != null) dayTimerOriginalScale = dayTimerText.transform.localScale;
    }

    private void Start()
    {
        UpdateCoinsUI();
        StartPrepPhase();
    }


    private void Update()
    {
        // Smoothly transition progress float between 0.0 and 3.0
        if (Mathf.Abs(currentSeasonValue - targetSeasonValue) > 0.001f)
        {
            currentSeasonValue = Mathf.MoveTowards(currentSeasonValue, targetSeasonValue, Time.deltaTime * seasonTransitionSpeed);

            // Pass globally to all low-poly grass and ground shaders
            Shader.SetGlobalFloat("_SeasonProgress", currentSeasonValue);
        }
    }

    private void UpdateShaderSeasonTarget()
    {
        switch (currentSeason)
        {
            case Season.SPRING_SEASON: targetSeasonValue = 0f; break;
            case Season.SUMMER_SEASON: targetSeasonValue = 1f; break;
            case Season.AUTUMN_SEASON: targetSeasonValue = 2f; break;
            case Season.WINTER_SEASON: targetSeasonValue = 3f; break;
        }
    }

    // ==========================================
    // PHASE & FLOW MANAGEMENT
    // ==========================================

    public void StartPrepPhase()
    {
        currentPhase = GamePhase.PrepPhase;
        timeRemaining = prepDuration;

        ToggleEnvironmentVisuals();
        ApplyShaderSeasonKeywords();

        // Show defense building UI during Prep Phase
        if (defensePlacementUI != null) defensePlacementUI.SetActive(true);

        StartCoroutine(ShowBannerRoutine("PREPARATION PHASE: PLACE DEFENSES!"));

        UpdateUI();
        TriggerPunchEffect(prepTimerText, prepTimerOriginalScale);

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(RunTimer());

        GameEvents.TriggerPrepPhaseStart(prepDuration);
    }

    public void StartDayPhase()
    {
        currentPhase = GamePhase.DayPhase;
        timeRemaining = dayDuration;

        ToggleEnvironmentVisuals();
        ApplyShaderSeasonKeywords();

        // Hide defense building UI once Day starts (Cannot place defenses)
        if (defensePlacementUI != null) defensePlacementUI.SetActive(false);

        StartCoroutine(ShowBannerRoutine("DAY HAS STARTED NOW!"));

        UpdateUI();
        TriggerPunchEffect(dayTimerText, dayTimerOriginalScale);

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(RunTimer());

        GameEvents.TriggerDayPhaseStart(dayDuration);
    }

    private IEnumerator RunTimer()
    {
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();
            yield return null;
        }

        timeRemaining = 0;
        UpdateTimerUI();
        OnTimerComplete();
    }

    private void OnTimerComplete()
    {
        if (currentPhase == GamePhase.PrepPhase)
        {
            Debug.Log($"Prep Phase for Day {currentDay} ended! Day starting...");
            StartDayPhase();
        }
        else if (currentPhase == GamePhase.DayPhase)
        {
            Debug.Log($"Day {currentDay} survived!");
            AdvanceDay(1);
            StartPrepPhase();
        }
    }

    private void AdvanceDay(int amount = 1)
    {
        currentDay += amount;

        int seasonIndex = ((currentDay - 1) / daysPerSeason) % 4;
        currentSeason = (Season)seasonIndex;

        UpdateUI();
        ToggleEnvironmentVisuals();
        ApplyShaderSeasonKeywords();
    }

    // ==========================================
    // ENVIRONMENT & SHADER TOGGLES
    // ==========================================

    private void ToggleEnvironmentVisuals()
    {
        bool isPrep = (currentPhase == GamePhase.PrepPhase);

        // Day / Night GOs
        if (dayVisuals != null) dayVisuals.SetActive(!isPrep);
        if (nightVisuals != null) nightVisuals.SetActive(isPrep);

        // Season GOs
        if (springVisuals != null) springVisuals.SetActive(currentSeason == Season.SPRING_SEASON);
        if (summerVisuals != null) summerVisuals.SetActive(currentSeason == Season.SUMMER_SEASON);
        if (autumnVisuals != null) autumnVisuals.SetActive(currentSeason == Season.AUTUMN_SEASON);
        if (winterVisuals != null) winterVisuals.SetActive(currentSeason == Season.WINTER_SEASON);
    }

    private void ApplyShaderSeasonKeywords()
    {
        Shader.DisableKeyword("_SEASON_SPRING");
        Shader.DisableKeyword("_SEASON_SUMMER");
        Shader.DisableKeyword("_SEASON_AUTUMN");
        Shader.DisableKeyword("_SEASON_WINTER");

        switch (currentSeason)
        {
            case Season.SPRING_SEASON: Shader.EnableKeyword("_SEASON_SPRING"); break;
            case Season.SUMMER_SEASON: Shader.EnableKeyword("_SEASON_SUMMER"); break;
            case Season.AUTUMN_SEASON: Shader.EnableKeyword("_SEASON_AUTUMN"); break;
            case Season.WINTER_SEASON: Shader.EnableKeyword("_SEASON_WINTER"); break;
        }
    }

    private IEnumerator ShowBannerRoutine(string message)
    {
        if (bannerText != null)
        {
            bannerText.gameObject.SetActive(true);
            bannerText.text = message;
            yield return new WaitForSeconds(3f);
            bannerText.gameObject.SetActive(false);
        }
    }

    // ==========================================
    // ECONOMY & PLAYER DEATH HOOKS
    // ==========================================

    public bool CanPlaceDefenses() => currentPhase == GamePhase.PrepPhase;

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UpdateCoinsUI();
            return true;
        }
        return false;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateCoinsUI();
    }

    public void SaveCheckpoint()
    {
        checkpointDay = currentDay;
        checkpointSeason = currentSeason;
        Debug.Log($"Checkpoint Saved: Day {checkpointDay} ({checkpointSeason})");
    }

    public void OnPlayerDeath(bool loadFromCheckpoint = true)
    {
        currentPhase = GamePhase.PlayerDied;

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        if (loadFromCheckpoint)
        {
            currentDay = checkpointDay;
            currentSeason = checkpointSeason;
            Debug.Log($"Player Died! Resetting to Checkpoint: Day {currentDay}");
        }
        else
        {
            currentDay = 1;
            currentSeason = Season.SPRING_SEASON;
            checkpointDay = 1;
            checkpointSeason = Season.SPRING_SEASON;
            coins = 100;
            Debug.Log("Player Died! Hard reset to Day 1.");
        }

        StartPrepPhase();
    }

    // ==========================================
    // UI MANAGEMENT & EFFECTS
    // ==========================================

    private void UpdateUI()
    {
        if (dayText != null) dayText.text = $"Day {currentDay}";
        if (seasonText != null) seasonText.text = $"{currentSeason}";
        if (phaseText != null) phaseText.text = currentPhase == GamePhase.PrepPhase ? "PREPARATION" : "SURVIVAL";

        if (prepTimerText != null) prepTimerText.gameObject.SetActive(currentPhase == GamePhase.PrepPhase);
        if (dayTimerText != null) dayTimerText.gameObject.SetActive(currentPhase == GamePhase.DayPhase);

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        TextMeshProUGUI activeTimerText = currentPhase == GamePhase.PrepPhase ? prepTimerText : dayTimerText;
        Vector3 originalScale = currentPhase == GamePhase.PrepPhase ? prepTimerOriginalScale : dayTimerOriginalScale;

        if (activeTimerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        activeTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Polish Effect: Low time pulse & warning color
        if (timeRemaining <= lowTimeThreshold && timeRemaining > 0)
        {
            activeTimerText.color = Color.Lerp(normalColor, warningColor, Mathf.PingPong(Time.time * pulseSpeed, 1f));
            float scaleMultiplier = 1f + (Mathf.Sin(Time.time * pulseSpeed * 2f) * 0.05f);
            activeTimerText.transform.localScale = originalScale * scaleMultiplier;
        }
        else
        {
            activeTimerText.color = normalColor;
            activeTimerText.transform.localScale = originalScale;
        }
    }

    private void UpdateCoinsUI()
    {
        if (coinsText != null) coinsText.text = $"Coins: {coins}";
    }

    private void TriggerPunchEffect(TextMeshProUGUI targetText, Vector3 baseScale)
    {
        if (targetText != null)
        {
            StartCoroutine(PunchScale(targetText, baseScale));
        }
    }

    private IEnumerator PunchScale(TextMeshProUGUI targetText, Vector3 baseScale)
    {
        Vector3 targetScale = baseScale * punchScaleAmount;
        float elapsed = 0f;

        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            targetText.transform.localScale = Vector3.Lerp(baseScale, targetScale, elapsed / (punchDuration / 2f));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            targetText.transform.localScale = Vector3.Lerp(targetScale, baseScale, elapsed / (punchDuration / 2f));
            yield return null;
        }

        targetText.transform.localScale = baseScale;
    }

    public DefenseDatabaseSO GetDatabase() => defenseDatabase;
    public int GetCurrentDay() => currentDay;

    // ==========================================
    // INSPECTOR CONTEXT MENU TEST BUTTONS
    // ==========================================

    [ContextMenu("Test: Set Time to 10 Seconds")]
    public void TestSetTimerTo10s()
    {
        timeRemaining = 10f;
    }

    [ContextMenu("Test: Skip Timer")]
    public void TestSkipTimer()
    {
        timeRemaining = 0;
    }

    [ContextMenu("Test: Skip 1 Day")]
    public void TestSkip1Day()
    {
        AdvanceDay(1);
    }

    [ContextMenu("Test: Skip 5 Days (+1 Season)")]
    public void TestSkip5Days()
    {
        AdvanceDay(5);
    }

    [ContextMenu("Test: Save Checkpoint")]
    public void TestSaveCheckpoint()
    {
        SaveCheckpoint();
    }

    [ContextMenu("Test: Player Die (Load Checkpoint)")]
    public void TestPlayerDieCheckpoint()
    {
        OnPlayerDeath(loadFromCheckpoint: true);
    }

    [ContextMenu("Test: Player Die (Reset to Day 1)")]
    public void TestPlayerDieHardReset()
    {
        OnPlayerDeath(loadFromCheckpoint: false);
    }
}