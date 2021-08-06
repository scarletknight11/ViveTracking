using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
public class CreateRoomMenu : MonoBehaviourPunCallbacks {

    [SerializeField]
    private Text _roomName;

    public void OnClick_CreateRoom()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        PhotonNetwork.JoinOrCreateRoom(_roomName.text, options, TypedLobby.Default);

    }

    public override void OnCreateRoom()
    {
        MasterManager.DebugConsole.AddText("Created room successfully.", this);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        MasterManager.DebugConsole.AddText("Room creation failed: " + message, this);
    }
}
