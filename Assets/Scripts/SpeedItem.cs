using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedItem : MonoBehaviour
{
    public WeaponType thisWeaponType;
    int OwnerActorIndex;

    private void Start()
    {
        if(!GameData.SinglePlayer)
            OwnerActorIndex = gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.transform.root.CompareTag("Player") && (KartRaceManager.Instance.PlayerViewIDReference[OwnerActorIndex - 1] != other.transform.root.gameObject.GetComponent<PhotonView>().ViewID))
        if (other.transform.root.CompareTag("Player"))
        {     
            if (this.thisWeaponType == WeaponType.SlowDown)
            {
                //gameObject.SetActive(false);
                PhotonNetwork.Destroy(gameObject);
                other.attachedRigidbody.GetComponent<KartController>().Wiggle(1f);
                other.attachedRigidbody.GetComponent<KartController>().SpeedPenalty(0.3f, 1f, 1f);
            }
        }
    }
}


