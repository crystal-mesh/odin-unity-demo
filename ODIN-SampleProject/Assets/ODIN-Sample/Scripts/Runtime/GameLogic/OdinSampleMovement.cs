﻿using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Base movement behavior for both the first and third person movement behavior. Inherit from this to implement
    /// your own movement behavior.
    /// </summary>
    public abstract class OdinSampleMovement : MonoBehaviour
    {
        /// <summary>
        /// The name of the axis used for left/right movement.
        /// </summary>
        [Header("Input")]
        [SerializeField] protected string horizontalMovement = "Horizontal";
        /// <summary>
        /// The name of the axis used for forward/backwards movement.
        /// </summary>
        [SerializeField] protected string verticalMovement = "Vertical";
        /// <summary>
        /// The name of the button used to activate sprint.
        /// </summary>
        [SerializeField] protected string sprintButton = "Sprint";
        
        /// <summary>
        /// The base movement speed.
        /// </summary>
        [Header("Values")]
        [SerializeField] protected float movementSpeed = 10.0f;
        /// <summary>
        /// The multiplier applied to the base movement speed, when the button <see cref="sprintButton"/> is pressed.
        /// </summary>
        [SerializeField] protected float sprintMultiplier = 1.5f;
        
        /// <summary>
        /// Reference to the Unity <c>CharacterController</c> script, which is used to move around.
        /// </summary>
        [Header("References")]
        [SerializeField] protected CharacterController characterController = null;

        protected float CurrentSprintMultiplier;
        protected Vector3 PlayerInput;
        
        protected virtual void Awake()
        {
            Assert.IsNotNull(characterController);
        }

        protected virtual void Update()
        {
            UpdateInput();
            // don't multiply with Time.deltaTime, it's already included in characterController.SimpleMove()
            Vector3 deltaMovement = (PlayerInput.x * transform.right + PlayerInput.z * transform.forward) * movementSpeed *
                                    CurrentSprintMultiplier;
            characterController.SimpleMove(deltaMovement);
        }

        protected virtual void UpdateInput()
        {
            float horizontal = Input.GetAxis(horizontalMovement);
            float vertical = Input.GetAxis(verticalMovement);

            CurrentSprintMultiplier = Input.GetButton(sprintButton) ? sprintMultiplier : 1.0f;
            PlayerInput = new Vector3(horizontal, 0.0f, vertical);
        }
    }
}