using ODIN_Sample.Scripts.Runtime.Photon;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

namespace Werwolf.Scripts
{
    /// <summary>
    /// Only let the player spawn, if we're in the correct state
    /// </summary>
    public class WerwolfPlayerSpawner : PhotonPlayerSpawner
    {
        // Player will only be able to spawn, if we're in this state, otherwise they'll have to wait until next round.
        [SerializeField] private string spawnState;
        [SerializeField] private WerwolfStateMachine stateMachine;

        protected override void Start()
        {
            playerName.Load();
            PhotonNetwork.NickName = playerName;
            if (stateMachine.CurrentState == spawnState)
                InstantiatePlayer();
            else
                stateMachine.OnSwitchedState += OnSwitchedState;
        }

        private void OnSwitchedState(string oldState, string newState)
        {
            if (stateMachine.CurrentState == spawnState)
            {
                InstantiatePlayer();
                stateMachine.OnSwitchedState -= OnSwitchedState;
            }
        }
    }
}