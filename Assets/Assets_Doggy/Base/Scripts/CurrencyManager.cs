using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private long startingCredits = 1000;
    [SerializeField] private TextMeshProUGUI creditsText;

    private long credits;

    public long Credits => credits;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        credits = startingCredits;
        UpdateDisplay();
    }

    private void Update()
    {
        // Test-Tasten (später entfernen)
        if (Input.GetKeyDown(KeyCode.F1)) AddMoney(100);
        if (Input.GetKeyDown(KeyCode.F2)) SpendMoney(50);
    }

    public void AddMoney(long amount)
    {
        if (amount <= 0) return;
        credits += amount;
        UpdateDisplay();
    }

    public bool SpendMoney(long amount)
    {
        if (amount <= 0 || credits < amount) return false;
        credits -= amount;
        UpdateDisplay();
        return true;
    }

    public bool CanAfford(long amount) => credits >= amount;

    private void UpdateDisplay()
    {
        if (creditsText != null)
            creditsText.text = credits.ToString();
    }
}