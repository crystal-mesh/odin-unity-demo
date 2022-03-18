using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Werwolf.Scripts
{
    public class VoteManager : MonoBehaviourPun
    {
        [SerializeField] private RectTransform playerListRoot;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private VoteView voteViewPrefab;

        private readonly Dictionary<int, VoteData> voteDataDictionary = new Dictionary<int, VoteData>();

        private void OnEnable()
        {
            ShowVoteCanvas(false);
        }

        public void ShowVoteCanvas(bool show)
        {
            foreach (Transform child in playerListRoot.transform) Destroy(child.gameObject);
            foreach (Transform child in transform) child.gameObject.SetActive(show);

            if (!PhotonNetwork.IsConnected)
                return;

            voteDataDictionary.Clear();
            foreach (var currentRoomPlayer in PhotonNetwork.CurrentRoom.Players)
            {
                string playerName = currentRoomPlayer.Value.NickName;
                VoteView voteView = Instantiate(voteViewPrefab, playerListRoot);

                var actorNumber = currentRoomPlayer.Value.ActorNumber;
                voteView.Init(actorNumber, toggleGroup);
                voteView.OnChangedVote += OnChangedVote;
                voteView.SetPlayerName(playerName);
                voteDataDictionary.Add(actorNumber, new VoteData { View = voteView });
            }

            toggleGroup.EnsureValidState();
        }

        private void OnChangedVote(int actorNumber, bool newActive)
        {
            photonView.RPC("ReceiveChangedVoteCount", RpcTarget.All, actorNumber, newActive ? 1 : -1);
        }

        [PunRPC]
        private void ReceiveChangedVoteCount(int actorNumber, int difference)
        {
            if (voteDataDictionary.TryGetValue(actorNumber, out VoteData voteData))
            {
                voteData.Count += difference;
                voteData.View.SetVoteCount(voteData.Count);
                Debug.Log($"ReceiveChangedVoteCount: {actorNumber} voted: {voteData.Count}");
            }
        }

        public class VoteData
        {
            public int Count;
            public VoteView View;
        }
    }
}