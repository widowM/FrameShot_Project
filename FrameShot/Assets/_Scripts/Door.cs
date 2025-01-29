using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [SerializeField] private Image fadeOutImage;
    [SerializeField] private GameObject thanksText;
    [SerializeField] private bool isLastLevel = false;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            Time.timeScale = 0;
            fadeOutImage.DOFade(1, 1.5f).SetUpdate(true).OnComplete(() => HandleLevelExit());
        }
    }

    private void HandleLevelExit()
    {
        if (isLastLevel)
        {
            StartCoroutine(ExitGameCoroutine());
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    IEnumerator ExitGameCoroutine()
    {
        thanksText.SetActive(true);
        yield return new WaitForSecondsRealtime(3f);
        PlayerPrefs.SetInt(SceneLoader.SAVED_LEVEL_KEY, 1);  // Reset to first level
        PlayerPrefs.Save();
        Application.Quit();
    }
}
