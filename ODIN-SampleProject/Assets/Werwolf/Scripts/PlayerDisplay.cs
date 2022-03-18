using TMPro;
using UnityEngine;

namespace Werwolf.Scripts
{
    public class PlayerDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text roleDisplay;


        private void OnEnable()
        {
            roleDisplay.text = "";
        }

        public void ShowRole(string role)
        {
            roleDisplay.text = role;
        }
    }
}