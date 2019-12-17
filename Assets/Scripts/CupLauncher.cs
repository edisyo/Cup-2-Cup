using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupLauncher : MonoBehaviour
{
    private Rigidbody cup;
    private Vector3 targetPos;
    private Vector3 startPos;

    public float h = 25f;
    public float gravity = -18f;

    private bool hasChanged = false;
    
    void Start()
    {
        cup = gameObject.GetComponent<Rigidbody>();
        cup.useGravity = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            launch();
        }
    }

    void launch()
    {
        hasChanged = false;
        readyCupToMoveRight();
        Physics.gravity = Vector3.up * gravity;
        cup.useGravity = true;
        cup.velocity = CalculateLaunchVelocity();
    }

    Vector3 CalculateLaunchVelocity()
    {
        float displacementY = targetPos.y - startPos.y;
        Vector3 displacementXZ = new Vector3(targetPos.x - startPos.x, 0f, targetPos.z - startPos.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity));

        return velocityXZ + velocityY;
    }
    

    void readyCupToMoveRight()
    {
        if(!hasChanged)
        {   
            //readyCupRotation();
            hasChanged = true;///TURN ON, SO WE UPDATE POSITION ONLY ONCE!!!
            startPos = transform.position;
            targetPos = new Vector3(transform.position.x + 2f, transform.position.y, transform.position.z);
            //print("RIGHT: reseting to new start [" + startPos + "] and end pos [" + endPos + "]");
        }
        
    }
}
