using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Werwolf.Scripts
{
    public class CountDownState : MonoBehaviour
    {
        [SerializeField] private string nextState = "RollenAuswaehlen";
        [SerializeField] private TMP_Text countDownDisplay;
        [SerializeField] private int minNumPlayers = 4;
        [SerializeField] private int countDown = 3;


        private int Counter;


        private WerwolfStateMachine werwolfStateMachine;

        private void OnEnable()
        {
            werwolfStateMachine = GetComponentInParent<WerwolfStateMachine>();
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
                    countDownDisplay.text = "Not Enough Players, waiting for more";
                    yield return new WaitForSeconds(1.0f);
                    currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                }

                StartCoroutine(CountDown());
            }
        }

        private IEnumerator CountDown()
        {
            Counter = countDown;
            while (Counter > 0)
            {
                countDownDisplay.text = Counter.ToString();
                Counter--;
                yield return new WaitForSeconds(1.0f);
            }

            werwolfStateMachine.SwitchState(nextState);
        }
    }
}