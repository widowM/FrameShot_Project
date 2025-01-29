using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerControls playerControls;
    private bool isScreenshotButtonPressed = false;
    private bool jumpPressed = false;
    [SerializeField] private bool IsNotFirstLevel = false;
    private bool isRotationEnabled = false;

    [Header("Broadcast on Event Channels")]
    [SerializeField] private VoidEventChannelSO gamestartedSO;
    [SerializeField] private VoidEventChannelSO enterScreenshotModeSO;
    [SerializeField] private VoidEventChannelSO exitScreenshotModeSO;
    [SerializeField] private VoidEventChannelSO snapshotActionButtonPressedSO;
    [SerializeField] private VoidEventChannelSO onJumpPressedSO;
    [Header("Listen to Event Channels")]
    [SerializeField] private VoidEventChannelSO enableUIControlsSO;
    [SerializeField] private VoidEventChannelSO cameraCollectedSO;
    [SerializeField] private VoidEventChannelSO cameraRotationEnabledSO;


    public bool JumpPressed
    {
        get => jumpPressed;
        set => jumpPressed = value;
    }

    private void Awake()
    {
        playerControls = new PlayerControls();

        playerControls.NormalActions.PlayerMove.performed += ctx => player.PlayerMovement.PlayerMove = ctx.ReadValue<Vector2>();
        playerControls.NormalActions.PlayerMove.canceled += ctx => player.PlayerMovement.PlayerMove = Vector2.zero;
        playerControls.NormalActions.Jump.performed += ctx => OnJumpPressed();
        playerControls.NormalActions.Jump.canceled += ctx => jumpPressed = false;
        playerControls.NormalActions.RestartLevel.performed += ctx => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        playerControls.NormalActionsWithCamera.PlayerMoveC.performed += ctx => player.PlayerMovement.PlayerMove = ctx.ReadValue<Vector2>();
        playerControls.NormalActionsWithCamera.PlayerMoveC.canceled += ctx => player.PlayerMovement.PlayerMove = Vector2.zero;
        playerControls.NormalActionsWithCamera.JumpC.performed += ctx => OnJumpPressed();
        playerControls.NormalActionsWithCamera.JumpC.canceled += ctx => jumpPressed = false;
        playerControls.NormalActionsWithCamera.EnterScreenshotMode.performed += ctx => OnScreenshotButtonPressed();
        playerControls.NormalActionsWithCamera.EnterScreenshotMode.canceled += ctx => OnScreenshotButtonReleased();
        playerControls.NormalActionsWithCamera.RestartC.performed += ctx => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        playerControls.SnapshotModeActions.ExitScreenshotMode.performed += ctx => SwitchToNormalWithCameraActionMap();
        playerControls.SnapshotModeActions.SnapshotActionButton.performed += ctx => snapshotActionButtonPressedSO.RaiseEvent();
        playerControls.SnapshotModeActions.SnapshotFrameMove.performed += ctx => player.PlayerCamMechanicManager.FrameMove = ctx.ReadValue<Vector2>();
        playerControls.SnapshotModeActions.SnapshotFrameMove.canceled += ctx => player.PlayerCamMechanicManager.FrameMove = Vector2.zero;

        playerControls.UI.Start.performed += ctx => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        playerControls.UI.Disable();

        playerControls.SnapshotModeActionsWithRotation.ExitScreenshotMode.performed += ctx => SwitchToNormalWithCameraActionMap();
        playerControls.SnapshotModeActionsWithRotation.SnapshotActionButton.performed += ctx => snapshotActionButtonPressedSO.RaiseEvent();
        playerControls.SnapshotModeActionsWithRotation.RotateLeft.performed += ctx => player.PlayerCamMechanicManager.IsCopyRotateLeft = true;
        playerControls.SnapshotModeActionsWithRotation.RotateLeft.canceled += ctx => player.PlayerCamMechanicManager.IsCopyRotateLeft = false;
        playerControls.SnapshotModeActionsWithRotation.RotateRight.performed += ctx => player.PlayerCamMechanicManager.IsCopyRotateRight = true;
        playerControls.SnapshotModeActionsWithRotation.RotateRight.canceled += ctx => player.PlayerCamMechanicManager.IsCopyRotateRight = false;
        playerControls.SnapshotModeActionsWithRotation.SnapshotFrameMove.performed += ctx => player.PlayerCamMechanicManager.FrameMove = ctx.ReadValue<Vector2>();
        playerControls.SnapshotModeActionsWithRotation.SnapshotFrameMove.canceled += ctx => player.PlayerCamMechanicManager.FrameMove = Vector2.zero;

    }

    private void Start()
    {
        if (IsNotFirstLevel)
        {
            SwitchToNormalWithCameraActionMap();
        }
    }

    private void OnJumpPressed()
    {
        if (!jumpPressed)
        {
            jumpPressed = true;
            //Debug.Log("OnJumpPressedCalled");
            onJumpPressedSO.RaiseEvent();
        }


    }

    private void SwitchToNormalWithCameraActionMap()
    {
        playerControls.NormalActions.Disable();
        playerControls.SnapshotModeActions.Disable();
        playerControls.NormalActionsWithCamera.Enable();
        exitScreenshotModeSO.RaiseEvent();
    }

    private void SwitchToSnapshotActionMap()
    {
        playerControls.NormalActionsWithCamera.Disable();
        playerControls.SnapshotModeActions.Enable();
        if (isRotationEnabled)
        {
            playerControls.SnapshotModeActions.Disable();
            playerControls.SnapshotModeActionsWithRotation.Enable();

        }
    }

    private void OnScreenshotButtonPressed()
    {
        if (!isScreenshotButtonPressed && player.PlayerPhysics.IsGrounded)
        {
            isScreenshotButtonPressed = true;
            SwitchToSnapshotActionMap();
            enterScreenshotModeSO.RaiseEvent();

        }
    }

    private void OnScreenshotButtonReleased()
    {
        //Debug.Log("Inside OnScreenButtonReleased");
        if (isScreenshotButtonPressed)
        {
            //Debug.Log("Inside Inside OnScreenButtonReleased");
            isScreenshotButtonPressed = false;
            SwitchToNormalWithCameraActionMap();
            exitScreenshotModeSO.RaiseEvent();
        }
    }

    private void EnableUIControls()
    {
        //Debug.Log("Enabling UI controls...");
        playerControls.UI.Enable();
    }

    private void SetRotationEnabled()
    {
        isRotationEnabled = true;
    }

    private void OnEnable()
    {
        playerControls.NormalActions.Enable();
        playerControls.UI.Disable();
        playerControls.NormalActionsWithCamera.Disable();
        playerControls.SnapshotModeActions.Disable();
        playerControls.SnapshotModeActionsWithRotation.Disable();
        enableUIControlsSO.OnEventRaised += EnableUIControls;
        cameraCollectedSO.OnEventRaised += SwitchToNormalWithCameraActionMap;
        cameraRotationEnabledSO.OnEventRaised += SetRotationEnabled;
    }

    private void OnDisable()
    {
        playerControls.NormalActions.Disable();
        playerControls.UI.Disable();
        playerControls.NormalActionsWithCamera.Disable();
        playerControls.SnapshotModeActions.Disable();
        playerControls.SnapshotModeActionsWithRotation.Disable();
        enableUIControlsSO.OnEventRaised -= EnableUIControls;
        cameraCollectedSO.OnEventRaised -= SwitchToNormalWithCameraActionMap;
        cameraRotationEnabledSO.OnEventRaised -= SetRotationEnabled;
    }
}
