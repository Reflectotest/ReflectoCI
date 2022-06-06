using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public GameObject RoomSpecsPrefab;
    public Transform RoomListparent;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    //public GameObject[] PlayerReferences;


    public KartRaceManager m_KartRaceManager;

    private static PhotonManager _instance;
    public static PhotonManager Instance { get { return _instance; } }

    private void Awake()
    {
        /*if (_instance != this && _instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);*/

        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        UIManager.Instance.EnableJoinCreateBtns();
    }

    public void OnClickCreateRoom()
    {
        Debug.Log("Price: "+ UIManager.Instance.GetPriceToPay());
        if(UIManager.Instance.GetPriceToPay() > GameData.CoinsCollected)
        {
            UIManager.Instance.DisplayMessageOnCreatePanel("Not enough coins!");
            return;
        }
        UIManager.Instance.DisplayMessageOnCreatePanel(null);
        GameData.PriceToPay = UIManager.Instance.GetPriceToPay();
        string[] LobbyOptions = new string[1];
        LobbyOptions[0] = CustomProperties.PriceToPay.ToString();

        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable()
        {
            {
                CustomProperties.PriceToPay.ToString(), UIManager.Instance.GetPriceToPay()
            }
        };

        //ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        //customProperties["PriceToPay"] = UIManager.Instance.GetPriceToPay();

        GameData.NumberOfLaps = UIManager.Instance.GetMaxLapsNumber();
        GameData.TimeToStart = UIManager.Instance.GetTimeToStart();
        int maxplayers = UIManager.Instance.GetMaxPlayerNumber();
        string roomname = UIManager.Instance.GetRoomName();
        RoomOptions roomoptions = new RoomOptions
        {
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = (byte)maxplayers,
            CustomRoomPropertiesForLobby = LobbyOptions,
            CustomRoomProperties = customProperties
        };

        PhotonNetwork.CreateRoom(roomname, roomoptions);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Room Created");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("Failed to create room: " + message);
    }

    public void JoinRoom(string gamename)
    {
        PhotonNetwork.JoinRoom(gamename);
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Joined Room");
        GameData.CoinsCollected -= GameData.PriceToPay;

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("PhotonNetwork : Trying to Load a level but we are not the master Client");
            m_KartRaceManager.InstantiateKart();
            m_KartRaceManager.InitializeWaitingText();
        }
        else
        {
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel(1);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("player entered");
        base.OnPlayerEnteredRoom(newPlayer);
        KartRaceManager.Instance.InitializeWaitingText();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log("Failed to join room: " + message);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        cachedRoomList.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        UpdateCachedRoomList(roomList);
    }
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        cachedRoomList.Clear();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        cachedRoomList.Clear();
    }

    int serial = 1;
    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        DestroyPrevRooms();

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
                //cachedRoomList.RemoveAt(i);
            }
            else
            {
                cachedRoomList[info.Name] = info;
                var room = Instantiate(RoomSpecsPrefab, RoomListparent);
                //Debug.Log("Price To Pay: " + info.CustomProperties["PriceToPay"]);
                room.GetComponent<RoomSpecs>().InitializeRoom(serial, info.Name, info.PlayerCount, info.MaxPlayers, info.CustomProperties[CustomProperties.PriceToPay.ToString()].ToString());
                serial++;
                //cachedRoomList[i] = info;
            }
        }
    }

    private void DestroyPrevRooms()
    {
        serial = 1;
        GameObject[] prevrooms = GameObject.FindGameObjectsWithTag("RoomSpecs");
        foreach (GameObject g in prevrooms)
            Destroy(g);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log("Left Room");
        Destroy(gameObject);
        SceneManager.LoadScene(0);
    }

}

