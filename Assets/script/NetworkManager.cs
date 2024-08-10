using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        ConnectToPhoton();
    }

    void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings(); // Connect to Photon server
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu"; // Specify the region for better connectivity, adjust as needed
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby(); // Join default lobby
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        if (!PhotonNetwork.InRoom)
        {
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 20, IsVisible = true, IsOpen = true };
            PhotonNetwork.JoinOrCreateRoom("Room", roomOptions, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause);
        ConnectToPhoton();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join Random Failed: " + message);
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 20 });
    }
}
