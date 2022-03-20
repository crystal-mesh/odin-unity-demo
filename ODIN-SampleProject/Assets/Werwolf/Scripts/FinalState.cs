using ODIN_Sample.Scripts.Runtime.Odin;
using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Werwolf.Scripts
{
    public class FinalState : MonoBehaviour
    {
        [SerializeField] private float finalScreenDuration = 5.0f;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private PlayerList players;
        [SerializeField] private OdinStringVariable restartScene;

        private void OnEnable()
        {
            int werewolfCount = players.GetRoleCount(RoleTypes.Werewolf);
            if (werewolfCount > 0)
                statusText.text = "Game Over - Werewolves won!\n <size=50%> (Won't they starve now?)";
            else
            {
                statusText.text = "Game Over - Villagers won!\n Good job, angry mob.";
            }
            PhotonNetwork.AutomaticallySyncScene = true;
            StartCoroutine(Countdown());
        }

        private IEnumerator Countdown()
        {
            yield return new WaitForSeconds(finalScreenDuration);
            if (PhotonNetwork.IsMasterClient)
            {
                // Restart Level
                PhotonNetwork.LoadLevel(restartScene);
            }
        }
    }
}