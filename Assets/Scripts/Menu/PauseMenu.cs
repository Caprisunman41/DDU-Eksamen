using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject Container;
    public bool isPaused = false;
    public PlayerHealth playerHealth;

    void Update()
    {
        if (playerHealth.isDead)
        {
            return;
        }
        
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeButton();
            }
            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        Container.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeButton()
    {
        Container.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OptionsButton()
    {
        
    }

    public void RespawnButton()
    {
        Container.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        playerHealth.Respawn();
    }

    public void RestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Start");
    }
}