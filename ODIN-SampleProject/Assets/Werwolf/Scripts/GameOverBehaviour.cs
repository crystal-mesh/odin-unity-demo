using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

namespace Werwolf.Scripts
{
    public class GameOverBehaviour : MonoBehaviourPun
    {
        [SerializeField] private PlayerList players;
        [SerializeField] private WerwolfStateMachine stateMachine;
        [SerializeField] private string finalState;

        private void OnEnable()
        {
            stateMachine.OnSwitchedState += OnSwitchedState;
        }

        private void OnDisable()
        {
            stateMachine.OnSwitchedState -= OnSwitchedState;
        }

        private void OnSwitchedState(string oldState, string newState)
        {
            bool isGameOver = players.IsGameOver();
            if (isGameOver)
            {
                stateMachine.OnSwitchedState -= OnSwitchedState;
                stateMachine.SwitchState(finalState);
            }
        }

    }
}