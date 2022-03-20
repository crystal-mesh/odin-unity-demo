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
            if (photonView.IsMine)
                roleDisplay.text = role;
            CurrentRole = roles.FromString(role);
            Debug.Log($"Current Role: {CurrentRole}");
        }
    }
}