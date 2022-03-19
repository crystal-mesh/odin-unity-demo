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
        [HideInInspector] public RoleTypes CurrentRole;


        private void OnEnable()
        {
            roleDisplay.text = "";
            blindingCanvas.SetActive(false);
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