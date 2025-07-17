using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipalManager : MonoBehaviour
{
    [Header("Crear sala")]
    [SerializeField] private InputField inputCreateRoom;
    [SerializeField] private Button btnCreateRoom;

    [Header("Unirse a sala")]
    [SerializeField] private InputField inputJoinRoom;
    [SerializeField] private Button btnJoinRoom;

    [Header("Otros")]
    [SerializeField] private Button quitButton;

    private void Start()
    {
        btnCreateRoom.onClick.AddListener(CreateRoom);
        btnJoinRoom.onClick.AddListener(JoinRoom);
        quitButton.onClick.AddListener(QuitGame);

        btnCreateRoom.interactable = false;
        btnJoinRoom.interactable = false;

        inputCreateRoom.onValueChanged.AddListener(s =>
            btnCreateRoom.interactable = !string.IsNullOrWhiteSpace(s));
        inputJoinRoom.onValueChanged.AddListener(s =>
            btnJoinRoom.interactable = !string.IsNullOrWhiteSpace(s));
    }

    private void CreateRoom()
    {
        RoomLauncher.PendingRoomName = inputCreateRoom.text.Trim();
        RoomLauncher.ShouldCreate = true;
        SceneManager.LoadScene("Lobby");
    }

    private void JoinRoom()
    {
        RoomLauncher.PendingRoomName = inputJoinRoom.text.Trim();
        RoomLauncher.ShouldCreate = false;
        SceneManager.LoadScene("Lobby");
    }

    private void QuitGame()
    {
        Application.Quit();
        Debug.Log("Juego cerrado.");
    }
}
