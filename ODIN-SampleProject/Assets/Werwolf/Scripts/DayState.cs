using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Werwolf.Scripts
{
    public class DayState : MonoBehaviourPun
    {
        [SerializeField] private float RequiredVotesPercentage = 0.51f;
        [SerializeField] private float FinalCountdownDuration = 5.0f;

        [SerializeField] private PlayerList players;
        [SerializeField] private VoteManager voteManager;

        [SerializeField] private Button votingButton;
        [SerializeField] private TMP_Text dayStatusDisplay;

        private Coroutine _CountDownRoutine;

        private void OnEnable()
        {
            if (PhotonNetwork.IsMasterClient)
                foreach (GameObject player in players.All)
                {
                    PhotonView playerView = player.GetComponent<PhotonView>();
                    playerView.RPC("SetCanSee", RpcTarget.All, true);
                }

            votingButton.gameObject.SetActive(players.IsLocalPlayerAlive());
            votingButton.onClick.AddListener(OnRequestedVoting);
        }

        private void OnDisable()
        {
            votingButton.onClick.RemoveListener(OnRequestedVoting);
            voteManager.OnVoteCriteriaMatched -= OnVoteCriteriaMatched;
        }

        private void OnRequestedVoting()
        {
            photonView.RPC("ReceiveDayVoteRequest", RpcTarget.All);
        }

        [PunRPC]
        private void ReceiveDayVoteRequest()
        {
            votingButton.gameObject.SetActive(false);
            voteManager.StartVote(true, RequiredVotesPercentage);
            voteManager.OnVoteCriteriaMatched += OnVoteCriteriaMatched;
        }

        private void OnVoteCriteriaMatched(VoteResultData resultData)
        {
            StopCoroutine(_CountDownRoutine);
            _CountDownRoutine = StartCoroutine(StartVoteCountdown(resultData));
        }

        private IEnumerator StartVoteCountdown(VoteResultData resultData)
        {
            float remainingCountDown = FinalCountdownDuration;

            if(PhotonNetwork.CurrentRoom.Players.TryGetValue(resultData.ActorNumber, out Player target)){
                while (remainingCountDown > 0.0f)
                {
                    dayStatusDisplay.text = $"Kicking {target.NickName} in {remainingCountDown.ToString("0")}";
                    remainingCountDown -= 1.0f;
                    yield return new WaitForSeconds(1.0f);
                }


                PhotonView playerView = players.GetPhotonViewByActorNumber(resultData.ActorNumber);
                if (playerView.ControllerActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    PhotonNetwork.Destroy(playerView);
                }
            }
        }


    }
}