using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class WerwolfStateMachine : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text stateDisplay;
    private readonly Dictionary<string, GameObject> states = new Dictionary<string, GameObject>();

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
        if (PhotonNetwork.IsMasterClient) photonView.RPC("ReceiveStateSwitch", RpcTarget.AllBuffered, nextState);
    }

    [PunRPC]
    private void ReceiveStateSwitch(string newState)
    {
        if (states.TryGetValue(newState, out GameObject foundState))
        {
            DeactivateAllStates();
            stateDisplay.text = newState;
            foundState.SetActive(true);
        }
    }
}