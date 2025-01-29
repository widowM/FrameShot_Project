using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private Button startButton;
    [SerializeField] private RectTransform logo;
    [SerializeField] private AudioSource flashAudio;
    [SerializeField] private Text text;
    PlayerControls controls;
    private bool gameStarted = false;
    public static string SAVED_LEVEL_KEY = "SavedLevelIndex";
    private static int lastBuildIndex;

    private void Awake()
    {
        PlayerAnimation.ShownCoyoteIndicatorCounter = 0;
        controls = new PlayerControls();
        controls.UI.Start.performed += ctx => StartGame();
        controls.UI.Disable();
    }

    private void Start()
    {
        logo.DOAnchorPosY(-7, 2f).SetEase(Ease.OutBounce).OnComplete(() => LoadButton());
    }

    private void LoadButton()
    {
        Cursor.visible = false;
        startButton.gameObject.SetActive(true);
        startButton.image.DOFade(1f, 1f).SetEase(Ease.Linear);
        text.DOFade(1f, 1f).SetEase(Ease.Linear).OnComplete(() => controls.UI.Enable());

    }
    public void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;
        flashImage.DOFade(1f, 0.3f).SetLoops(2, LoopType.Yoyo).OnComplete(() => LoadGame());
        flashAudio.Play();
    }

    private void LoadGame()
    {
        int savedLevel = PlayerPrefs.GetInt(SAVED_LEVEL_KEY, 1);
        SceneManager.LoadScene(savedLevel);
    }

    void OnDisable()
    {
        controls.UI.Disable();
    }
}
