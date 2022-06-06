using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHit : MonoBehaviour
{
    public GameObject ExplosionPrefab;
    public int BombImpactRadius = 8;
    //public LayerMask layermask;

    int OwnerActorIndex;
    private void Start()
    {
        if (!GameData.SinglePlayer)
            OwnerActorIndex = gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
    }

    [PunRPC]
    void RPC_AddForceBullet(int force)
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        if(OwnerActorIndex == 0 && !GameData.SinglePlayer)
            OwnerActorIndex = gameObject.GetComponent<PhotonView>().Owner.ActorNumber;

        GameObject explosion = Instantiate(ExplosionPrefab, transform.position, ExplosionPrefab.transform.rotation);
        //GameObject explosion = PhotonNetwork.Instantiate("BigExplosion", transform.position, ExplosionPrefab.transform.rotation);
        gameObject.SetActive(false);
        Collider[] hitObjects = Physics.OverlapSphere(explosion.transform.position, BombImpactRadius);
        
        foreach(Collider hit in hitObjects)
        {
            //TODO: Conditions in single player for not hitting oneself
            //if (hit.transform.root.CompareTag("Player") && ((!GameData.SinglePlayer &&!hit.attachedRigidbody.GetComponent<PhotonView>().IsMine) || (GameData.SinglePlayer && !hit.attachedRigidbody.GetComponent<PlayerManager>().MyKart)))
            //Debug.Log("Owner Actor Number: " + OwnerActorIndex);
            //if (hit.transform.root.CompareTag("Player") && (KartRaceManager.Instance.PlayerViewIDReference[OwnerActorIndex - 1] != hit.transform.root.gameObject.GetComponent<PhotonView>().ViewID))
            if (hit.transform.root.CompareTag("Player"))
            {
                hit.attachedRigidbody.GetComponent<KartController>().Spin(2);
                hit.attachedRigidbody.GetComponent<KartController>().SpeedPenalty(0.2f, 0.5f, 0.5f);
                break;
            }
        }
        Destroy(gameObject);
    }
    //PhotonNetwork.Destroy(gameObject);
}


