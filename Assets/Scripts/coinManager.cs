using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class coinManager : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    private int coins;

    void Start()
    {
        coins = 0;
    }

    
    void Update()
    {
        coinText.text = coins.ToString();
    }

    public void turnOn()
    {
        coinText.gameObject.SetActive(true);
    }

    public void turnOff()
    {
        coinText.gameObject.SetActive(false);
    }

    public void addCoins(int addedCoins)
    {
        coins += addedCoins;
    }

    public void substractCoins(int subCoins)
    {
        coins -= subCoins;
    }

    public int getCoins()
    {
        return coins;
    }
}
