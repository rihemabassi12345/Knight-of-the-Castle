using UnityEngine;
using TMPro;

public class CoinUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsText;

    private void OnEnable()
    {
        GameEvents.OnCoinsChanged += UpdateCoinDisplay;

        // Push initial value if service is ready
        if (CoinManager.Instance != null)
        {
            UpdateCoinDisplay(CoinManager.Instance.CurrentCoins);
        }
    }

    private void OnDisable()
    {
        GameEvents.OnCoinsChanged -= UpdateCoinDisplay;
    }

    private void UpdateCoinDisplay(int newAmount)
    {
        if (coinsText != null)
        {
            coinsText.text = $"Coins: {newAmount}";
        }
    }
}