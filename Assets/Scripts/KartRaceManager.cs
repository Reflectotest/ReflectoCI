using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Cameras;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using Photon.Pun;
using System;
using System.Linq;

public class KartRaceManager : MonoBehaviour
{
    PhotonView PV;
    public GameObject[] PlayerReference = new GameObject[0];
    public int[] PlayerViewIDReference = new int[0];

    public GameObject KartPrefab;
    public Transform[] KartSpawnPoints;

    public AutoCam CameraFollower;
    public HUD speedometer;

    public GameObject kartSpawned;
    public Text PlayerName;
    public Text Laps;
    public Text WrongWayMessage;
    [SerializeField] Text[] NumberOfItemsText;
    public Text PlayerPositions;

    public GameObject CheckPointParent;
    private Transform[] CheckPoints;

    public int[] PlayerRanks;
    public int Position = 0;
    public GameObject WinnerTextPrefab;
    public Transform WinnerTextParent;

    public Text PlayerPositionText;
    public int[] PlayerTotalCheckpointsCrossed;
    public int[] PlayerIdx;

    public bool StartGame;

    [Header("Waiting Info")]
    public GameObject WaitingScreen;
    public Text PlayersJoinedText;
    public Text CountDownText;

    public GameObject NoPlayersPanel;
    public Text PlayersMsg;

    bool TimerStopped = false;
    Coroutine TimeToStartCoroutine;
    public Text TimerText;

    private static KartRaceManager _instance;
    public static KartRaceManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != this && _instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (!GameData.SinglePlayer)
        {
            PhotonManager.Instance.m_KartRaceManager = this;
            PV = GetComponent<PhotonView>();
        }

        CheckPoints = new Transform[CheckPointParent.transform.childCount];
        for (int c = 0; c < CheckPointParent.transform.childCount; c++)
        {
            CheckPoints[c] = CheckPointParent.transform.GetChild(c);
            CheckPoints[c].GetComponent<CheckPoints>().CheckPointNumber = c;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient && !GameData.SinglePlayer)
        {
            InstantiateKart();
            InitializeWaitingText();
            //kartSpawned.layer = 6; //Player Layer
        }
        else if (GameData.SinglePlayer)
        {
            kartSpawned = Instantiate(KartPrefab, KartSpawnPoints[0].position, KartPrefab.transform.rotation);
            speedometer.vehicle = kartSpawned.GetComponent<KartController>();

            Laps.text = "0/" + GameData.NumberOfLaps.ToString();
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && (PhotonNetwork.PlayerList.Length == PhotonNetwork.CurrentRoom.MaxPlayers || TimerStopped) && PhotonNetwork.CurrentRoom.IsOpen && !StartGame)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;

            if (!TimerStopped)
                StopCoroutine(TimeToStartCoroutine);

            StartCoroutine(StartRace(3));
        }
    }

    IEnumerator StartRace(float wait)
    {
        yield return new WaitForSeconds(wait);
        PV.RPC(nameof(RPC_StartRace), RpcTarget.AllBuffered, GameData.NumberOfLaps, (object)PlayerViewIDReference);
    }

    [PunRPC]
    void RPC_StartRace(int laps, int[] viewIDs) //Set number of laps from master client
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            PlayerViewIDReference = new int[PhotonNetwork.CurrentRoom.MaxPlayers];
            PlayerViewIDReference = viewIDs;
        }

        foreach (int v in viewIDs)
        {
            Debug.Log("view ID: " + v);
            if (v == GameData.myPhotonViewID)
                GameData.myKartRef.layer = 6;
        }

        Position = 0;
        PlayerRanks = new int[PhotonNetwork.CurrentRoom.MaxPlayers];
        PlayerTotalCheckpointsCrossed = new int[PhotonNetwork.CurrentRoom.MaxPlayers];
        PlayerIdx = Enumerable.Range(0, PlayerTotalCheckpointsCrossed.Length).ToArray();
        GameData.NumberOfLaps = laps;
        Laps.text = "0/" + GameData.NumberOfLaps.ToString();

        //if (WaitingScreen.activeSelf)
        //WaitingScreen.SetActive(false);

        //StartGame = true;
        StartCoroutine(StartCountDown(3));
    }

    WaitForSeconds OneSecond = new WaitForSeconds(1);
    IEnumerator StartCountDown(int countdown)
    {
        PlayersJoinedText.text = null;
        for (int c = countdown; c > 0; c--)
        {
            CountDownText.text = c.ToString();
            yield return OneSecond;
        }

        if (WaitingScreen.activeSelf)
        {
            WaitingScreen.SetActive(false);
            CountDownText.text = null;
        }

        StartGame = true;
        StartCoroutine(CalculatePositionInRace());
    }

    public void SetUpPlayer(int playerID, int viewID)
    {
        PV.RPC(nameof(RPC_SetUpPlayer), RpcTarget.MasterClient, playerID, viewID);
    }

    [PunRPC]
    void RPC_SetUpPlayer(int playerID, int viewID)
    {
        if (PlayerReference.Length == 0)
        {
            PlayerReference = new GameObject[PhotonNetwork.CurrentRoom.MaxPlayers];
            PlayerViewIDReference = new int[PhotonNetwork.CurrentRoom.MaxPlayers];
        }

        PhotonView tempPhotonView = PhotonView.Find(viewID);
        PlayerReference[playerID - 1] = tempPhotonView.gameObject;
        PlayerViewIDReference[playerID - 1] = viewID;
    }

    public void UpdateRanks(int playerID)
    {
        Position++;
        //if(Position == 1) //First Place. Allot prize money
        if(Position <= 3) //First Three Places. Allot prize money
        {
            int prize = GameData.PriceToPay * PhotonNetwork.CurrentRoom.PlayerCount;
            //GameData.CoinsCollected -= prize;
            GameData.CoinsCollected += prize;
            PickupManager.Instance.UpdateCollectedCoinText(GameData.CoinsCollected);
            PlayersMsg.text = "Congratulations! You won " + AddOrdinal(Position) + " place!";
        }
        else
            PlayersMsg.text = "You have completed the race!";

        NoPlayersPanel.SetActive(true);

        PV.RPC(nameof(RPC_UpdateRanks), RpcTarget.AllBuffered, Position, playerID);
    }

    [PunRPC]
    void RPC_UpdateRanks(int position, int playerID)
    {
        Position = position;
        PlayerRanks[position - 1] = playerID;

        var winner = Instantiate(WinnerTextPrefab, WinnerTextParent);
        winner.GetComponent<Text>().text = Position.ToString() + ".  Player" + playerID.ToString();
        Debug.Log("Player" + playerID + " Position " + Position);
    }

    public void InstantiateKart()
    {
        Debug.Log("Player Actor number: " + PhotonNetwork.LocalPlayer.ActorNumber);
        kartSpawned = PhotonNetwork.Instantiate("Kart", KartSpawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].position, KartPrefab.transform.rotation);
        speedometer.vehicle = kartSpawned.GetComponent<KartController>();
        PlayerName.text = "Player " + PhotonNetwork.LocalPlayer.ActorNumber;

        //PhotonManager.Instance.PlayerReference[PhotonNetwork.LocalPlayer.ActorNumber - 1] = kartSpawned;
    }

    public void InitializeWaitingText()
    {
        if(!WaitingScreen.activeSelf)
            WaitingScreen.SetActive(true);
        PlayersJoinedText.text = "Players Joined: " + PhotonNetwork.CurrentRoom.PlayerCount + " of " + PhotonNetwork.CurrentRoom.MaxPlayers;

        if(PhotonNetwork.IsMasterClient)
            TimeToStartCoroutine = StartCoroutine(CountdownTimeToStart(GameData.TimeToStart));
    }

    int timercount;

    IEnumerator CountdownTimeToStart(int time)
    {
        timercount = time + 1;
        //yield return new WaitForSeconds(time);

        for(int t = 0; t < time; t++)
        {
            yield return OneSecond;
            timercount--;
            TimerText.text = timercount.ToString(); 
        }

        TimerText.text = null;

        if (PhotonNetwork.PlayerList.Length > 1) //More than one player has joined the race
            TimerStopped = true;
        else
        {
            Debug.Log("No players joined. Exit");
            NoPlayersPanel.SetActive(true);
            PlayersMsg.text = "No players have joined the room yet!";
        }
    }

    public void Exit()
    {
        PhotonManager.Instance.LeaveRoom();
    }

    
    public Transform[] GetCheckPoints()
    {
        return CheckPoints;
    }

    public void SetNumberOfItemsText(WeaponType type, int items)
    {
        NumberOfItemsText[(int)type].text = items.ToString();
    }


    WaitForSeconds FiveSeconds = new WaitForSeconds(5);
    WaitForSeconds TwoSeconds = new WaitForSeconds(2);
    IEnumerator CalculatePositionInRace()
    {
        //yield return FiveSeconds;
        yield return TwoSeconds;
        while (true)
        {
            Debug.Log("Calculate");
            //int[] A = new int[3] { 1, 2,3 };
            //int[] idx = Enumerable.Range(0, A.Length).ToArray();
            //Debug.Log(A[idx[0]]);
            //Array.Sort<int>(idx, (a, b) => A[a].CompareTo(A[b]));
            Array.Sort<int>(PlayerIdx, (a, b) => PlayerTotalCheckpointsCrossed[a].CompareTo(PlayerTotalCheckpointsCrossed[b]));
            Array.Reverse(PlayerIdx);
            for (int p = 0; p < PlayerIdx.Length; p++)
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber - 1 == PlayerIdx[p])
                    PlayerPositionText.text = AddOrdinal(p+1);
            }

            //yield return FiveSeconds;
            DisplayAllPlayersPositions();
            yield return TwoSeconds;
        }
    }

    string playerPositions;
    private void DisplayAllPlayersPositions()
    {
	playerPositions = "";
        for(int p = 0; p < PlayerIdx.Length; p++)
        {
            playerPositions += AddOrdinal(p + 1) + " Player " + (PlayerIdx[p] + 1).ToString() + "\n";
        }
        Debug.Log("Player Positions: " + playerPositions);
        PlayerPositions.text = playerPositions;
    }

    public string AddOrdinal(int num)
    {
        if (num <= 0) return num.ToString();

        switch (num % 100)
        {
            case 11:
            case 12:
            case 13:
                return num + "th";
        }

        switch (num % 10)
        {
            case 1:
                return num + "st";
            case 2:
                return num + "nd";
            case 3:
                return num + "rd";
            default:
                return num + "th";
        }
    }
}
