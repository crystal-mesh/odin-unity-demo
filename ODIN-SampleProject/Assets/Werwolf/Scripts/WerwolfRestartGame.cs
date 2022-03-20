using ODIN_Sample.Scripts.Runtime.Odin;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Assets.Werwolf.Scripts
{
    public class WerwolfRestartGame : MonoBehaviour
    {
        [SerializeField] OdinStringVariable targetScene;
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1.0f);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.DestroyAll();
                PhotonNetwork.LoadLevel(targetScene);
            }
        }
    }
}