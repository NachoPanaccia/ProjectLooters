using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyChat : MonoBehaviour
{
    [SerializeField] private GameObject mensajePrefab;
    [SerializeField] private Transform contenedorMensajes;

    public void AgregarMensaje(string texto)
    {
        GameObject nuevoMensaje = Instantiate(mensajePrefab, contenedorMensajes);
        TMP_Text textoTMP = nuevoMensaje.GetComponent<TMP_Text>();

        if (textoTMP != null)
        {
            textoTMP.text = texto;
        }
    }
}