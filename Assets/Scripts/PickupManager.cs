using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupManager : MonoBehaviour
{
    public Text CollectedCoins;
    public Image CurrentItem;
    public Sprite[] Itemicons;

    private static PickupManager _instance;
    public static PickupManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        CollectedCoins.text = GameData.CoinsCollected.ToString();
    }

    public void UpdateCollectedCoinText(int coins)
    {
        CollectedCoins.text = coins.ToString();
    }

    public void UpdateCurrentItemIcon(WeaponType itemcollected)
    {
        if (itemcollected == WeaponType.None)
        {
            CurrentItem.sprite = null;
            CurrentItem.color = new Color(CurrentItem.color.r, CurrentItem.color.g, CurrentItem.color.b, 0);
        }
        else
        {
            CurrentItem.sprite = Itemicons[(int)itemcollected];
            CurrentItem.color = new Color(CurrentItem.color.r, CurrentItem.color.g, CurrentItem.color.b, 1);
        }
    }

}
