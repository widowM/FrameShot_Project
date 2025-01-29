using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class PlayerCamMechanicManager : MonoBehaviour
{
    [SerializeField] private Player player;
    bool isScreenshotMode;
    [SerializeField] private PlayerCamMechanicCore snapshotCore;
    [SerializeField] private BoxCollider2D objToScreenshot;
    [SerializeField] private Transform frameTransform;
    Vector2 frameMove;
    private bool isCopyRotateLeft;
    private bool isCopyRotateRight;
    bool canRotate = false;

    private float timeSinceLastRotation = 0f;
    private float rotationInterval = 0.2f;
    private float rotationAmount = 90f;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private int maxScreenshots = 5;
    [SerializeField] private bool showMaxLimitWarning = true;
    private bool haveCopy;
    private GameObject previewObject;
    private List<GameObject> placedCopies = new List<GameObject>();
    [SerializeField] private float frameMoveInterval = 0.5f;
    [SerializeField] private float frameMoveUnitSteps = 1;
    private float timeSinceLastFrameMove = 0f;
    [SerializeField] private Image flashImage;
    [SerializeField] private TextMeshProUGUI remainingScreenshotsText;
    [Header("Listen to Event Channels")]
    [SerializeField] private VoidEventChannelSO enterScreenshotModeSO;
    [SerializeField] private VoidEventChannelSO exitScreenshotModeSO;
    [SerializeField] private VoidEventChannelSO snapshotActionButtonPressedSO;
    [Header("Broadcast on Event Channels")]
    [SerializeField] private VoidEventChannelSO frameMovedSO;
    [SerializeField] private VoidEventChannelSO snapshotTakenSO;
    [SerializeField] private VoidEventChannelSO snapshotNegatedSO;
    [SerializeField] private VoidEventChannelSO snapshotPlacedSO;


    public Vector2 FrameMove
    {
        get => frameMove;
        set => frameMove = value;
    }

    public bool IsCopyRotateLeft
    {
        get => isCopyRotateLeft;
        set => isCopyRotateLeft = value;
    }

    public bool IsCopyRotateRight
    {
        get => isCopyRotateRight;
        set => isCopyRotateRight = value;
    }

    private void Start()
    {
        remainingScreenshotsText.text = maxScreenshots.ToString();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (snapshotCore == null)
        {
            snapshotCore = GetComponent<PlayerCamMechanicCore>();
        }

        if (objToScreenshot == null)
        {
            objToScreenshot = snapshotCore.ObjToScreenshot;
        }
    }

    private void EnterScreenshotMode()
    {
        frameTransform.gameObject.SetActive(true);
        isScreenshotMode = true;

        // Set initial position to screen center
        Vector2 centerPos = GetScreenCenter();
        frameTransform.position = new Vector3(centerPos.x, centerPos.y, frameTransform.position.z);
    }

    private void ExitScreenshotMode()
    {
        frameTransform.gameObject.SetActive(false);
        isScreenshotMode = false;
    }

    private Vector2 GetScreenCenter()
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        return mainCamera.ScreenToWorldPoint(screenCenter);
    }

    Vector2[] GetBoxColliderCorners(BoxCollider2D collider)
    {
        // Get the center and size of the BoxCollider2D in world space
        Vector2 center = collider.bounds.center;
        Vector2 size = collider.bounds.size;

        // Calculate the world corners of the BoxCollider2D
        Vector2 topLeft = center + new Vector2(-size.x / 2, size.y / 2);
        Vector2 topRight = center + new Vector2(size.x / 2, size.y / 2);
        Vector2 bottomLeft = center + new Vector2(-size.x / 2, -size.y / 2);
        Vector2 bottomRight = center + new Vector2(size.x / 2, -size.y / 2);

        return new Vector2[] { topLeft, topRight, bottomLeft, bottomRight };
    }

    void Update()
    {
        if (player.PlayerHealthCondition.HasDied) return;

        if (isScreenshotMode)
        {
            // Get the world corners of the BoxCollider2D
            Vector2[] corners = GetBoxColliderCorners(objToScreenshot);

            // Iterate through each corner and check the distance from screen bounds
            foreach (Vector2 corner in corners)
            {
                // Convert world position to screen position
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(corner);

                // Check if the corner is near the bounds of the screen
                if (screenPoint.x <= 0 || screenPoint.x >= Screen.width || screenPoint.y <= 0 || screenPoint.y >= Screen.height)
                {
                    canRotate = false;
                }
                else
                {
                    canRotate = true;
                }
            }
            HandleFrameMovement();
            HandleRotateAction();
        }
        else
        {
            ResetCopyMode();
        }
    }

    private void HandleFrameMovement()
    {
        timeSinceLastFrameMove += Time.deltaTime;

        if (timeSinceLastFrameMove >= frameMoveInterval)
        {
            Vector3 newPosition = frameTransform.position;
            if (frameMove != Vector2.zero)
            {
                Vector3 movement = new Vector3(frameMove.x, frameMove.y, 0).normalized * 1f; // Move by 1 unit
                newPosition += movement * frameMoveUnitSteps;
                frameMovedSO.RaiseEvent();
            }

            frameTransform.position = ClampPositionToScreen(newPosition);

            timeSinceLastFrameMove = 0f;
        }
    }

    void ResetCopyMode()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        // Only destroy the preview copy if we haven't placed it yet
        if (snapshotCore.copy != null && haveCopy)
        {
            Destroy(snapshotCore.copy);
            snapshotCore.copy = null;
        }
        haveCopy = false;
    }

    private Vector3 ClampPositionToScreen(Vector3 position)
    {
        // Get screen bounds
        Vector2 bottomLeft = mainCamera.ScreenToWorldPoint(Vector2.zero);
        Vector2 topRight = mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        // Get frame bounds (assuming it has a BoxCollider2D)
        Vector2 frameHalfSize = objToScreenshot.bounds.extents;

        // Clamp position
        float x = Mathf.Clamp(position.x,
            bottomLeft.x + frameHalfSize.x,
            topRight.x - frameHalfSize.x);
        float y = Mathf.Clamp(position.y,
            bottomLeft.y + frameHalfSize.y,
            topRight.y - frameHalfSize.y);

        return new Vector3(x, y, position.z);
    }

    void HandleRotateAction()
{
    if (!haveCopy)
    {
        frameTransform.transform.rotation = Quaternion.identity;
        return;
    }

    timeSinceLastRotation += Time.deltaTime;

    if (timeSinceLastRotation >= rotationInterval && canRotate)
    {
        if (isCopyRotateLeft)
        {
            frameTransform.transform.Rotate(0, 0, rotationAmount);
            //frameMovedSO.RaiseEvent();
            timeSinceLastRotation = 0f;
        }
        else if (isCopyRotateRight)
        {
            frameTransform.transform.Rotate(0, 0, -rotationAmount);
            //frameMovedSO.RaiseEvent();
            timeSinceLastRotation = 0f;
        }
    }
}
    private void HandleSnapshotButtonPress()
    {
        if (!isScreenshotMode) return;

        if (!haveCopy)
        {
            if (placedCopies.Count >= maxScreenshots)
            {
                if (showMaxLimitWarning)
                {
                    snapshotNegatedSO.RaiseEvent();
                    //Debug.LogWarning($"Maximum number of screenshots ({maxScreenshots}) reached!");
                }
                return;
            }
            StartCoroutine(snapshotCore.TakeSnapShotAndSave());
            haveCopy = true;
            // Message PlayerAudio to play snapshot sound effect
            snapshotTakenSO.RaiseEvent();
            //audioSource.PlayOneShot(cameraShutterSFX, 2);

            Color32 c = new Color32(245, 229, 191, 0);
            flashImage.color = c;
            flashImage.DOFade(1, 0.1f).SetUpdate(true).OnComplete(() => flashImage.DOFade(0, 0.1f).SetUpdate(true)).SetEase(Ease.Linear);
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, 0.0001f).SetUpdate(true).OnComplete(() =>
                {
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1, 0.0001f).SetUpdate(true);
                    }).SetUpdate(true);
                });
        }
        else
        {
            // Check for collisions using OverlapCollider
            Transform copy = snapshotCore.copy.transform;
            Collider2D[] copyColliders = copy.GetComponentsInChildren<Collider2D>();
            if (copyColliders.Length > 0)
            {
                ContactFilter2D filter = new ContactFilter2D();
                filter.useTriggers = false;
                filter.SetLayerMask(~LayerMask.GetMask("Capture"));
                Collider2D[] results = new Collider2D[10];
                foreach (Collider2D copyCollider in copyColliders)
                {
                    int numColliders = copyCollider.Overlap(filter, results);
                    if (numColliders > 0)
                    {
                        // Message PlayerAudio to play snapshot negation sound effect
                        snapshotNegatedSO.RaiseEvent();
                        // if (audioSource.isPlaying) return;
                        // audioSource.PlayOneShot(negatePlaceCopySFX, 0.3f);
                        //shakeCameraSO.RaiseEvent();
                        //Debug.Log("Cannot place copy - overlapping with other objects!");
                        return;
                    }
                }
            }
            Color32 c = new Color32(62, 62, 62, 0);
            flashImage.color = c;
            flashImage.DOFade(1, 0.1f).SetUpdate(true).OnComplete(() => flashImage.DOFade(0, 0.1f).SetUpdate(true)).SetEase(Ease.Linear);
            // Message PlayerAudio to play paste sound effect
            snapshotPlacedSO.RaiseEvent();
            //audioSource.PlayOneShot(earthRumbleSFX, 1f);

            // If no collisions, proceed with placing the copy
            copy.rotation = frameTransform.rotation;
            copy.SetParent(null);
            SetLayerRecursively(copy.gameObject, 0);

            var sr = copy.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color co = sr.color;
                co.a = 1f;
                sr.color = co;
                sr.sprite = snapshotCore.screenShotSprite;
            }

            placedCopies.Add(copy.gameObject);

            previewObject = null;
            haveCopy = false;
            snapshotCore.copy = null;
            int remainingScreenshots = GetRemainingScreenshots();
            remainingScreenshotsText.text = remainingScreenshots.ToString();
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, 0.0001f).SetUpdate(true).OnComplete(() =>
                {
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1, 0.0001f).SetUpdate(true);
                    }).SetUpdate(true);
                });
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public int GetRemainingScreenshots()
    {
        return maxScreenshots - placedCopies.Count;
    }

    private void OnEnable()
    {
        enterScreenshotModeSO.OnEventRaised += EnterScreenshotMode;
        exitScreenshotModeSO.OnEventRaised += ExitScreenshotMode;
        snapshotActionButtonPressedSO.OnEventRaised += HandleSnapshotButtonPress;
    }

    private void OnDisable()
    {
        enterScreenshotModeSO.OnEventRaised -= EnterScreenshotMode;
        exitScreenshotModeSO.OnEventRaised -= ExitScreenshotMode;
        snapshotActionButtonPressedSO.OnEventRaised -= HandleSnapshotButtonPress;
    }
}
