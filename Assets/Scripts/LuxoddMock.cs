using System;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network
{
    public enum SessionOptionAction
    {
        Continue,
        End,
        Restart
    }

    public class WebSocketService : MonoBehaviour
    {
        private Action<SessionOptionAction> _storedAction;

        public void SendSessionOptionContinue(Action<SessionOptionAction> callback)
        {
            Debug.Log("[Mock SDK] Pretending to show a $2 Continue Popup right now! It will automatically choose 'Continue' in 3 seconds.");
            _storedAction = callback;
            Invoke(nameof(SimulateContinue), 3f);
        }

        private void SimulateContinue()
        {
            _storedAction?.Invoke(SessionOptionAction.Continue);
        }

        public void SendSessionOptionRestart(Action<SessionOptionAction> callback)
        {
            Debug.Log("[Mock SDK] Showed Restart Popup.");
            callback?.Invoke(SessionOptionAction.End);
        }

        public void BackToSystem()
        {
            Debug.Log("[Mock SDK] Pretending to return you to the main system dashboard.");
        }
    }

    public class WebSocketCommandHandler : MonoBehaviour
    {
        public void SendLevelEndRequestCommand(Action onComplete)
        {
            Debug.Log("[Mock SDK] Sending your score to the server now...");
            onComplete?.Invoke();
        }
    }
}
