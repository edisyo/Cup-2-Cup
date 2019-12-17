using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public Color normalColor;
    public Color highlightColor;

    [Range(0.0f, 2f)]
    public float raycastRange;
    private Material objectMaterial;

    [HideInInspector] public int numberOfCups;

    void Start()
    {
        objectMaterial = GetComponent<Renderer>().material;
        setToNormalColor();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.right) * raycastRange, Color.red);
        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.left) * raycastRange, Color.black);
        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * raycastRange, Color.green);
        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.back) * raycastRange, Color.blue);

        
    }

    public void setToHighlighColor()
    {
        objectMaterial.color = highlightColor;
    }

    public void setToNormalColor()
    {
        objectMaterial.color = normalColor;
    }

    public bool checkRightPlatformForCups()//CHEK IF WE HAVE PLATFORM WITH CUPS
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out hit, raycastRange))
        {
            if(hit.collider.gameObject.GetComponentInChildren<cupMovement>() != null)
            {
                //print("has x amount of cups on right platform");
                numberOfCups = hit.collider.GetComponentInChildren<cupMovement>().stackAmount;
                print(hit.collider.name + " has number of cups: " + numberOfCups);
                return true;//get amount of cups
            }
            else
            {
                //print("has no cups on right platform");
                return false;
            }
        }else
        {
            return false;
        }
    }

    
    public bool checkLeftPlatformForCups()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out hit, raycastRange))
        {
            if(hit.collider.gameObject.GetComponentInChildren<cupMovement>() != null)
            {
                //print("has x amount of cups on left platform");
                numberOfCups = hit.collider.GetComponentInChildren<cupMovement>().stackAmount;
                print(hit.collider.name + " has number of cups: " + numberOfCups);
                return true;//get amount of cups
            }
            else
            {
                //print("has no cups on left platform");
                return false;
            }
        }else
        {
            return false;
        }

        
    }

    public bool checkUpPlatformForCups()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, raycastRange))
        {
            if(hit.collider.gameObject.GetComponentInChildren<cupMovement>() != null)
            {
                //print("has x amount of cups on up platform");
                numberOfCups = hit.collider.GetComponentInChildren<cupMovement>().stackAmount;
                print(hit.collider.name + " has number of cups: " + numberOfCups);
                return true;//get amount of cup
            }
            else
            {
                //print("has no cups on up platform");
                return false;
            }
        }else
        {
            return false;
        }
    }

    public bool checkDownPlatformForCups()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.back), out hit, raycastRange))
        {
            if(hit.collider.gameObject.GetComponentInChildren<cupMovement>() != null)
            {
                //print("has x amount of cups on down platform");
                numberOfCups = hit.collider.GetComponentInChildren<cupMovement>().stackAmount;
                print(hit.collider.name + " has number of cups: " + numberOfCups);
                return true;//get amount of cups
                
            }
            else
            {
                //print("has no cups on down platform");
                return false;
            }
        }else
        {
            return false;
        }
    }
}
