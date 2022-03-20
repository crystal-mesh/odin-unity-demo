using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Werwolf.Scripts
{
    public class WerwolfStateMachine : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_Text stateDisplay;
        private readonly Dictionary<string, GameObject> states = new Dictionary<string, GameObject>();

        public Action<string, string> OnSwitchedState;

        private string _CurrentState;

        public string CurrentState { get => _CurrentState; }

        private void Awake()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                GameObject state = transform.GetChild(i).gameObject;
                states.Add(state.name, state);
            }

            DeactivateAllStates();

            string firstStateName = transform.GetChild(0).gameObject.name;
            SwitchState(firstStateName);
        }


        private void DeactivateAllStates()
        {
            foreach (GameObject value in states.Values) value.SetActive(false);
        }

        public void SwitchState(string nextState)
        {
            
            // only let the master client actually change the game state
            if (PhotonNetwork.IsMasterClient) photonView.RPC("ReceiveStateSwitch", RpcTarget.All, nextState);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient) photonView.RPC("ReceiveStateSwitch", newPlayer, CurrentState);
        }


        [PunRPC]
        private void ReceiveStateSwitch(string newState)
        {
            if (states.TryGetValue(newState, out GameObject foundState))
            {
                

                DeactivateAllStates();
                stateDisplay.text = "State: " + newState;
                foundState.SetActive(true);

                string oldState = _CurrentState;
                _CurrentState = newState;

                OnSwitchedState?.Invoke(oldState, _CurrentState);
            }
        }
    }
}