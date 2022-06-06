using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoints : MonoBehaviour
{
    public CheckPointType thisCheckPointType;
    public int CheckPointNumber;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player") && (other.attachedRigidbody.GetComponent<PhotonView>().IsMine || GameData.SinglePlayer))
        {
            if (!GameData.SinglePlayer)
            {
                if (!other.attachedRigidbody.GetComponent<PlayerManager>().IsWrongWay())
                {
                    int owner = other.attachedRigidbody.GetComponent<PhotonView>().Owner.ActorNumber;
                    KartRaceManager.Instance.PlayerTotalCheckpointsCrossed[owner - 1]++;
                }
            }
            //other.attachedRigidbody.GetComponent<PlayerManager>().TotalCheckpointsIncrease();
            if (thisCheckPointType == CheckPointType.Finish)
            {
                //Do Something
                other.attachedRigidbody.GetComponent<PlayerManager>().SetCurrentCheckPoint(-1);
                other.attachedRigidbody.GetComponent<PlayerManager>().SetCurrentLap();
            }
            else if (thisCheckPointType == CheckPointType.WayPoint)
                other.attachedRigidbody.GetComponent<PlayerManager>().SetCurrentCheckPoint(CheckPointNumber);
            else if (thisCheckPointType == CheckPointType.Start)
            {
                other.attachedRigidbody.GetComponent<PlayerManager>().OnCrossedStartLine();
            }
        }
        else if (other.transform.root.CompareTag("Player") && (!other.attachedRigidbody.GetComponent<PhotonView>().IsMine || GameData.SinglePlayer))
        {
            //int ownr = other.attachedRigidbody.GetComponent<PhotonView>().OwnerActorNr;
            int owner = other.attachedRigidbody.GetComponent<PhotonView>().Owner.ActorNumber;
            KartRaceManager.Instance.PlayerTotalCheckpointsCrossed[owner - 1]++;
            //other.attachedRigidbody.GetComponent<PlayerManager>().TotalCheckpointsIncrease();
        }
    }

}
