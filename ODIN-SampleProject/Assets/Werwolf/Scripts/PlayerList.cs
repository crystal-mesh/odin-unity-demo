using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Werwolf.Scripts
{
    [CreateAssetMenu(fileName = "PlayerList", menuName = "Werewolf/PlayerList", order = 0)]
    public class PlayerList : ScriptableObject
    {
        public List<GameObject> all = new List<GameObject>();


        private GameObject _localPlayer;

        public GameObject GetLocalPlayer()
        {
            if (!_localPlayer)
                foreach (GameObject player in all)
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
    }
}