using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Werwolf.Scripts
{
    public class NightState : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private string nextState;

        [SerializeField] private float nightDuration = 3.0f;


        [Header("References")] [SerializeField]
        private WerwolfStateMachine stateMachine;

        [FormerlySerializedAs("werewolfVoteManager")] [SerializeField]
        private VoteManager voteManager;

        [SerializeField] private RectTransform villagerDisplay;

        [FormerlySerializedAs("countDownDisplay")] [SerializeField]
        private TMP_Text nightDisplay;

        [SerializeField] private PlayerList players;

        private Coroutine _NightCountdown;

        private void OnEnable()
        {
            // All but the werewolve should be blinded
            if (PhotonNetwork.IsMasterClient)
                foreach (GameObject player in players.All)
                {
                    PhotonView photonView = player.GetComponent<PhotonView>();
                    WerwolfPlayer playerBehaviour = player.GetComponent<WerwolfPlayer>();

                    // only werewolves are allowed to see at night currently
                    if (playerBehaviour.CurrentRole == RoleTypes.Werewolf)
                        photonView.RPC("SetCanSee", RpcTarget.All, true);
                    else
                        photonView.RPC("SetCanSee", RpcTarget.All, false);
                }

            GameObject localPlayer = players.GetLocalPlayer();
            bool canSeeVote = players.IsLocalPlayerAlive();
            if (localPlayer)
            {
                WerwolfPlayer basicPlayerBehaviour = players.GetLocalPlayer().GetComponent<WerwolfPlayer>();
                // Only show the vote to the local player, if they are a werewolf. 
                canSeeVote = basicPlayerBehaviour.CurrentRole == RoleTypes.Werewolf;
            }

            // all werewolves have to select the same target
            int werewolfCount = players.GetRoleCount(RoleTypes.Werewolf);
            
            voteManager.StartVote(canSeeVote, werewolfCount, RoleTypes.Werewolf);
            voteManager.OnVoteCriteriaMatched += OnVoteEnded;

            villagerDisplay.gameObject.SetActive(!canSeeVote);

            _NightCountdown = StartCoroutine(NightCountdown());
        }

        private void OnDisable()
        {
            if (PhotonNetwork.IsConnectedAndReady &&  PhotonNetwork.IsMasterClient)
                foreach (GameObject player in players.All)
                {
                    PhotonView photonView = player.GetComponent<PhotonView>();
                    photonView.RPC("SetCanSee", RpcTarget.All, true);
                }

            voteManager.SetVisibility(false);
            voteManager.OnVoteCriteriaMatched -= OnVoteEnded;
        }

        private void OnVoteEnded(VoteResultData result)
        {
            StopCoroutine(_NightCountdown);
            if (PhotonNetwork.CurrentRoom.Players.TryGetValue(result.ActorNumber, out Player kickedPlayer))
            {
                nightDisplay.text = $"Player {kickedPlayer.NickName} was eaten...";
                PhotonView killedPlayerView = players.GetPhotonViewByActorNumber(kickedPlayer.ActorNumber);
                if (kickedPlayer.IsLocal)
                {
                    PhotonNetwork.Destroy(killedPlayerView);
                }
            }
            else
            {
                nightDisplay.text =
                    "The Vote ended, nobody was killed. Let's move on.";
            }

            StartCoroutine(DelayedSwitchState(4.0f));
        }

        private IEnumerator DelayedSwitchState(float duration)
        {
            voteManager.SetVisibility(false);
            yield return new WaitForSeconds(duration);
            stateMachine.SwitchState(nextState);
        }

        private IEnumerator NightCountdown()
        {
            float remainingTime = nightDuration;
            while (remainingTime > 0)
            {
                nightDisplay.text = "Remaining Time: " + (int)remainingTime;
                yield return new WaitForSeconds(1.0f);
                remainingTime -= 1.0f;
            }

            nightDisplay.text = "Nobody was killed... Difficult to decide who to eat, eh?";
            StartCoroutine(DelayedSwitchState(4.0f));
        }
    }
}