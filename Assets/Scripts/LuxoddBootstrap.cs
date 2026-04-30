using System;
using System.Collections.Generic;
using UnityEngine;
using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.CommandHandler;
using Luxodd.Game.Scripts.Game.Leaderboard;

/// <summary>
/// Single-entry bridge into the Luxodd Unity plugin.
///
/// Responsibilities:
///  - Persist across scene loads (DontDestroyOnLoad).
///  - Connect to the WebSocket on first Start, activate the heartbeat.
///  - Send a level_begin when the game starts and level_end when it ends.
///  - Provide cached player profile + leaderboard data the rest of the game can
///    read at any time (LeaderboardUI uses it to show real Luxodd rankings).
///  - Refresh leaderboard data periodically and on-demand around game-over.
///
/// Drag the plugin's UnityPluginPrefab into the scene first; this script will
/// auto-find the WebSocketService / WebSocketCommandHandler / HealthStatusCheck
/// service references at runtime if they aren't wired in the inspector.
/// </summary>
public class LuxoddBootstrap : MonoBehaviour
{
    public static LuxoddBootstrap Instance { get; private set; }

    [Header("Plugin references — auto-found if left empty")]
    [SerializeField] private WebSocketService _webSocketService;
    [SerializeField] private WebSocketCommandHandler _commandHandler;
    [SerializeField] private HealthStatusCheckService _healthStatusCheck;

    [Header("Behavior")]
    [Tooltip("If true, attempts to connect to the WebSocket server on Start.")]
    public bool autoConnect = true;
    [Tooltip("Also attempt to connect when running inside the Unity Editor. The plugin really only works in WebGL builds where the dev token is passed in the URL, so leave this OFF to silence 'WebSocket Closed' spam during in-editor playtesting.")]
    public bool connectInEditor = false;
    [Tooltip("Activate the periodic health-check ping after a successful connect.")]
    public bool autoActivateHealthCheck = true;
    [Tooltip("How often (seconds) to refresh the leaderboard data while running.")]
    public float leaderboardRefreshInterval = 30f;

    public string PlayerName { get; private set; } = "Player";
    public int Balance { get; private set; }
    public bool IsConnected { get; private set; }

    public class Entry
    {
        public int rank;
        public string playerName;
        public int score;
    }
    public List<Entry> Leaderboard { get; private set; } = new List<Entry>();
    public Entry CurrentUserLeaderboard { get; private set; }

    public event Action OnLeaderboardUpdated;
    public event Action OnConnected;

    private float _nextLeaderboardRefresh;
    private bool _hasSentLevelBegin;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        ResolveRefs();
    }

    void Start()
    {
        if (!autoConnect) return;
#if UNITY_EDITOR
        if (!connectInEditor)
        {
            Debug.Log("[Luxodd] In-editor connect skipped (connectInEditor is OFF). Build to WebGL and append '?token=<dev_token>' to the URL to test the real plugin.");
            return;
        }
#endif
        Connect();
    }

    void Update()
    {
        if (IsConnected && Time.unscaledTime >= _nextLeaderboardRefresh)
        {
            _nextLeaderboardRefresh = Time.unscaledTime + leaderboardRefreshInterval;
            RefreshLeaderboard();
        }
    }

    void ResolveRefs()
    {
        if (_webSocketService == null)
            _webSocketService = FindAnyObjectByType<WebSocketService>(FindObjectsInactive.Include);
        if (_commandHandler == null)
            _commandHandler = FindAnyObjectByType<WebSocketCommandHandler>(FindObjectsInactive.Include);
        if (_healthStatusCheck == null)
            _healthStatusCheck = FindAnyObjectByType<HealthStatusCheckService>(FindObjectsInactive.Include);
    }

    public void Connect()
    {
        ResolveRefs();
        if (_webSocketService == null)
        {
            Debug.LogWarning("[Luxodd] WebSocketService not found — drag UnityPluginPrefab into your starting scene.");
            return;
        }

        _webSocketService.ConnectToServer(
            () =>
            {
                IsConnected = true;
                Debug.Log("[Luxodd] Connected.");
                OnConnected?.Invoke();
                if (autoActivateHealthCheck && _healthStatusCheck != null) _healthStatusCheck.Activate();
                RequestProfile();
                RefreshLeaderboard();
            },
            () =>
            {
                IsConnected = false;
                Debug.LogError("[Luxodd] Connect failed.");
            });
    }

    public void RequestProfile()
    {
        if (_commandHandler == null) return;
        _commandHandler.SendProfileRequestCommand(
            handle =>
            {
                PlayerName = string.IsNullOrEmpty(handle) ? "Player" : handle;
                if (LeaderboardManager.Instance != null)
                    LeaderboardManager.Instance.SetCurrentPlayer(PlayerName);
                Debug.Log($"[Luxodd] Player handle: {PlayerName}");
            },
            (code, msg) => Debug.LogWarning($"[Luxodd] Profile request failed: {code} {msg}"));
    }

    public void RefreshLeaderboard()
    {
        if (_commandHandler == null) return;
        _commandHandler.SendLeaderboardRequestCommand(
            response =>
            {
                Leaderboard.Clear();
                if (response != null)
                {
                    if (response.CurrentUserData != null)
                    {
                        CurrentUserLeaderboard = new Entry
                        {
                            rank = response.CurrentUserData.Rank,
                            playerName = response.CurrentUserData.PlayerName,
                            score = response.CurrentUserData.TotalScore
                        };
                    }
                    if (response.Leaderboard != null)
                    {
                        foreach (var l in response.Leaderboard)
                        {
                            Leaderboard.Add(new Entry
                            {
                                rank = l.Rank,
                                playerName = l.PlayerName,
                                score = l.TotalScore
                            });
                        }
                    }
                }
                OnLeaderboardUpdated?.Invoke();
            },
            (code, msg) => Debug.LogWarning($"[Luxodd] Leaderboard request failed: {code} {msg}"));
    }

    public void NotifyLevelBegin(int level = 1)
    {
        if (_commandHandler == null) return;
        _hasSentLevelBegin = true;
        _commandHandler.SendLevelBeginRequestCommand(level,
            () => Debug.Log("[Luxodd] level_begin acknowledged."),
            (code, msg) => Debug.LogWarning($"[Luxodd] level_begin failed: {code} {msg}"));
    }

    /// <summary>
    /// Called from PlayerHealth when the player runs out of lives. Sends the
    /// final level_end, refreshes the leaderboard, then asks
    /// InGameTransactionController to show the Continue/Restart popup.
    /// </summary>
    public void NotifyLevelEnd(int level, int score)
    {
        if (_commandHandler == null) return;
        _commandHandler.SendLevelEndRequestCommand(level, score,
            () =>
            {
                Debug.Log("[Luxodd] level_end acknowledged.");
                RefreshLeaderboard();
            },
            (code, msg) => Debug.LogWarning($"[Luxodd] level_end failed: {code} {msg}"));
    }
}
