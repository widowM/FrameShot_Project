using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelProgressTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SaveCurrentLevel();
        }
    }

    private void SaveCurrentLevel()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("SavedLevelIndex", currentLevel);
        PlayerPrefs.Save();
    }
}