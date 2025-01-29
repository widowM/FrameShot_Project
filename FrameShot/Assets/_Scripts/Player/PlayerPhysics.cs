using UnityEngine;
using DG.Tweening;

public class PlayerPhysics : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] Transform[] groundCheckPoints;
    private LayerMask groundLayerMask;
    [SerializeField] float groundCheckrayLength = 1.0f;
    private bool isGrounded = false;
    private bool wasGroundedLastFrame;
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    [SerializeField] private float fallingGravityScale = 3.0f;
    [SerializeField] private float normalGravityScale = 1.0f;
    [SerializeField] private GameObject pieces;
    [SerializeField] private PhysicsMaterial2D playerPhysicsMaterial;
    [Header("Broadcast on Enent Channels")]
    [SerializeField] private VoidEventChannelSO cameraCollectedSO;
    [SerializeField] private VoidEventChannelSO playingSnapshotSound;
    [SerializeField] private VoidEventChannelSO wentOffLedgeSO;
    [SerializeField] private VoidEventChannelSO prepareLandingSoundSO;
    [SerializeField] private VoidEventChannelSO isGroundedSO;
    [SerializeField] private VoidEventChannelSO isNotGroundedSO;
    [SerializeField] private VoidEventChannelSO isFallingSO;
    [SerializeField] private VoidEventChannelSO playerDamagedSO;
    [SerializeField] private VoidEventChannelSO cameraRotationEnabledSO;
    [Header("Listen to Event Channels")]
    [SerializeField] private VoidEventChannelSO gameOverSO;
    [SerializeField] private VoidEventChannelSO goreStartedSO;
    public bool IsGrounded => isGrounded;
    public Rigidbody2D Rb2D => rb2D;
    public float CoyoteTime => coyoteTime;
    private void Start()
    {
        groundLayerMask = LayerMask.GetMask("Default");
        rb2D = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        isGrounded = CheckIfGrounded();
        if (isGrounded)
        {
            isGroundedSO.RaiseEvent();
        }
        else
        {
            isNotGroundedSO.RaiseEvent();
            prepareLandingSoundSO.RaiseEvent();

        }
        HandlePlayerGravity();
        if (!isGrounded)
        {
            prepareLandingSoundSO.RaiseEvent();
        }
    }
    private bool CheckIfGrounded()
    {
        bool isOnGround = false;

        foreach (Transform groundCheckPoint in groundCheckPoints)
        {
            // Draw debug ray in scene view
            Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckrayLength, Color.red);

            RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckrayLength, groundLayerMask);
            if (hit.collider != null)
            {
                isOnGround = true;
                break;
            }
        }

        // Start coyote time when player falls off platform
        if (wasGroundedLastFrame && !isOnGround && !player.PlayerMovement.HasJumped)
        {
            coyoteTimeCounter = coyoteTime;
            if (PlayerAnimation.ShownCoyoteIndicatorCounter < 5)
            {
                // Send message to PlayerAnimation to show
                wentOffLedgeSO.RaiseEvent();
            }
        }
        else if (isOnGround)
        {
            coyoteTimeCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        wasGroundedLastFrame = isOnGround;
        return isOnGround || (coyoteTimeCounter > 0 && !player.PlayerMovement.HasJumped);
    }

    private void HandlePlayerGravity()
    {
        if (rb2D.linearVelocity.y < 0 && !isGrounded)
        {
            rb2D.gravityScale = fallingGravityScale;
            isFallingSO.RaiseEvent();
        }
        else
        {
            rb2D.gravityScale = normalGravityScale;
        }
    }

    private void StopPhysicsSimulation()
    {
        rb2D.simulated = false;
    }

    private void EnablePlayerPieces()
    {
        pieces.SetActive(true);
    }

    private void PlayGoreExplosion()
    {
        Collider2D[] playerPieces = Physics2D.OverlapCircleAll(transform.position, 5f, LayerMask.GetMask("Pieces"));
            foreach (Collider2D piece in playerPieces)
            {
                Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
                Vector2 force = new Vector2(Random.Range(-5, 5), Random.Range(2, 10)); // Ensure y-component is positive
                rb.AddForce(force, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-2, 2), ForceMode2D.Impulse);
                piece.GetComponent<Collider2D>().enabled = false;
                piece.transform.DOScale(Random.Range(1, 6), Random.Range(1, 2));
            }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Hazard"))
        {
            // Send message to PlayerHealth to start
            // DeathSequenceCoroutine
            playerDamagedSO.RaiseEvent();
        }
        if (other.gameObject.CompareTag("Camera"))
        {
            other.gameObject.SetActive(false);
            // Send message to PlayerInput to switch to NormalWithCamera action map
            cameraCollectedSO.RaiseEvent();
            // Send message to PlayerAudio to play camera shutter sound effect
                    DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, 0.0001f).SetUpdate(true).OnComplete(() =>
            {
                DOVirtual.DelayedCall(2f, () =>
                {
                    DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1, 0.0001f).SetUpdate(true);
                }).SetUpdate(true);
            });

        }

        if (other.gameObject.CompareTag("CameraRot"))
        {
            other.gameObject.SetActive(false);
            cameraCollectedSO.RaiseEvent();
            // Send message to PlayerAudio to play camera shutter sound effect
                    DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, 0.0001f).SetUpdate(true).OnComplete(() =>
            {
                DOVirtual.DelayedCall(2f, () =>
                {
                    DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1, 0.0001f).SetUpdate(true);
                }).SetUpdate(true);
            });
            cameraRotationEnabledSO.RaiseEvent();
        }
    }

    private void OnEnable()
    {
        gameOverSO.OnEventRaised += StopPhysicsSimulation;
        gameOverSO.OnEventRaised += EnablePlayerPieces;
        goreStartedSO.OnEventRaised += PlayGoreExplosion;
    }

    private void OnDisable()
    {
        gameOverSO.OnEventRaised -= StopPhysicsSimulation;
        gameOverSO.OnEventRaised -= EnablePlayerPieces;
        goreStartedSO.OnEventRaised -= PlayGoreExplosion;
    }
}
