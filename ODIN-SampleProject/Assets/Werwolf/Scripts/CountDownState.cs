using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Werwolf.Scripts
{
    public class CountDownState : MonoBehaviourPun, IOnEventCallback
    {
        [Header("Settings")] [SerializeField] private string nextState = "RollenAuswaehlen";

        [SerializeField] private int minNumPlayers = 4;
        [SerializeField] private int countDown = 3;
        [SerializeField] private string notEnoughPlayersMessage = "Not Enough Players, waiting for {0} more";

        [Header("References")] [SerializeField]
        private TMP_Text countDownDisplay;
        [SerializeField] private Button startButton;

        private int _counter;
        private WerwolfStateMachine _werwolfStateMachine;


        public const byte StartGameEventCode = 1;

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
            countDownDisplay.text = "";
            _werwolfStateMachine = GetComponentInParent<WerwolfStateMachine>();
            startButton.gameObject.SetActive(false);
            startButton.onClick.AddListener(OnClickedStartButton);
            StartCoroutine(CheckNumPlayer());
        }

        

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            StopAllCoroutines();
        }

        private IEnumerator CheckNumPlayer()
        {
            if (PhotonNetwork.IsConnected)
            {
                startButton.gameObject.SetActive(false);
                var currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                while (currentRoomPlayerCount < minNumPlayers)
                {
                    countDownDisplay.text = string.Format(notEnoughPlayersMessage, minNumPlayers - currentRoomPlayerCount);
                    yield return new WaitForSeconds(1.0f);
                    currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                }
                countDownDisplay.text = "";
                startButton.gameObject.SetActive(true);
            }
        }

        private void OnClickedStartButton()
        {
            PhotonNetwork.RaiseEvent(StartGameEventCode, null, new RaiseEventOptions() { CachingOption = EventCaching.DoNotCache, Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == StartGameEventCode)
            {
                StartCoroutine(CountDown());
            }
        }

        private IEnumerator CountDown()
        {
            startButton.gameObject.SetActive(false);
            _counter = countDown;
            while (_counter > 0)
            {
                countDownDisplay.text = _counter.ToString();
                _counter--;
                yield return new WaitForSeconds(1.0f);
                var currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                if (currentRoomPlayerCount < minNumPlayers)
                {
                    StartCoroutine(CheckNumPlayer());
                    yield break;
                }
            }

            _werwolfStateMachine.SwitchState(nextState);
        }

        
    }
}