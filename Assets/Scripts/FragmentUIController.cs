using UnityEngine;
using TMPro;

/// <summary>
/// Simple UI Controller to display the live Data Fragments balance.
/// </summary>
public class FragmentUIController : MonoBehaviour
{
    [Tooltip("TextMeshPro UI element to display current fragments")]
    public TextMeshProUGUI fragmentText;

    private void Start()
    {
        if (EconomyManager.Instance != null)
        {
            // Subscribe to live balance changes
            EconomyManager.Instance.OnBalanceChanged += UpdateUI;
            
            // Set initial value immediately
            UpdateUI(EconomyManager.Instance.GetBalance());
        }
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBalanceChanged -= UpdateUI;
        }
    }

    private void UpdateUI(int balance)
    {
        if (fragmentText != null)
            fragmentText.text = $"Fragments: {balance}";
    }
}
