using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameOver : MonoBehaviour
{

    public PlayerHealth playerHealth;
    public GameObject Container1;
    
    public void YouDied()
    {
        Container1.SetActive(true);
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (playerHealth.isDead)
        {
            YouDied();
        }
    }
    
    public void OptionsButton()
    {
        
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
