using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Kills the player if no input is detected for a configurable duration after the game starts.
/// Resets the timer on any keyboard / gamepad / mouse activity.
/// </summary>
public class AfkKiller : MonoBehaviour
{
    [Header("AFK Settings")]
    [Tooltip("Seconds without any input before the player is killed. Default: 60s.")]
    public float afkTimeoutSeconds = 60f;

    [Header("Optional warning UI")]
    [Tooltip("Optional TMP text showing the AFK countdown when it's about to fire.")]
    public TMP_Text warningTmp;
    [Tooltip("Optional legacy Text showing the AFK countdown when it's about to fire.")]
    public Text warningText;
    [Tooltip("Show the warning when this many seconds remain.")]
    public float showWarningWhenRemaining = 10f;
    [Tooltip("Format string. {0} is replaced with the integer seconds remaining.")]
    public string warningFormat = "AFK in {0}s";

    private float timeSinceLastInput;
    private bool dead;

    void Start()
    {
        timeSinceLastInput = 0f;
        HideWarning();
    }

    void Update()
    {
        if (dead) return;

        // Only count AFK time while the game is actually playing.
        if (!GameManager.Playing())
        {
            timeSinceLastInput = 0f;
            HideWarning();
            return;
        }

        if (AnyInputThisFrame())
        {
            timeSinceLastInput = 0f;
            HideWarning();
            return;
        }

        timeSinceLastInput += Time.deltaTime;
        float remaining = afkTimeoutSeconds - timeSinceLastInput;

        if (remaining <= showWarningWhenRemaining)
            ShowWarning(Mathf.Max(0, Mathf.CeilToInt(remaining)));
        else
            HideWarning();

        if (timeSinceLastInput >= afkTimeoutSeconds)
        {
            dead = true;
            HideWarning();
            PlayerKillLimit.TriggerEventStatic();
        }
    }

    bool AnyInputThisFrame()
    {
        // Keyboard: any key activity at all.
        var kb = Keyboard.current;
        if (kb != null && kb.anyKey.isPressed) return true;

        // Mouse: any button or movement.
        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.isPressed || mouse.rightButton.isPressed || mouse.middleButton.isPressed)
                return true;
            Vector2 delta = mouse.delta.ReadValue();
            if (delta.sqrMagnitude > 0.01f) return true;
        }

        // Gamepad: any button or stick movement.
        var gp = Gamepad.current;
        if (gp != null)
        {
            if (gp.leftStick.ReadValue().sqrMagnitude > 0.04f) return true;
            if (gp.rightStick.ReadValue().sqrMagnitude > 0.04f) return true;
            if (gp.buttonSouth.isPressed || gp.buttonNorth.isPressed
                || gp.buttonEast.isPressed || gp.buttonWest.isPressed
                || gp.startButton.isPressed || gp.selectButton.isPressed
                || gp.leftTrigger.ReadValue() > 0.1f || gp.rightTrigger.ReadValue() > 0.1f)
                return true;
        }

        return false;
    }

    void ShowWarning(int secondsLeft)
    {
        string text = string.Format(warningFormat, secondsLeft);
        if (warningTmp != null)
        {
            warningTmp.gameObject.SetActive(true);
            warningTmp.text = text;
        }
        if (warningText != null)
        {
            warningText.gameObject.SetActive(true);
            warningText.text = text;
        }
    }

    void HideWarning()
    {
        if (warningTmp != null) warningTmp.gameObject.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
    }
}
