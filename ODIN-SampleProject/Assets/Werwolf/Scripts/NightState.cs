using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace Werwolf.Scripts
{
    public class NightState : MonoBehaviour
    {
        [SerializeField] private WerwolfStateMachine stateMachine;
        [SerializeField] private VoteManager werewolfVoteManager;
        [SerializeField] private Canvas villagerDisplay;

        [SerializeField] private string nextState;

        [SerializeField] private PlayerList players;
        [SerializeField] private float nightDuration = 3.0f;

        private void OnEnable()
        {
            if (PhotonNetwork.IsMasterClient)
                foreach (GameObject player in players.all)
                {
                    PhotonView photonView = player.GetComponent<PhotonView>();
                    BasicPlayerBehaviour playerBehaviour = player.GetComponent<BasicPlayerBehaviour>();
                    if (playerBehaviour.CurrentRole == RoleTypes.Werewolf)
                        photonView.RPC("SetCanSee", RpcTarget.All, true);
                    else
                        photonView.RPC("SetCanSee", RpcTarget.All, false);
                }

            BasicPlayerBehaviour basicPlayerBehaviour = players.GetLocalPlayer().GetComponent<BasicPlayerBehaviour>();

            bool isWerewolf = basicPlayerBehaviour.CurrentRole == RoleTypes.Werewolf;
            werewolfVoteManager.ShowVoteCanvas(isWerewolf);
            villagerDisplay.gameObject.SetActive(!isWerewolf);

            StartCoroutine(WaitForEndOfNight());
        }

        private void OnDisable()
        {
            werewolfVoteManager.ShowVoteCanvas(false);
        }

        private IEnumerator WaitForEndOfNight()
        {
            yield return new WaitForSeconds(nightDuration);
            stateMachine.SwitchState(nextState);
        }
    }
}