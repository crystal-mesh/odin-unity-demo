using System;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin.Room;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    public class OdinPushToTalk : MonoBehaviour
    {
        [SerializeField] private OdinPushToTalkData[] pushToTalkSettings;

        private void OnEnable()
        {
            OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
        }

        private void OnDisable()
        {
            OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
        }

        
        /// <summary>
        /// Mute the local microphone when first being added. This avoids e.g. the odin local voice indicator to
        /// show or for some data to be sent on accident.
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="mediaAddedEventArgs"></param>
        private void OnMediaAdded(object arg0, MediaAddedEventArgs mediaAddedEventArgs)
        {
            // check if the added media is one of the rooms for which the user has provided push to talk data
            foreach (OdinPushToTalkData pushToTalkData in pushToTalkSettings)
            {
                if (pushToTalkData.connectedRoom == mediaAddedEventArgs.Peer.RoomName)
                {
                    Room pushToTalkRoom = OdinHandler.Instance.Rooms[pushToTalkData.connectedRoom];
                    // if the local microphone is the one, for which OnMediaAdded was called
                    if (pushToTalkRoom.MicrophoneMedia.Id == mediaAddedEventArgs.Media.Id)
                    {
                        // mute the microphone initially
                        pushToTalkRoom.MicrophoneMedia.SetMute(true);
                    }
                }
            }
        }

        private void Update()
        {
            if (!(OdinHandler.Instance && null != OdinHandler.Instance.Rooms))
                return;
            
            foreach (OdinPushToTalkData pushToTalkData in pushToTalkSettings)
            {
                HandleRoomMutedStatus(pushToTalkData.connectedRoom,pushToTalkData.pushToTalkButton);
            }
        }

        /// <summary>
        /// Mutes / unmutes local microphone in the room given by <see cref="roomName"/> based on whether the button given
        /// by <see cref="pushToTalkButton"/> is pressed.
        /// </summary>
        /// <param name="roomName">Room to check</param>
        /// <param name="pushToTalkButton">Push to talk button for that room</param>
        private void HandleRoomMutedStatus(string roomName, string pushToTalkButton)
        {
            if (OdinHandler.Instance.Rooms.Contains(roomName))
            {
                Room roomToCheck = OdinHandler.Instance.Rooms[roomName];

                if (null != roomToCheck.MicrophoneMedia)
                {
                    bool isPushToTalkPressed = Input.GetButton(pushToTalkButton);
                    roomToCheck.MicrophoneMedia.SetMute(!isPushToTalkPressed);
                }
            }
        }
    }

    [Serializable]
    public class OdinPushToTalkData
    {
        /// <summary>
        /// The room for which the push to talk button should work
        /// </summary>
        public StringVariable connectedRoom;
        /// <summary>
        /// The push to talk button. If this is pressed, the microphone data
        /// will be transmitted in the room given by <see cref="connectedRoom"/>.
        /// </summary>
        public string pushToTalkButton;
    }
}
