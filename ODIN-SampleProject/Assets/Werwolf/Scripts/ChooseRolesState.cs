using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Werwolf.Scripts
{
    public class ChooseRolesState : MonoBehaviour
    {
        [SerializeField] private PlayerList players;
        [SerializeField] private Roles roles;
        [SerializeField] private int minWerwolfCount = 1;

        [SerializeField] private string nextState;


        [SerializeField] private WerwolfStateMachine stateMachine;

        private void OnEnable()
        {
            if (PhotonNetwork.IsConnected) StartCoroutine(WaitForPlayersToSpawn());
        }


        private IEnumerator WaitForPlayersToSpawn()
        {
            int currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;

            GameObject[] playerArray = GameObject.FindGameObjectsWithTag("Player");
            while (playerArray.Length != currentRoomPlayerCount)
            {
                yield return new WaitForSeconds(0.25f);
                playerArray = GameObject.FindGameObjectsWithTag("Player");
                currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            }

            int werwolfNum = currentRoomPlayerCount / 3;
            werwolfNum = Mathf.Max(minWerwolfCount, werwolfNum);

            players.all.Clear();
            players.all.AddRange(playerArray);

            List<GameObject> villagers = new List<GameObject>(playerArray);
            for (int i = 0; i < werwolfNum; i++)
            {
                int werwolfIndex = Random.Range(0, villagers.Count);
                GameObject werwolf = villagers[werwolfIndex];

                SendRole(werwolf, RoleTypes.Werewolf);

                villagers.RemoveAt(werwolfIndex);
            }

            foreach (GameObject villager in villagers) SendRole(villager, RoleTypes.Villager);

            stateMachine.SwitchState(nextState);
        }

        private void SendRole(GameObject target, RoleTypes roleType)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonView photonView = target.GetComponent<PhotonView>();
                if (photonView)
                    photonView.RPC("SetRole", RpcTarget.All, roles.ToString(roleType));
            }
        }
    }
}