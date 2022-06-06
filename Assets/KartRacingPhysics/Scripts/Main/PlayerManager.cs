using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(KartController))]
public class PlayerManager : MonoBehaviour
{
    public KartController kart;
    public PhotonView PV;

    //For Single Player
    public bool MyKart = true;
    //CheckPoints
    [SerializeField]
    int currentCheckpoint = -1;
    [SerializeField]
    int totalCheckpoints = 0;

    private Transform[] CheckPointList;

    //Wrong Way
    bool WrongWay = false;

    float dist_next = 0;
    float dist_prev = 0;

    //Laps
    [SerializeField]
    int CurrentLap = 1;
    bool CrossedFinishLine = false;

    [Header("Weapons")]
    [SerializeField] Rigidbody RocketWeaponPrefab;
    public int RocketThrowForce;
    public float BombThrowForce;

    [SerializeField] GameObject SlowDownItemPrefab;
    private void Start()
    {
        this.kart = base.GetComponent<KartController>();
        this.PV = base.GetComponent<PhotonView>();

        if (this.PV.IsMine)
        {
            //Set up player
            GameData.myPhotonViewID = gameObject.GetPhotonView().ViewID;
            GameData.myKartRef = this.gameObject;
            KartRaceManager.Instance.SetUpPlayer(PhotonNetwork.LocalPlayer.ActorNumber, gameObject.GetPhotonView().ViewID);
        }

        CheckPointList = KartRaceManager.Instance.GetCheckPoints();
        if (PV.IsMine || GameData.SinglePlayer)
            StartCoroutine(CheckForWrongWay());
    }

    private void Update()
    {
        if ((PV.IsMine && KartRaceManager.Instance.StartGame) || GameData.SinglePlayer)
        {
            this.kart.Thrust = Input.GetAxis("Vertical");
            this.kart.Steering = Input.GetAxis("Horizontal");
            /*if (Input.GetKeyDown(KeyCode.Q))
            {
                this.kart.Spin(2f);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                this.kart.Wiggle(2f);
            }*/
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.kart.Jump(1f);
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                UseWeapon();
                //if(GameData.WeaponsAndPowerDrops[(int)WeaponType.Bomb] > 0)
                    //ShootBombForward();
            }

        }
        /*if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			this.kart.SpeedBoost();
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			this.kart.SpeedPenalty();
		}*/
    }

    public void SetCurrentCheckPoint(int waypointIndex)
    {
        //currentCheckpoint = waypointIndex;

        if (PV.IsMine || GameData.SinglePlayer)
        {
            if(waypointIndex == -1 && (currentCheckpoint == (CheckPointList.Length - 2)))
            {
                //Debug.Log("FINISH LINE?");
                currentCheckpoint = waypointIndex;
                return;
            }
            //if (waypointIndex != -1)
            //{
            if (waypointIndex < currentCheckpoint)
            {
                KartRaceManager.Instance.WrongWayMessage.text = "Wrong Way!";
                WrongWay = true;
            }
            else
            {
                //if (currentCheckpoint - waypointIndex <= 1)
                if (Mathf.Abs(currentCheckpoint - waypointIndex) < 2)
                {
                    KartRaceManager.Instance.WrongWayMessage.text = null;
                    WrongWay = false;
                    currentCheckpoint = waypointIndex;
                }
                else
                {
                    //Wrong Way
                    KartRaceManager.Instance.WrongWayMessage.text = "Wrong Way!";
                    WrongWay = true;
                }
            }
            //}
            /*else
            {

                KartRaceManager.Instance.WrongWayMessage.text = null;
                //WrongWay = false;
                currentCheckpoint = waypointIndex;
                CrossedFinishLine = true;
            }*/
        }
    }

    int flag = 0;
    public void SetCurrentLap()
    {
        if (PV.IsMine || GameData.SinglePlayer)
        {
            if (!WrongWay && flag < 1)
            {
                if (CurrentLap < GameData.NumberOfLaps)
                {
                    CurrentLap++;
                    KartRaceManager.Instance.Laps.text = (CurrentLap - 1).ToString() + "/" + GameData.NumberOfLaps.ToString();
                }
                else if (CurrentLap == GameData.NumberOfLaps)
                {
                    //Finished Laps
                    KartRaceManager.Instance.Laps.text = CurrentLap.ToString() + "/" + GameData.NumberOfLaps.ToString();
                    if (!GameData.SinglePlayer)
                    {
                        //Calculate Rank
                        KartRaceManager.Instance.StartGame = false;
                        KartRaceManager.Instance.UpdateRanks(PhotonNetwork.LocalPlayer.ActorNumber);
                        //TEST Player number
                    }
                    Debug.Log("Total Laps Completed: " + CurrentLap);
                }

            }
            flag++;
        }
    }

    public void OnCrossedStartLine()
    {
        flag = 0;
        CrossedFinishLine = false;
        currentCheckpoint = 0;
    }

    IEnumerator CheckForWrongWay()
    {
        while (true)
        {
            CalculateDistanceCheckPoints();

            yield return new WaitForSeconds(1);
        }
    }

    float prev_dist_next;
    float prev_dist_prev;
    private void CalculateDistanceCheckPoints()
    {
        prev_dist_next = dist_next;
        prev_dist_prev = dist_prev;

        if (currentCheckpoint < CheckPointList.Length - 1)
            dist_next = Vector3.Distance(transform.position, CheckPointList[currentCheckpoint + 1].position);
        else
            dist_next = Vector3.Distance(transform.position, CheckPointList[0].position);

        if (currentCheckpoint > 0)
            dist_prev = Vector3.Distance(transform.position, CheckPointList[currentCheckpoint - 1].position);
        else
            dist_prev = Vector3.Distance(transform.position, CheckPointList[CheckPointList.Length - 2].position);

        float next_difference = dist_next - prev_dist_next;
        float prev_difference = dist_prev - prev_dist_prev;

        //if(dist_next <= prev_dist_next && dist_prev >= prev_dist_prev)
        if (next_difference <= 15 && prev_difference >= -15)
        {
            WrongWay = false;
            KartRaceManager.Instance.WrongWayMessage.text = null;
        }
        //else if(dist_next > prev_dist_next && dist_prev < prev_dist_prev)
        else if (next_difference > 15 && prev_difference < -15)
        {
            WrongWay = true;
            StartCoroutine(DisplayWrongWayPrompt());
            //KartRaceManager.Instance.WrongWayMessage.text = "Wrong Way!";
            //Debug.Log("NEXT: " + next_difference);
            //Debug.Log("PREVIOUS: " + prev_difference);
        }
    }

    WaitForSeconds WrongWayDelay = new WaitForSeconds(2);

    IEnumerator DisplayWrongWayPrompt()
    {
        yield return WrongWayDelay;
        if(WrongWay)
            KartRaceManager.Instance.WrongWayMessage.text = "Wrong Way!";
        else
            KartRaceManager.Instance.WrongWayMessage.text = null;
    }

    private void ShootRocketProjectile()
    {
        //GameData.NumberOfProjectileRockets--;
        //KartRaceManager.Instance.SetNumberOfRocketsText(GameData.NumberOfProjectileRockets);

        var rot = Quaternion.AngleAxis(60, Vector3.right);
        // that's a local direction vector that points in forward direction but also 45 upwards.
        var lDirection = rot * -Vector3.forward;
        var rocket = Instantiate(RocketWeaponPrefab, transform.position, RocketWeaponPrefab.transform.rotation);
        rocket.isKinematic = false;
        //rocket.AddForce(-Vector3.forward * RocketThrowForce, ForceMode.Impulse);
        rocket.AddForce(lDirection * RocketThrowForce, ForceMode.Impulse);
    }

    private void ShootBombForward()
    {
        GameData.CurrentWeapon = WeaponType.None;
        //Vector3 offset = new Vector3(-1, 0.2f, -1);
        Vector3 offset = transform.forward * 1.5f + new Vector3(0, 0.1f, 0);
        if (GameData.SinglePlayer)
        {
            Rigidbody rocketSP = Instantiate(RocketWeaponPrefab, transform.position + offset, RocketWeaponPrefab.transform.rotation);
            rocketSP.isKinematic = false;
            rocketSP.AddForce(transform.forward * BombThrowForce, ForceMode.Impulse);
        }
        else
        {
            GameObject rocket = PhotonNetwork.Instantiate("Weapon", transform.position + offset, RocketWeaponPrefab.transform.rotation);
            PhotonView photonViewR = rocket.GetComponent<PhotonView>();
            photonViewR.RPC("RPC_AddForceBullet", RpcTarget.All, BombThrowForce);
            //photonViewR.RPC("RPC_AddForceBullet", RpcTarget.All, 1000, 1000);
        }
    }

    GameObject slow;
    private void DropSlowDownItem()
    {
        GameData.CurrentWeapon = WeaponType.None;

        //Vector3 offset = new Vector3(0, 0.1f, 1);
        Vector3 offset = -transform.forward + new Vector3(0, 0.1f, 0);
        if (GameData.SinglePlayer)
            slow = Instantiate(SlowDownItemPrefab, transform.position + offset, SlowDownItemPrefab.transform.rotation);
        else
            slow = PhotonNetwork.Instantiate("SlowDownCube", transform.position + offset, SlowDownItemPrefab.transform.rotation);
    }

    private void UseWeapon()
    {
        switch (GameData.CurrentWeapon)
        {
            case WeaponType.Bomb:
                ShootBombForward();
                    break;
            case WeaponType.SlowDown:
                DropSlowDownItem();
                break;
            case WeaponType.None:
                break;
        }
        PickupManager.Instance.UpdateCurrentItemIcon(WeaponType.None);
    }

    public void TotalCheckpointsIncrease()
    {
        totalCheckpoints++;
    }

    public bool IsWrongWay()
    {
        return WrongWay;
    }
}
