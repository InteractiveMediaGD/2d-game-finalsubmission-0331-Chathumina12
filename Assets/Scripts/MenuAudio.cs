using UnityEngine;

/// <summary>
/// Attach to the MainMenuController's GameObject.
/// Boots the AudioManager and handles all menu sound events.
/// </summary>
[RequireComponent(typeof(MainMenuController))]
public class MenuAudio : MonoBehaviour
{
    private void Start()
    {
        // Ensure AudioManager exists (creates it if this is the first scene)
        if (AudioManager.Instance == null)
        {
            var go = new GameObject("[AudioManager]");
            go.AddComponent<AudioManager>();
        }

        // Start calm ambient menu music
        AudioManager.PlayMusic("music_menu");

        // Wire button hover/click sounds to every Button in the scene
        WireAllButtons();
    }

    private void WireAllButtons()
    {
        UnityEngine.UI.Button[] buttons =
            FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);

        foreach (var btn in buttons)
        {
            // Capture loop variable for lambdas
            var captured = btn;

            // Hover — uses EventTrigger
            var trigger = captured.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
                trigger = captured.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            // PointerEnter → hover sound
            var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            enterEntry.callback.AddListener(_ => AudioManager.Play("btn_hover"));
            trigger.triggers.Add(enterEntry);

            // Click → click sound (add before any navigation so it fires first)
            captured.onClick.AddListener(() => AudioManager.Play("btn_click"));
        }
    }

    // ─── Called by MainMenuController buttons via the same Unity hooks ────────
    // These are separate methods so the Inspector can wire them explicitly too.

    public void OnDifficultyOpened()  => AudioManager.Play("btn_click");
    public void OnDifficultyChosen()  => AudioManager.Play("difficulty_select");
    public void OnBackPressed()       => AudioManager.Play("menu_back");
}
