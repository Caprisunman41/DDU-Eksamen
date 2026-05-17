using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Continue-knappen — vises kun hvis der findes en gemt save.")]
    [SerializeField] private Button continueButton;

    private void Start()
    {
        if (continueButton != null)
        {
            bool hasSave = GameStateManager.Instance != null && GameStateManager.Instance.HasSaveFile();
            continueButton.gameObject.SetActive(hasSave);
        }
    }

    public void PlayGame()
    {
        if (GameStateManager.Instance != null) GameStateManager.Instance.ClearSave();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ContinueGame()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.HasSaveFile())
            GameStateManager.Instance.ContinueFromSave();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
