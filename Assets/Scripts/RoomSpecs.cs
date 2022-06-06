using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomSpecs : MonoBehaviour
{
    [SerializeField] Text SerialNumber;
    [SerializeField] Text GameName;
    [SerializeField] Text SpotsLeft;
    [SerializeField] Text Price;

    public void InitializeRoom(int serial, string roomname, int connectedplayers, int maxplayers, string price)
    {
        this.SerialNumber.text = serial.ToString();
        this.GameName.text = roomname;
        int left = maxplayers - connectedplayers;
        this.SpotsLeft.text = left.ToString() + "/" + maxplayers.ToString();
        this.Price.text = price;
    }

    public void OnClickJoinBtn()
    {
        if(Convert.ToInt32(Price.text) > GameData.CoinsCollected)
        {
            UIManager.Instance.DisplayMessageOnJoinPanel("Not enough coins!");
            return;
        }
        UIManager.Instance.DisplayMessageOnJoinPanel(null);
        GameData.PriceToPay = Convert.ToInt32(Price.text);
        PhotonManager.Instance.JoinRoom(this.GameName.text);
    }
}
