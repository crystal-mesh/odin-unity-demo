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
        [SerializeField] private string waitingForMorePlayersState = "Countdown";
        [SerializeField] private string finalState;

        private void OnEnable()
        {
            stateMachine.OnSwitchedState += OnSwitchedState;
            StartCoroutine(RegularGameOverChecks());
        }

        private void OnDisable()
        {
            stateMachine.OnSwitchedState -= OnSwitchedState;
        }

        private IEnumerator RegularGameOverChecks()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(1.0f);
                TryEndGame();
            }
        }


        private void OnSwitchedState(string oldState, string newState)
        {
            TryEndGame();
        }

        private void TryEndGame()
        {
            if(stateMachine.CurrentState != waitingForMorePlayersState)
            {
                bool isGameOver = players.IsGameOver();
                if (isGameOver)
                {
                    stateMachine.OnSwitchedState -= OnSwitchedState;
                    stateMachine.SwitchState(finalState);
                    StopAllCoroutines();
                }
            }
        }
    }
}