using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class skin : MonoBehaviour
{
    public int skinPrice = 3;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI ownedText;
    public bool isOwned;

    void Start()
    {
        priceText.gameObject.SetActive(true);
        ownedText.gameObject.SetActive(false);

        if(!isOwned)
        {
            priceText.text = skinPrice.ToString();
        }
        else
        {
            ownedText.gameObject.SetActive(true);
            priceText.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!isOwned)
        {
            priceText.text = skinPrice.ToString();
        }
        else
        {
            ownedText.gameObject.SetActive(true);
            priceText.gameObject.SetActive(false);
        }
    }
}
