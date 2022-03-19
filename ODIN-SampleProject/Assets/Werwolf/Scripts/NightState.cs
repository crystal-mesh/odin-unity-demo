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

            WerwolfPlayer basicPlayerBehaviour = players.GetLocalPlayer().GetComponent<WerwolfPlayer>();

            bool isWerewolf = basicPlayerBehaviour.CurrentRole == RoleTypes.Werewolf;
            voteManager.StartVote(isWerewolf, 1.0f, RoleTypes.Werewolf);
            voteManager.OnVoteCriteriaMatched += OnVoteEnded;
            villagerDisplay.gameObject.SetActive(!isWerewolf);

            _NightCountdown = StartCoroutine(NightCountdown());
        }

        private void OnDisable()
        {
            voteManager.SetVisibility(false);
            voteManager.OnVoteCriteriaMatched -= OnVoteEnded;
        }

        private void OnVoteEnded(VoteResultData result)
        {
            StopCoroutine(_NightCountdown);
            if (PhotonNetwork.CurrentRoom.Players.TryGetValue(result.ActorNumber, out Player kickedPlayer))
            {
                nightDisplay.text = $"Player {kickedPlayer.NickName} was eaten...";
                if (kickedPlayer.IsLocal)
                {
                    GameObject kickedPlayerObject = players.GetByPhotonActorNumber(kickedPlayer.ActorNumber);
                    PhotonNetwork.Destroy(kickedPlayerObject);
                }
            }
            else
            {
                nightDisplay.text =
                    "The Vote ended, nobody was killed. Let's move on.";
            }

            voteManager.SetVisibility(false);
            StartCoroutine(DelayedSwitchState(4.0f));
        }

        private IEnumerator DelayedSwitchState(float duration)
        {
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