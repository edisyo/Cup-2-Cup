using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skinManager : MonoBehaviour
{
    public GameObject cup;
    public GameObject[] skins;
    private coinManager coinManager;
    private GameObject _pressedButton;
    void Start()
    {
        coinManager = FindObjectOfType<coinManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void getButtonGameObject(GameObject pressedButton)//FIRST THIS       
    {
        _pressedButton = pressedButton;
        print("pressed: " + _pressedButton.name);
    }

    public void changeSkin(int i)//THEN THIS
    {
        GameObject[] allCups;
        allCups = GameObject.FindGameObjectsWithTag("Cup");

        bool owned = isOwned();
        if(!owned)
        {
            //player doesnt have this skin
            //can he purchase it?
            bool purchase = canPurchase();
            print("not owned");

            if (purchase)//if player has enough coins
            {
                print("can purchase");

                foreach (GameObject cup in allCups)
                {
                    cup.GetComponent<MeshFilter>().mesh = skins[i - 1].GetComponent<MeshFilter>().sharedMesh;
                    Material[] newMeshMaterials = skins[i - 1].GetComponent<MeshRenderer>().sharedMaterials;
                    cup.GetComponent<MeshRenderer>().materials = newMeshMaterials;
                }
                print("MESH!!!");

                coinManager.substractCoins(_pressedButton.GetComponent<skin>().skinPrice);
                _pressedButton.GetComponent<skin>().isOwned = true;
            }
            else
            {
                //dont have enough coins
                print("cant purchase");
            }
        }else
        {
            print("is owned");
            foreach (GameObject cup in allCups)
            {
                cup.GetComponent<MeshFilter>().mesh = skins[i - 1].GetComponent<MeshFilter>().sharedMesh;
                Material[] newMeshMaterials = skins[i - 1].GetComponent<MeshRenderer>().sharedMaterials;
                cup.GetComponent<MeshRenderer>().materials = newMeshMaterials;
            }
            print("MESH!!!");
        }

        
        
    }


    bool isOwned()
    {
        if(_pressedButton.GetComponent<skin>().isOwned)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    bool canPurchase()
    {
        int coinAmount = coinManager.getCoins();

        if(coinAmount >= _pressedButton.GetComponent<skin>().skinPrice)
        {
            
            return true;
        }
        else
        {
            return false;
        }
    }
    /**/
    
}
