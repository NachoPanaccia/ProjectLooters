using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        playButton.onClick.AddListener(PlayGame);
        quitButton.onClick.AddListener(QuitGame);

        optionsButton.interactable = false;
    }

    void Update()
    {

    }

    private void PlayGame()
    {
        SceneManager.LoadScene("Lobby");
    }

    private void QuitGame()
    {
        Application.Quit();
        Debug.Log("Juego cerrado");
    }
}
