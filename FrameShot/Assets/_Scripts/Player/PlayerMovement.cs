using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Player player;
    Vector2 playerMove;
    [SerializeField] float playerMoveSpeed = 10;
    [SerializeField] float jumpForceMultiplier = 500f;
    [SerializeField] float onAirHorizontalSpeedModifier = 0.6f;
    [SerializeField] float onGroundHorizontalSpeedModifier = 1;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsIdleHash = Animator.StringToHash("IsIdle");
    [Header("Broadcast on Event Channels")]
    [SerializeField] private VoidEventChannelSO movingRightSO;
    [SerializeField] private VoidEventChannelSO movingLeftSO;
    [SerializeField] private VoidEventChannelSO isMovingSO;
    [SerializeField] private VoidEventChannelSO isIdleSO;
    [SerializeField] private VoidEventChannelSO playerLandedSO;
    
    [Header("Listen to Event Channels")]
    [SerializeField] private VoidEventChannelSO playerJumpedSO;


    private bool hasJumped = false;

    public bool HasJumped => hasJumped;
    public Vector2 PlayerMove
    {
        get => playerMove;
        set => playerMove = value;
    }

    private void FixedUpdate()
    {
        
        if (player.PlayerPhysics.IsGrounded && !player.PlayerController.JumpPressed)
        {
            hasJumped = false;
        }
        if (player.PlayerPhysics.IsGrounded && 
            !player.PlayerAudio.PlayedLandingSound &&
             !player.PlayerAnimation.Animator.GetBool(IsWalkingHash) &&
             !player.PlayerAnimation.Animator.GetBool(IsIdleHash))
            {
                playerLandedSO.RaiseEvent();

            }
        HandlePlayerMovement();


    }

    private void Jump()
    {
        if (player.PlayerPhysics.IsGrounded && player.PlayerController.JumpPressed)
        {
            player.PlayerPhysics.Rb2D.AddForce(Vector2.up * jumpForceMultiplier);
            hasJumped = true;
        }
    }

    private void HandlePlayerMovement()
    {
        Vector2 playerMovement;
        if (player.PlayerPhysics.IsGrounded)
        {
            playerMovement = GetPlayerMovementVector(onGroundHorizontalSpeedModifier);
        }
        else
        {
            playerMovement = GetPlayerMovementVector(onAirHorizontalSpeedModifier);
        }
        player.PlayerPhysics.Rb2D.AddForce(playerMovement);

        // Update sprite direction based on movement
        if (playerMove.x > 0)
        {
            movingRightSO.RaiseEvent();
        }
        else if (playerMove.x < 0)
        {
            movingLeftSO.RaiseEvent();
        }
        else if (playerMove.x == 0 && player.PlayerPhysics.IsGrounded)
        {
            player.PlayerPhysics.Rb2D.linearVelocity = new Vector2(0, player.PlayerPhysics.Rb2D.linearVelocity.y);
        }

        if (playerMove.x != 0 && player.PlayerPhysics.IsGrounded)
        {
            isMovingSO.RaiseEvent();
        }
        else if (playerMove.x == 0 && player.PlayerPhysics.IsGrounded)
        {
            isIdleSO.RaiseEvent();
        }
    }

    private Vector2 GetPlayerMovementVector(float speedModifier)
    {
        //Vector2 playerMovement = new Vector2(playerMove.x, playerMove.y);
        Vector2 playerMovement = new Vector2(playerMove.x, 0).normalized;
        playerMovement *= playerMoveSpeed * speedModifier * Time.deltaTime;

        return playerMovement;
    }

    private void OnEnable()
    {
        playerJumpedSO.OnEventRaised += Jump;
    }

    private void OnDisable()
    {
        playerJumpedSO.OnEventRaised -= Jump;
    }
}
