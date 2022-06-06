using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static bool SinglePlayer = true;
    public static int myPhotonViewID;
    public static GameObject myKartRef;

    public static int TimeToStart;
    public static int PriceToPay;

    public static int CoinsCollected = 100; //Starting Amount
    public static int NumberOfLaps = 1;
    public static int[] WeaponsAndPowerDrops = new int[2] { 0, 0 };
    public static WeaponType CurrentWeapon = WeaponType.None;
}

public enum CheckPointType { Start, WayPoint, Finish };
public enum SpeedType { Penalty, Boost }
public enum WeaponType { Bomb, SlowDown, None };
public enum CustomProperties { PriceToPay };