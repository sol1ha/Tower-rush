using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PressKeyToPlay : MonoBehaviour
{
    [Header("Auto-start timer")]
    [Tooltip("Seconds before the game auto-starts if no input is detected.")]
    public float autoStartSeconds = 30f;

    [Header("Optional countdown UI")]
    [Tooltip("Optional TMP text that shows the remaining seconds. Format: \"Auto-start: {0}s\".")]
    public TMP_Text countdownTmp;
    [Tooltip("Optional legacy Text that shows the remaining seconds.")]
    public Text countdownText;
    [Tooltip("Format string. {0} is replaced with the integer seconds remaining.")]
    public string countdownFormat = "Auto-start: {0}s";

    private Rigidbody2D rb;
    private float autoStartTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        autoStartTimer = autoStartSeconds;
        UpdateCountdownUi();
    }

    void Update()
    {
        autoStartTimer -= Time.deltaTime;
        UpdateCountdownUi();

        bool inputPressed = false;
        if (Keyboard.current != null)
            inputPressed = Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame;
        if (!inputPressed && Gamepad.current != null)
            inputPressed = Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame;

        if (inputPressed || autoStartTimer <= 0f)
        {
            HideCountdownUi();
            this.enabled = false;

            if (GameManager.instance != null)
                GameManager.StartGame();

            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 12f);
            }
        }
    }

    void UpdateCountdownUi()
    {
        int secondsLeft = Mathf.Max(0, Mathf.CeilToInt(autoStartTimer));
        string text = string.Format(countdownFormat, secondsLeft);
        if (countdownTmp != null) countdownTmp.text = text;
        if (countdownText != null) countdownText.text = text;
    }

    void HideCountdownUi()
    {
        if (countdownTmp != null) countdownTmp.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }
}
