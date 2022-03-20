using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Werwolf.Scripts
{
    public class DayState : MonoBehaviourPun
    {
        [Header("Settings")]
        [SerializeField] private float requiredVotesPercentage = 0.51f;
        [SerializeField] private float finalCountdownDuration = 5.0f;
        [SerializeField] private string nextState = "Night";

        [Header("References")]
        [SerializeField] private PlayerList players;
        [SerializeField] private VoteManager voteManager;
        [SerializeField] private WerwolfStateMachine stateMachine;
        [SerializeField] private Button votingButton;
        [SerializeField] private TMP_Text dayStatusDisplay;

        private Coroutine _CountDownRoutine;
        private bool hasCountdownStarted = false;

        private void OnEnable()
        {
            hasCountdownStarted = false;
            dayStatusDisplay.text = "";
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

            int requiredVotes = Mathf.CeilToInt(players.All.Count * requiredVotesPercentage);
            requiredVotes = Mathf.Min(players.All.Count, requiredVotes);

            bool canVote = players.IsLocalPlayerAlive();
            voteManager.StartVote(canVote, requiredVotes);
            Debug.Log($"Requiring {requiredVotes} Votes.");

            voteManager.OnVoteCriteriaMatched += OnVoteCriteriaMatched;
            voteManager.OnInvalidVote += OnReceivedInvalidVoteResult;
        }

        private void OnReceivedInvalidVoteResult()
        {
            if(null != _CountDownRoutine)
            {
                dayStatusDisplay.text = $"Stopped Countdown.";
                StopCoroutine(_CountDownRoutine);
                hasCountdownStarted = false;
            }
        }

        private void OnVoteCriteriaMatched(VoteResultData resultData)
        {
            if (!hasCountdownStarted)
            {
                hasCountdownStarted = true;
                _CountDownRoutine = StartCoroutine(StartVoteCountdown(resultData));
            }
        }

        private IEnumerator StartVoteCountdown(VoteResultData resultData)
        {
            float remainingCountDown = finalCountdownDuration;

            // Get player from photon network
            if(PhotonNetwork.CurrentRoom.Players.TryGetValue(resultData.ActorNumber, out Player target)){

                // perform countdown
                string targetName = target.NickName;
                while (remainingCountDown > 0.0f)
                {
                    dayStatusDisplay.text = $"{targetName} will be killed in {remainingCountDown.ToString("0")}";
                    remainingCountDown -= 1.0f;
                    yield return new WaitForSeconds(1.0f);
                }

                voteManager.SetVisibility(false);
                // if we get here, the countdown was not canceled, so kill target player
                GameObject playerObject = players.GetGameObjectByActorNumber(resultData.ActorNumber);
                if (playerObject)
                {
                    WerwolfPlayer player = playerObject.GetComponent<WerwolfPlayer>();
                    RoleTypes killedPlayerRole = player.CurrentRole;
                    dayStatusDisplay.text = $"You decided to kill {targetName}, who was a {killedPlayerRole}";

                    if (target.IsLocal)
                    {
                        PhotonView playerView = playerObject.GetComponent<PhotonView>();
                        PhotonNetwork.Destroy(playerView);
                    }
                }
                else
                {
                    dayStatusDisplay.text = $"The vote has ended, but nobody was killed.";
                }
            }

            yield return new WaitForSeconds(3.0f);
            stateMachine.SwitchState(nextState);
        }

    }
}