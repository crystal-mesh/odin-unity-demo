using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Werwolf.Scripts
{
    public class VoteManager : MonoBehaviourPun
    {
        [SerializeField] private PlayerList players;

        [SerializeField] private RectTransform playerListRoot;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private VoteView voteViewPrefab;

        private readonly Dictionary<int, VoteData> voteDataDictionary = new Dictionary<int, VoteData>();

        public Action<VoteResultData> OnVoteEnded;

        public int TotalVotes { get; private set; }
        public int NumPossibleVotes { get; private set; }

        private void Reset()
        {
            foreach (Transform child in playerListRoot.transform) Destroy(child.gameObject);
            voteDataDictionary.Clear();
            TotalVotes = 0;
            NumPossibleVotes = 0;
        }

        private void OnEnable()
        {
            SetVisibility(false);
        }

        public void SetVisibility(bool isVisible)
        {
            foreach (Transform child in transform) child.gameObject.SetActive(isVisible);
        }

        public void StartVote(bool isVisible)
        {
            Reset();
            SetVisibility(isVisible);

            if (!PhotonNetwork.IsConnected)
                return;

            foreach (GameObject player in players.All)
            {
                PhotonView playerView = player.GetComponent<PhotonView>();

                Player photonPlayer = playerView.Controller;

                VoteView voteView = Instantiate(voteViewPrefab, playerListRoot);
                voteView.OnChangedVote += OnChangedVote;

                // Make sure we can connect the vote UI to the correct actor by using the actor number
                int actorNumber = photonPlayer.ActorNumber;
                voteView.Init(actorNumber, toggleGroup);

                // Ensure the player can't vote for her/him/themselves
                bool isLocalPlayer = actorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
                voteView.SetToggleActive(!isLocalPlayer);
                if (!isLocalPlayer)
                    NumPossibleVotes++;

                // Show player name
                string playerName = photonPlayer.NickName;
                voteView.SetPlayerName(playerName);

                // Add To Dictionary
                voteDataDictionary.Add(actorNumber, new VoteData { View = voteView });
            }
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
                TotalVotes += difference;
                Debug.Log(
                    $"ReceiveChangedVoteCount: {actorNumber} voted: {voteData.Count}, total Votes: {TotalVotes}");

                TryEndVote();
            }
        }

        /// <summary>
        ///     Only let the Master Client decide, if the vote has ended, to remove issues with synchronization when counting
        ///     votes.
        ///     Notify all clients via RPC that Vote has Ended.
        /// </summary>
        private void TryEndVote()
        {
            if (PhotonNetwork.IsMasterClient)
                foreach (KeyValuePair<int, VoteData> voteEntry in voteDataDictionary)
                {
                    VoteData voteData = voteEntry.Value;
                    if (voteData.Count >= NumPossibleVotes)
                        photonView.RPC("ReceivedVoteResult", RpcTarget.All, voteData.Count, voteEntry.Key);
                }
        }

        [PunRPC]
        private void ReceivedVoteResult(int count, int actorNumber)
        {
            OnVoteEnded?.Invoke(new VoteResultData
            {
                Count = count, ActorNumber = actorNumber
            });
        }

        public class VoteData
        {
            public int Count;
            public VoteView View;
        }
    }

    public class VoteResultData
    {
        public int ActorNumber;
        public int Count;
    }
}