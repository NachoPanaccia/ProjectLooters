using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class StartManager : MonoBehaviour
{
    [SerializeField] private float minTiempoEnEscena = 3.5f;
    private float tiempoEntradaEscena;
    public enum EstadoInicio { EsperandoValidaciones, ValidacionesCorrectas, ErrorEnValidaciones }

    private EstadoInicio estadoActual = EstadoInicio.EsperandoValidaciones;

    private HashSet<string> validacionesEsperadas = new HashSet<string> { "Equipment" };
    private HashSet<string> validacionesRecibidas = new HashSet<string>();

    private bool partidaIniciada = false;

    private void Awake()
    {
        tiempoEntradaEscena = Time.time;    
    }

    public void ConfirmarValidacion(string nombre)
    {
        if (!validacionesEsperadas.Contains(nombre))
        {
            Debug.LogWarning($"StartManager: Validación desconocida recibida: {nombre}");
            return;
        }

        if (validacionesRecibidas.Contains(nombre))
        {
            Debug.LogWarning($"StartManager: La validación '{nombre}' ya fue recibida.");
            return;
        }

        validacionesRecibidas.Add(nombre);
        Debug.Log($"StartManager: Validación recibida: {nombre}");

        VerificarEstado();
    }

    public void ReportarError(string nombre, string detalle)
    {
        estadoActual = EstadoInicio.ErrorEnValidaciones;
        Debug.LogError($"[StartManager] Error en validación de '{nombre}': {detalle}");
    }

    private void VerificarEstado()
    {
        if (estadoActual == EstadoInicio.ErrorEnValidaciones || partidaIniciada)
            return;

        if (validacionesRecibidas.SetEquals(validacionesEsperadas))
        {
            estadoActual = EstadoInicio.ValidacionesCorrectas;
            Debug.Log("[StartManager] Todas las validaciones fueron exitosas.");

            StartCoroutine(IniciarPartidaTrasCuentaRegresiva());
        }
    }

    private IEnumerator IniciarPartidaTrasCuentaRegresiva()
    {
        partidaIniciada = true;

        float transcurrido = Time.time - tiempoEntradaEscena;
        float falta = Mathf.Max(0f, minTiempoEnEscena - transcurrido);
        if (falta > 0f)
        {
            Debug.Log($"StartManager -> esperando {falta:F1}s extra para cumplir tiempo mínimo…");
            yield return new WaitForSeconds(falta);
        }

        yield return new WaitForSeconds(3f);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Nivel Jugable");
        }
        else
        {
            Debug.Log("[StartManager] Soy cliente, esperando que el Master inicie la partida.");
        }
    }

    public EstadoInicio ObtenerEstadoActual()
    {
        return estadoActual;
    }
}
