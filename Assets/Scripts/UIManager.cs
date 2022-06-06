using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject MainMenu;
    public GameObject CreateGamePanel;
    public GameObject JoinGamePanel;
    public GameObject RankListPanel;
    public GameObject BuyCarPanel;

    public Button SinglePlayerButton;

    [Header("Join Game")]
    public GameObject JoinPopup;
    public Button JoinGameBtn;
    public Text CoinsOnJoinPanel;
    public Text MessageOnJoinPanel;

    [Header("Create Game")]
    public Button CreateGameBtn;
    public Text CoinsOnCreatePanel;
    public Text MessageOnCreatePanel;

    public InputField MinPlayers;
    public InputField MaxPlayers;
    public InputField RoomName;
    public InputField NumberOfLaps;
    public InputField TimeToStart;
    public InputField PriceToPay;
    public InputField PrizeMoney;

    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }


    private void DeSelectAll()
    {
        MainMenu.SetActive(false);
        CreateGamePanel.SetActive(false);
        JoinGamePanel.SetActive(false);
        RankListPanel.SetActive(false);
        BuyCarPanel.SetActive(false);
    }

    public void OnClickCreateGame()
    {
        GameData.SinglePlayer = false;

        DeSelectAll();
        CreateGamePanel.SetActive(true);
        CoinsOnCreatePanel.text = GameData.CoinsCollected.ToString();
    }

    public void OnClickJoinGame()
    {
        GameData.SinglePlayer = false;

        DeSelectAll();
        JoinGamePanel.SetActive(true);
        CoinsOnJoinPanel.text = GameData.CoinsCollected.ToString();
    }

    public void OnClickRankListButton()
    {
        DeSelectAll();
        RankListPanel.SetActive(true);
    }

    public void OnClickBuyCar()
    {
        DeSelectAll();
        BuyCarPanel.SetActive(true);
    }

    public void OnClickJoin()
    {
        JoinPopup.SetActive(true);
    }

    public void OnClickCancelJoin()
    {
        JoinPopup.SetActive(false);
    }

    public void BackToMain()
    {
        DeSelectAll();
        MainMenu.SetActive(true);
    }

    public void OnClickSinglePlayer()
    {
        GameData.SinglePlayer = true;
        SceneManager.LoadScene(1);
    }

    public void EnableJoinCreateBtns()
    {
        JoinGameBtn.interactable = true;
        CreateGameBtn.interactable = true;

        SinglePlayerButton.interactable = true;
    }

    public int GetMaxPlayerNumber()
    {
        if (!string.IsNullOrEmpty(MaxPlayers.text))
        {
            int max = Convert.ToInt32(MaxPlayers.text);
            max = Mathf.Clamp(max, 2, 6);
            return max;
        }
        else
            return 2;
    }

    public int GetMaxLapsNumber()
    {
        if (!string.IsNullOrEmpty(NumberOfLaps.text))
        {
            int lap = Convert.ToInt32(NumberOfLaps.text);
            lap = Mathf.Clamp(lap, 1, 6);
            return lap;
        }
        else
            return 1;
    }

    public int GetTimeToStart()
    {
        if (!string.IsNullOrEmpty(TimeToStart.text))
        {
            int timer = Convert.ToInt32(TimeToStart.text);
            //timer = Mathf.Clamp(timer, 15, 60);
            return timer;
        }
        else
            return 15;
    }

    public int GetPriceToPay()
    {
        if (!string.IsNullOrEmpty(PriceToPay.text))
        {
            int price = Convert.ToInt32(PriceToPay.text);
            //price = Mathf.Clamp(price, 15, 60);
            return price;
        }
        else
            return 0;
    }

    public string GetRoomName()
    {
        return RoomName.text;
    }

    public void CalculatePrizeMoney()
    {
        int prize = GetPriceToPay() * GetMaxPlayerNumber();
        Debug.Log(prize);
        PrizeMoney.text = prize.ToString();
    }

    public void DisplayMessageOnCreatePanel(string msg)
    {
        MessageOnCreatePanel.text = msg;
    }

    public void DisplayMessageOnJoinPanel(string msg)
    {
        MessageOnJoinPanel.text = msg;
    }
}
