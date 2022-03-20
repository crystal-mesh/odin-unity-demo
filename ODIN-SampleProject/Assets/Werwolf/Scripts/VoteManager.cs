using System;
using System.Collections.Generic;
using System.Linq;
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

        private float _requiredVotesPercentage = 1.0f;

        public Action<VoteResultData> OnVoteCriteriaMatched;
        // Gets called, when the vote data was updated, but the vote criteria was not yet reached.
        public Action OnInvalidVote;

        public int TotalVotes { get; private set; }
        public int NumPossibleVotes { get; private set; }

        public float RequiredVotesPercentage
        {
            get => _requiredVotesPercentage;
            private set => _requiredVotesPercentage = Mathf.Clamp01(value);
        }

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

        public void StartVote(bool isVisible, float requiredVotesPercentage = 1.0f, params RoleTypes[] excludedRoles)
        {
            Reset();
            SetVisibility(isVisible);
            RequiredVotesPercentage = requiredVotesPercentage;

            if (!PhotonNetwork.IsConnected || !players.IsLocalPlayerAlive())
                return;

            foreach (GameObject player in players.All)
            {
                WerwolfPlayer werwolfPlayer = player.GetComponent<WerwolfPlayer>();
                RoleTypes role = werwolfPlayer.CurrentRole;
                // allow votes, if the player has one of the voted roles or if
                bool canVoteOnPlayer = !excludedRoles.Contains(role);

                if (canVoteOnPlayer) CreateVoteView(player);
            }
        }

        private void CreateVoteView(GameObject player)
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

            // Show player name
            string playerName = photonPlayer.NickName;
            voteView.SetPlayerName(playerName);

            // Add To Dictionary
            NumPossibleVotes++;
            voteDataDictionary.Add(actorNumber, new VoteData { View = voteView, ActorNumber = actorNumber, Count = 0 });
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
                TotalVotes += difference;
                if (voteData.View)
                    voteData.View.SetVoteCount(voteData.Count);

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
            {
                VoteData validVote = null;
                foreach (KeyValuePair<int, VoteData> voteEntry in voteDataDictionary)
                {
                    VoteData voteData = voteEntry.Value;
                    if (HasRequiredVoteCount(voteData))
                    {
                        validVote = voteData;
                        break;
                    }
                }

                if (null != validVote)
                    photonView.RPC("ReceivedVoteResult", RpcTarget.All, validVote.Count, validVote.ActorNumber);
                else
                    photonView.RPC("ReceivedInvalidVote", RpcTarget.All);
            }

        }

        private bool HasRequiredVoteCount(VoteData voteData)
        {
            return (float)voteData.Count / NumPossibleVotes >= RequiredVotesPercentage;
        }

        /// <summary>
        ///     Receiving the actor with the highest number of votes
        /// </summary>
        /// <param name="count"></param>
        /// <param name="actorNumber"></param>
        [PunRPC]
        private void ReceivedVoteResult(int count, int actorNumber)
        {
            Debug.Log($"Received Vote Result: {actorNumber} was voted with {count} Votes");
            OnVoteCriteriaMatched?.Invoke(new VoteResultData
            {
                Count = count,
                ActorNumber = actorNumber
            });
        }

        [PunRPC]
        private void ReceivedInvalidVote()
        {
            OnInvalidVote?.Invoke();
        }

        public class VoteData
        {
            public int ActorNumber;
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