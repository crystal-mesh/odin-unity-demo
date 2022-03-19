using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Werwolf.Scripts
{
    public class CountDownState : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private string nextState = "RollenAuswaehlen";

        [SerializeField] private int minNumPlayers = 4;
        [SerializeField] private int countDown = 3;
        [SerializeField] private string notEnoughPlayersMessage = "Not Enough Players, waiting for more";

        [Header("References")] [SerializeField]
        private TMP_Text countDownDisplay;


        private int _counter;
        private WerwolfStateMachine _werwolfStateMachine;

        private void OnEnable()
        {
            _werwolfStateMachine = GetComponentInParent<WerwolfStateMachine>();
            StartCoroutine(CheckNumPlayer());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator CheckNumPlayer()
        {
            if (PhotonNetwork.IsConnected)
            {
                var currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                while (currentRoomPlayerCount < minNumPlayers)
                {
                    countDownDisplay.text = notEnoughPlayersMessage;
                    yield return new WaitForSeconds(1.0f);
                    currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                }

                StartCoroutine(CountDown());
            }
        }

        private IEnumerator CountDown()
        {
            _counter = countDown;
            while (_counter > 0)
            {
                countDownDisplay.text = _counter.ToString();
                _counter--;
                yield return new WaitForSeconds(1.0f);
            }

            _werwolfStateMachine.SwitchState(nextState);
        }
    }
}