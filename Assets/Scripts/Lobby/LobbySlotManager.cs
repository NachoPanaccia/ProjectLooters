using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class LobbySlotManager : MonoBehaviourPunCallbacks
{
    [Header("Referencias visuales")]
    [SerializeField] private Text policeSlot;
    [SerializeField] private Text[] thiefSlots;

    [Header("Botones de cambio de slot")]
    [SerializeField] private SlotChangeButton[] slotButtons;

    [Header("Botón de comenzar partida")]
    [SerializeField] private Button startGameButton;

    private Dictionary<int, (int slot, string nickname)> estadoSlots = new Dictionary<int, (int, string)>();
    

    private void Start()
    {
        LimpiarSlotsVisuales();
        ValidarBotonComenzar();
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Soy el primer jugador (MasterClient). Asignándome manualmente...");
            photonView.RPC("RPC_EnviarNicknameAlMaster", RpcTarget.MasterClient, PhotonNetwork.NickName);
        }

        photonView.RPC("RPC_EnviarNicknameAlMaster", RpcTarget.MasterClient, PhotonNetwork.NickName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (estadoSlots.ContainsKey(otherPlayer.ActorNumber))
        {
            estadoSlots.Remove(otherPlayer.ActorNumber);
            ActualizarVisualGlobal();
        }
    }

    [PunRPC]
    private void RPC_EnviarNicknameAlMaster(string nickname, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Player player = info.Sender;
        int actorNumber = player.ActorNumber;

        if (estadoSlots.ContainsKey(actorNumber))
            return;

        int slot = AsignarSlotDisponible();
        if (slot == int.MinValue)
        {
            Debug.LogWarning("No hay slots disponibles para " + nickname);
            return;
        }

        estadoSlots[actorNumber] = (slot, nickname);
        PhotonNetwork.CurrentRoom.GetPlayer(actorNumber).SetCustomProperties(new Hashtable { { "slot", slot } });
        ActualizarVisualGlobal();
    }

    private int AsignarSlotDisponible()
    {
        if (!ExisteSlotOcupado(-1)) return -1;
        for (int i = 0; i < thiefSlots.Length; i++)
        {
            if (!ExisteSlotOcupado(i)) return i;
        }
        return int.MinValue;
    }

    private bool ExisteSlotOcupado(int slot)
    {
        foreach (var kvp in estadoSlots)
        {
            if (kvp.Value.slot == slot) return true;
        }
        return false;
    }

    private void LimpiarSlotsVisuales()
    {
        policeSlot.text = "POLICÍA";
        foreach (Text t in thiefSlots) t.text = "LADRÓN";
    }

    private void ActualizarVisualGlobal()
    {
        policeSlot.text = "POLICÍA";
        foreach (Text t in thiefSlots) t.text = "LADRÓN";

        foreach (var kvp in estadoSlots)
        {
            photonView.RPC("RPC_ActualizarSlotVisual", RpcTarget.All, kvp.Value.slot, kvp.Value.nickname);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            bool[] ocupacion = new bool[4];
            foreach (var entry in estadoSlots.Values)
            {
                int index = entry.slot == -1 ? 0 : entry.slot + 1;
                ocupacion[index] = true;
            }

            photonView.RPC("RPC_ActualizarEstadoBotones", RpcTarget.All, ocupacion);
            ValidarBotonComenzar();
        }
    }

    private void ValidarBotonComenzar()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int totalJugadores = estadoSlots.Count;
        bool hayPolicia = false;
        bool hayLadron = false;

        foreach (var entry in estadoSlots.Values)
        {
            if (entry.slot == -1) hayPolicia = true;
            if (entry.slot >= 0) hayLadron = true;
        }

        bool habilitado = (totalJugadores >= 2) && hayPolicia && hayLadron;

        photonView.RPC("RPC_SetBotonIniciarPartidaEstado", RpcTarget.All, habilitado);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer == newMasterClient)
        {
            Debug.Log("Soy el nuevo MasterClient. Solicitando estados a los jugadores...");

            photonView.RPC("RPC_PedirEstado", RpcTarget.Others);

            int miSlot = PlayerPrefs.GetInt("MiSlot", -99);
            string miNick = PhotonNetwork.NickName;

            photonView.RPC("RPC_EnviarEstadoAlNuevoMaster", PhotonNetwork.LocalPlayer, miNick, miSlot);
        }
    }

    [PunRPC]
    private void RPC_ActualizarSlotVisual(int slot, string nickname, PhotonMessageInfo info)
    {
        if (slot == -1) policeSlot.text = nickname;
        else if (slot >= 0 && slot < thiefSlots.Length) thiefSlots[slot].text = nickname;

        if (info.Sender.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            PlayerPrefs.SetInt("MiSlot", slot);
            
            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { "slot",   slot },
                { "prefab", slot == -1 ? "Guard" : $"Thief{slot}" }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(ht);
        }
    }

    [PunRPC]
    private void RPC_ActualizarEstadoBotones(bool[] ocupacion)
    {
        foreach (var boton in slotButtons)
        {
            int slot = boton.GetTargetSlot();
            int index = slot == -1 ? 0 : slot + 1;
            boton.SetInteractable(!ocupacion[index]);
        }
    }

    [PunRPC]
    private void RPC_SolicitarCambioDeSlot(int actorNumberSolicitante, int slotDeseado)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        foreach (var kvp in estadoSlots)
        {
            if (kvp.Value.slot == slotDeseado)
                return;
        }

        if (!estadoSlots.ContainsKey(actorNumberSolicitante))
            return;

        string nickname = estadoSlots[actorNumberSolicitante].nickname;
        int slotAnterior = estadoSlots[actorNumberSolicitante].slot;

        photonView.RPC("RPC_LimpiarSlot", RpcTarget.All, slotAnterior);

        estadoSlots[actorNumberSolicitante] = (slotDeseado, nickname);
        ActualizarVisualGlobal();
    }

    [PunRPC]
    private void RPC_LimpiarSlot(int slot)
    {
        if (slot == -1)
            policeSlot.text = "POLICÍA";
        else if (slot >= 0 && slot < thiefSlots.Length)
            thiefSlots[slot].text = "LADRÓN";
    }

    [PunRPC]
    private void RPC_PedirEstado()
    {
        int slotActual = PlayerPrefs.GetInt("MiSlot", -99);
        string nickname = PhotonNetwork.NickName;

        photonView.RPC("RPC_EnviarEstadoAlNuevoMaster", RpcTarget.MasterClient, nickname, slotActual);
    }

    [PunRPC]
    private void RPC_EnviarEstadoAlNuevoMaster(string nickname, int slot, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int actorNumber = info.Sender.ActorNumber;
        estadoSlots[actorNumber] = (slot, nickname);

        ActualizarVisualGlobal();
    }

    [PunRPC]
    private void RPC_SetBotonIniciarPartidaEstado(bool habilitado)
    {
        if (startGameButton != null)
        {
            startGameButton.interactable = habilitado;
            startGameButton.gameObject.SetActive(true);
        }
    }

    [PunRPC]
    private void RPC_IniciarPartidaDesdeCliente(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"Jugador {info.Sender.NickName} solicitó iniciar la partida.");

        GuardarSlotsEnRoomProperties();

        PhotonNetwork.LoadLevel("Cargando");
    }

    public void GuardarSlotsEnRoomProperties()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();

        foreach (var entry in estadoSlots)
            props[$"slot_{entry.Key}"] = entry.Value.slot;

        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}