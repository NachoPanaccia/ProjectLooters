using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MenuNameManager : MonoBehaviourPunCallbacks
{

    [SerializeField] private InputField nicknameInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Text errorText;

    private Coroutine mensajeErrorCoroutine;

    
    void Start()
    {
        if (PlayerPrefs.HasKey("nickname"))
        {
            nicknameInput.text = PlayerPrefs.GetString("nickname");
        }
        confirmButton.onClick.AddListener(SetNickname);
        errorText.text = "";
    }

    private void SetNickname()
    {
        string nickname = nicknameInput.text.Trim();

        string[] nombresProhibidos = { "admin", "administrador", "Admin", "Administrador"};

        foreach (string prohibido in nombresProhibidos)
        {
            if (nickname.Equals(prohibido, System.StringComparison.OrdinalIgnoreCase))
            {
                MostrarError("Ese nickname está reservado y no puede ser usado.");
                return;
            }
        }

        if (string.IsNullOrEmpty(nickname))
        {
            MostrarError("El nickname no puede estar vacío.");
            return;
        }

        PhotonNetwork.NickName = nickname;
        PlayerPrefs.SetString("nickname", nickname);
        errorText.text = "";
        Debug.Log("Nickname seteado correctamente: " + nickname);

        NicknameMenuTransition nicknameMenuTransition = FindObjectOfType<NicknameMenuTransition>();
        if (nicknameMenuTransition != null)
        {
            nicknameMenuTransition.FadeToScene("Menu Principal");
        }
        else
        {
            Debug.LogWarning("SceneTransition no encontrado en la escena.");
        }
    }

    private void MostrarError(string mensaje)
    {
        if (mensajeErrorCoroutine != null)
        {
            StopCoroutine(mensajeErrorCoroutine);
        }

        errorText.text = mensaje;
        mensajeErrorCoroutine = StartCoroutine(LimpiarMensajeErrorLuegoDeTiempo(3f));
    }
    
    private IEnumerator LimpiarMensajeErrorLuegoDeTiempo(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        errorText.text = "";
        mensajeErrorCoroutine = null;
    }
}
