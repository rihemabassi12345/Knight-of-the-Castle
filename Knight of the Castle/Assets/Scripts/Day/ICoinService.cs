public interface ICoinService
{
    int CurrentCoins { get; }
    bool HasEnoughCoins(int amount);
    bool SpendCoins(int amount);
    void AddCoins(int amount);
    void SaveCoins();
    void LoadCoins();
}