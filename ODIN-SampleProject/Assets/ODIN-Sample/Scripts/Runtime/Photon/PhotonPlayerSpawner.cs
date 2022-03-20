using ODIN_Sample.Scripts.Runtime.Odin;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    ///     Spawns the player object on start, using the Photon Spawn method.
    /// </summary>
    public class PhotonPlayerSpawner : MonoBehaviour
    {
        [SerializeField] protected OdinStringVariable playerName;

        /// <summary>
        ///     Prefab of the player object. Has to be located in a resources folder! (Photon requirement)
        /// </summary>
        [SerializeField] protected GameObject playerPrefab;

        /// <summary>
        ///     The location at which we should spawn the player.
        /// </summary>
        [SerializeField] private Vector3 spawnLocation;

        private GameObject _instantiatedPlayer;

        private void Awake()
        {
            Assert.IsNotNull(playerPrefab);
        }

        protected virtual void Start()
        {
            playerName.Load();
            PhotonNetwork.NickName = playerName;
            InstantiatePlayer();
        }

        protected void InstantiatePlayer()
        {
            if (null != playerPrefab && PhotonNetwork.IsConnectedAndReady)
            {
                // Try to adjust spawn location, if we find a collision. Doesn't work very well when using
                // Unity's CharacterController script as collider.
                Vector3 adjustedSpawnLocation = spawnLocation;
                Collider playerCollider = playerPrefab.GetComponent<Collider>();
                if (playerCollider)
                {
                    Bounds playerBounds = playerCollider.bounds;
                    bool hitSomething = Physics.BoxCast(spawnLocation, playerBounds.extents, Vector3.down);
                    if (hitSomething) adjustedSpawnLocation.y = adjustedSpawnLocation.y + 2.0f;
                }

                // instantiate player using the Photon synchronised instantiation method.
                _instantiatedPlayer =
                    PhotonNetwork.Instantiate(playerPrefab.name, adjustedSpawnLocation, Quaternion.identity);
            }
        }
    }
}