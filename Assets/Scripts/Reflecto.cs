using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reflecto : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player") && (other.attachedRigidbody.GetComponent<PhotonView>().IsMine || GameData.SinglePlayer))
        {
            GameData.CoinsCollected++;
            PickupManager.Instance.UpdateCollectedCoinText(GameData.CoinsCollected);
            this.gameObject.SetActive(false);
        }
        //Destroy(this.gameObject);
    }
}
