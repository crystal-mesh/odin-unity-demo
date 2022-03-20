using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Werwolf.Scripts
{
    [CreateAssetMenu(fileName = "PlayerList", menuName = "Werewolf/PlayerList", order = 0)]
    public class PlayerList : ScriptableObject
    {
        private readonly List<GameObject> _all = new List<GameObject>();

        private GameObject _localPlayer;

        public List<GameObject> All
        {
            get
            {
                // Remove all destroyed players. Iterate backwards to avoid issues with jumping over objects
                for (int i = _all.Count - 1; i >= 0; i--)
                {
                    GameObject player = _all[i];
                    if (!player)
                        _all.RemoveAt(i);
                }

                return _all;
            }
        }

        public bool IsLocalPlayerAlive()
        {
            return IsAlive(PhotonNetwork.LocalPlayer.ActorNumber);
        }

        public bool IsAlive(int actorNumber)
        {
            return GetGameObjectByActorNumber(actorNumber);
        }

        public GameObject GetLocalPlayer()
        {
            if (!_localPlayer)
                foreach (GameObject player in All)
                {
                    PhotonView view = player.GetComponent<PhotonView>();
                    if (view.IsMine)
                    {
                        _localPlayer = player;
                        return player;
                    }
                }

            return _localPlayer;
        }

        public GameObject GetGameObjectByActorNumber(int actorNumber)
        {
            foreach (GameObject player in All)
            {
                PhotonView playerView = player.GetComponent<PhotonView>();
                if (playerView && playerView.ControllerActorNr == actorNumber)
                    return player;
            }

            return null;
        }

        public PhotonView GetPhotonViewByActorNumber(int actorNumber)
        {
            GameObject player = GetGameObjectByActorNumber(actorNumber);
            if (player)
            {
                return player.GetComponent<PhotonView>();
            }
            return null;
        }

        public int GetRoleCount(RoleTypes role)
        {
            int count = 0;
            foreach(GameObject playerObject in All)
            {
                WerwolfPlayer player = playerObject.GetComponent<WerwolfPlayer>();
                if (role == player.CurrentRole)
                    count++;
            }
            return count;
        }

        public bool IsGameOver()
        {
            bool isGameOver = false;

            int werewolveCount = GetRoleCount(RoleTypes.Werewolf);
            int totalPlayerCount = All.Count;

            // Game Over Conditions:
            if(werewolveCount >= Mathf.CeilToInt(totalPlayerCount / 2))
                isGameOver = true;
            
            if (werewolveCount == 0)
                isGameOver = true;

            return isGameOver;
        }
    }
}