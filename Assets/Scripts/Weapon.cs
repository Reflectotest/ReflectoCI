using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponType thisWeaponType;
    public ParticleSystem CollectionEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player") && (other.attachedRigidbody.GetComponent<PhotonView>().IsMine || GameData.SinglePlayer))
        {
            gameObject.SetActive(false);
            //transform.parent.gameObject.SetActive(false);
            CollectionEffect.Play();
            //GameData.WeaponsAndPowerDrops[(int)this.thisWeaponType]++;
            GameData.CurrentWeapon = this.thisWeaponType;
            PickupManager.Instance.UpdateCurrentItemIcon(this.thisWeaponType);

            //KartRaceManager.Instance.SetNumberOfItemsText(this.thisWeaponType, GameData.WeaponsAndPowerDrops[(int)this.thisWeaponType]);
        }
    }

    WaitForSeconds dropwait = new WaitForSeconds(3);
    IEnumerator ReEnableDrop()
    {
        yield return dropwait;
        gameObject.SetActive(true);
    }
}
