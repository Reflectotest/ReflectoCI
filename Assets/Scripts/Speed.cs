using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : MonoBehaviour
{
    public SpeedType thisHurdleType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player") && (other.attachedRigidbody.GetComponent<PhotonView>().IsMine || GameData.SinglePlayer))
        {
            if (this.thisHurdleType == SpeedType.Penalty)
                other.attachedRigidbody.GetComponent<KartController>().SpeedPenalty();
            //ExamplePlayerController.Instance.kart.SpeedPenalty();
            else if (this.thisHurdleType == SpeedType.Boost)
                other.attachedRigidbody.GetComponent<KartController>().SpeedBoost();
        }
    }
}


