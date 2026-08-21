using UnityEngine;

public class CoinManager : MonoBehaviour, ICoinService
{
    public static CoinManager Instance { get; private set; }

    [SerializeField] private int startingCoins = 100;
    private const string COIN_SAVE_KEY = "PLAYER_COINS_SAVE";

    public int CurrentCoins { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadCoins();
    }

    public bool HasEnoughCoins(int amount) => CurrentCoins >= amount;

    public bool SpendCoins(int amount)
    {
        if (!HasEnoughCoins(amount)) return false;

        CurrentCoins -= amount;
        GameEvents.TriggerCoinsChanged(CurrentCoins);
        SaveCoins();
        return true;
    }

    public void AddCoins(int amount)
    {
        CurrentCoins += amount;
        GameEvents.TriggerCoinsChanged(CurrentCoins);
        SaveCoins();
    }

    public void SaveCoins()
    {
        PlayerPrefs.SetInt(COIN_SAVE_KEY, CurrentCoins);
        PlayerPrefs.Save();
    }

    public void LoadCoins()
    {
        CurrentCoins = PlayerPrefs.GetInt(COIN_SAVE_KEY, startingCoins);
        GameEvents.TriggerCoinsChanged(CurrentCoins);
    }

    [ContextMenu("Reset Saved Coins")]
    public void ResetCoins()
    {
        CurrentCoins = startingCoins;
        SaveCoins();
        GameEvents.TriggerCoinsChanged(CurrentCoins);
    }
}