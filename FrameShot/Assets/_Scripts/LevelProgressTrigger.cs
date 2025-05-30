using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelProgressTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("player")) // Make sure the tag matches exactly - "player" (lowercase) vs "Player"
        {
            SaveCurrentLevel();
        }
    }

    private void SaveCurrentLevel()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt(SceneLoader.SAVED_LEVEL_KEY, currentLevel);
        PlayerPrefs.Save();
#if UNITY_EDITOR
        Debug.Log($"Level {currentLevel} saved as current level.");
        #endif
    }
}