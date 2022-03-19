using Photon.Pun;
using UnityEngine;

namespace Werwolf.Scripts
{
    public class DayState : MonoBehaviour
    {
        [SerializeField] private PlayerList players;


        private void OnEnable()
        {
            if (PhotonNetwork.IsMasterClient)
                foreach (GameObject player in players.All)
                {
                    PhotonView photonView = player.GetComponent<PhotonView>();
                    photonView.RPC("SetCanSee", RpcTarget.All, true);
                }
        }
    }
}