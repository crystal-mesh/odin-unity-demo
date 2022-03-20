using ODIN_Sample.Scripts.Runtime.Odin;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Werwolf.Scripts
{
    public class WerwolfPlayer : MonoBehaviourPun
    {
        [SerializeField] private GameObject blindingCanvas;
        [SerializeField] private TMP_Text roleDisplay;

        [SerializeField] private Roles roles;
        [SerializeField] private PlayerList players;
        [SerializeField] private OdinStringVariable WerewolfVoiceRoom;
         public RoleTypes CurrentRole { get; set; }

        private void OnEnable()
        {
            roleDisplay.text = "";
            blindingCanvas.SetActive(false);
            // Player should add themselves to the list
            players.All.Add(this.gameObject);
        }

        private void OnDisable()
        {
            // Player should remove themselves from the list
            players.All.Remove(this.gameObject);
        }

        [PunRPC]
        public void SetCanSee(bool canSee)
        {
            if (photonView.IsMine)
                blindingCanvas.gameObject.SetActive(!canSee);
        }

        [PunRPC]
        public void SetRole(string role)
        {
            CurrentRole = roles.FromString(role);

            if (photonView.IsMine)
            {
                roleDisplay.text = role;
                if (RoleTypes.Werewolf == CurrentRole)
                    OdinHandler.Instance.JoinRoom(WerewolfVoiceRoom, new OdinSampleUserData());
                Debug.Log($"Current Role: {CurrentRole}");

            }

        }

        public void OnDestroy()
        {
            if (OdinHandler.Instance && photonView.IsMine)
            {
                OdinHandler.Instance.LeaveRoom(WerewolfVoiceRoom);
            }
        }
    }
}