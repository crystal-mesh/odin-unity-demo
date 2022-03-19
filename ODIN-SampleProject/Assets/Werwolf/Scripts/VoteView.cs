using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Werwolf.Scripts
{
    public class VoteView : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text voteCountDisplay;
        [SerializeField] private Toggle _toggle;

        private int _actorNumber = -1;

        public Action<int, bool> OnChangedVote;

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnChangedToggle);
        }

        private void OnChangedToggle(bool isVoted)
        {
            OnChangedVote?.Invoke(_actorNumber, isVoted);
        }

        public void Init(int actorNumber, ToggleGroup group)
        {
            _actorNumber = actorNumber;
            playerName.text = "";
            voteCountDisplay.text = "0";
            group.RegisterToggle(_toggle);
            _toggle.group = group;
        }

        public void SetToggleActive(bool toggleActive)
        {
            _toggle.interactable = toggleActive;
        }

        public void SetPlayerName(string newName)
        {
            playerName.text = newName;
        }

        public void SetVoteCount(int count)
        {
            voteCountDisplay.text = count.ToString();
        }
    }
}