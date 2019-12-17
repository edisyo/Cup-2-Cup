using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cupMovement : MonoBehaviour
{
    [Header("Direction states")]
    public bool moveRight;
    public bool moveLeft;
    public bool moveUp;
    public bool moveDown;    
    
    [Header("Rotation state")]
    public bool standingNormal;//true - STANING NORMALLY, false - STANDING ON CUP OPENING (USPIDE DOWN)

    [Header("Animation Speed Controllers")]
    [Range(0.0f, 3f)] public float positionSpeed;
    [Range(0.0f, 3f)] public float rotationSpeed;

    private float anim = 0f;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 collisionPos;
    private bool hasChanged = false;
    [HideInInspector] public bool isMoving = false;
    private bool readyForNewParent = false;
    private bool isStacked = false;
    private bool hasCollided;
    private GameObject CollisionObject;


    private Vector3 normalRotation = new Vector3(0f, 0f, 0f);
    private Vector3 upsideRotation = new Vector3(0f, 0f, 180f);
    private Vector3 startRot;
    private Vector3 endRot;
    private Vector3 collisionRot;

    private Vector3 OriginalPos;//POSITION, to go Back to if game is reset
    private Vector3 OriginalRot;//POSITION, to go Back to if game is reset
    private bool OriginalState;//POSITION, to go Back to if game is reset
    public GameObject OriginalPlatform;//POSITION, to go Back to if game is reset
    public GameObject OriginalRoot;
    

    [HideInInspector] private List<GameObject> cupStack = new List<GameObject>();
    [HideInInspector] public int nextStackHeight;//on swipe, we check how many cups are on next platform. We use this to animate cup to correct height

    private bool hasPlayed;


    [Header("STACKING CONTROL")]
    public int stackAmount;
    public GameObject rootCup;
    public GameObject collidersRootCup;

    touchManager touchManager;


    private void Start()
    {
        moveRight = moveDown = moveLeft = moveRight = false;
        hasChanged = false;
        isStacked = false;
        hasCollided = false;
        hasPlayed = false;
        
        CollisionObject = null;

        transform.GetComponent<MeshCollider>().isTrigger = true;

        anim = 0f;

        if(standingNormal)
        {
            transform.localEulerAngles = normalRotation;
            OriginalState = standingNormal;
            print("original state: " + OriginalState + standingNormal);
        }else
        {
            transform.localEulerAngles = upsideRotation;
            OriginalState = standingNormal;
            print("original state: " + OriginalState + standingNormal);
        }

        
        //cupStack.Add(gameObject);//add first item
        //print("cupstack count: " + cupStack.Count);
        //print("cupstack count: " + cupStack[0].name);
        //startPos = transform.position;
        //endPos = new Vector3(transform.position.x + 2f, transform.position.y, transform.position.z);

        stackAmount = 1;///add current item
        rootCup =  gameObject;///At first, everyone is its own root Cup
        
        OriginalPos = transform.position;//For retry button
        OriginalRoot = gameObject;
        OriginalPlatform = gameObject.transform.parent.gameObject;

        touchManager = FindObjectOfType<touchManager>();

    }

    private void Update() 
    {
        if(moveRight)////                           ==========================================CUP MOVING RIGHT==============================================================
        {
            if (!hasCollided)//animate while havent collided
            {
                readyCupToMoveRight();//RESETS START AND END POSITION FOR LERPING

                isMoving = true;
                transform.GetComponent<MeshCollider>().enabled = true;
                anim += Time.deltaTime;
                //anim = anim % 2f;

                transform.position = Parabola(startPos, endPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION

                transform.localEulerAngles = Vector3.Lerp(startRot, endRot, anim * rotationSpeed);

                collisionPos = transform.position;//use as start position to lerp back to start if cant cant cups
                collisionRot = transform.eulerAngles;
            }
            else///                            ------------has Collided with cup------------
            {
                bool allowStack = canAllowStack();

                if (allowStack)//------------------------------------------------------------CAN STACK CUPS
                {
                    anim += Time.deltaTime;//continue to animate cup till the end
                    //anim = anim % 2f;

                    transform.position = Parabola(startPos, endPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION

                    transform.localEulerAngles = Vector3.Lerp(startRot, endRot, anim * rotationSpeed);

                    //print("can");
                    if (transform.position.x <= endPos.x + 0.1f && transform.position.x >= endPos.x - 0.1f)//CHECK IF HAS ALMOST RAECHED END POSITION
                    {
                        //print("has moved right");
                        //endPos.y += 0.4f;//stacking effect?
                        touchManager.blop.Play();
                        transform.position = endPos;
                        transform.localEulerAngles = endRot;
                        anim = 0f;

                        if (readyForNewParent)//PARENT TO NEW CUP WHEN ANIMATION COMPLETED
                        {
                            readyForNewParent = false;
                            //transform.SetParent(rootCup.transform);
                            stackController();
                            //print("COLLIDERS ROOT: " + colliderRootCup.GetComponent<cupMovement>().stackAmount);
                            //colliderRootCup.GetComponent<cupMovement>().stackAmount += this.rootCup.GetComponent<cupMovement>().stackAmount;
                            //print("THIS ROOT: " + this.rootCup.GetComponent<cupMovement>().stackAmount);
                            
                            //print("new parent is - " + CollisionObject.name);
                            isStacked = true;//MBY DISABLE SCRIPT, INSTEAD OF THIS???
                            CollisionObject = null;
                        }

                        hasChanged = false;//FALSE, SO WE ALLOW TO UPDATE START AND NED POSITIONS
                        moveRight = false;
                        isMoving = false;
                        hasCollided = false;

                        //transform.GetComponent<MeshCollider>().enabled = false;
                    }
                }
                else//-----------------------------------------------------------------------CANT STACK CUPS
                {
                    //print("cant");//return cup to its start position and rotation

                    //hasChanged = false;
                    resetAnim();
                    
                    anim += Time.deltaTime;//continue to animate cup till the end
                    //anim = anim % 2f;

                    transform.position = Parabola(collisionPos, startPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION

                    transform.localEulerAngles = Vector3.Lerp(-collisionRot, startRot, anim * rotationSpeed);

                    if(transform.position.x <= startPos.x + 0.1f && transform.position.x >= startPos.x - 0.1f)
                    {
                        //print("back to start positon after failed stack!");
                        touchManager.cupDrop.Play();
                        transform.position = startPos;
                        transform.localEulerAngles = startRot;
                        anim = 0f;

                        hasCollided = false;
                        hasChanged = false;
                        moveRight = false;
                        isMoving = false;

                        CollisionObject = null;

                        revertToStartRotationState();
                    }
                }
            }
        }

        
        if(moveLeft)////                           ==========================================CUP MOVING LEFT==============================================================
        {
            //print("animating");
            if (!hasCollided)
            {
                readyCupToMoveLeft();//get right start and end pos for next right move

                isMoving = true;
                transform.GetComponent<MeshCollider>().enabled = true;

                anim += Time.deltaTime;

                transform.position = Parabola(startPos, endPos, nextStackHeight + 5f, anim * positionSpeed);//POSITION LERP

                transform.localEulerAngles = Vector3.Lerp(startRot, endRot, anim * rotationSpeed);//LERP ROTATION

                collisionPos = transform.position;//use as start position to lerp back to start if cant cant cups
                collisionRot = transform.eulerAngles;
            }
            else///                            ------------has Collided with cup------------
            {
                bool allowStack = canAllowStack();

                if (allowStack)//------------------------------------------------------------CAN STACK CUPS
                {
                    anim += Time.deltaTime;//continue to animate cup till the end

                    transform.position = Parabola(startPos, endPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION

                    transform.localEulerAngles = Vector3.Lerp(startRot, endRot, anim * rotationSpeed);//LERP ROTATION

                    if (transform.position.x <= endPos.x + 0.1f && transform.position.x >= endPos.x - 0.1f)
                    {
                        touchManager.blop.Play();
                        //print("has moved left");
                        //endPos.y += 0.4f;//stacking effect?
                        transform.position = endPos;
                        transform.localEulerAngles = endRot;

                        anim = 0f;

                        if (readyForNewParent)//PARENT TO NEW CUP WHEN ANIMATION COMPLETED
                        {
                            readyForNewParent = false;
                            //transform.SetParent(CollisionObject.transform);
                            stackController();
                            //ransform.SetParent(rootCup.transform);

                            //print("COLLIDERS ROOT: " + colliderRootCup.GetComponent<cupMovement>().stackAmount);
                            //colliderRootCup.GetComponent<cupMovement>().stackAmount += this.rootCup.GetComponent<cupMovement>().stackAmount;
                            //print("THIS ROOT: " + rootCup.GetComponent<cupMovement>().stackAmount);

                            //print("new parent is - " + CollisionObject.name);
                            isStacked = true;//MBY DISABLE SCRIPT, INSTEAD OF THIS???
                            CollisionObject = null;
                        }

                        hasChanged = false;//FALSE, SO WE ALLOW TO UPDATE START AND NED POSITIONS
                        moveLeft = false;
                        isMoving = false;

                        //transform.GetComponent<MeshCollider>().enabled = false;
                    }
                }
                else//-----------------------------------------------------------------------CANT STACK CUPS
                {
                    //print("cant");//return cup to its start position and rotation

                    //hasChanged = false;
                    resetAnim();

                    anim += Time.deltaTime;//continue to animate cup till the end
                    //anim = anim % 2f;

                    transform.position = Parabola(collisionPos, startPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION

                    transform.localEulerAngles = Vector3.Lerp(collisionRot, -startRot, anim * rotationSpeed);//LERP ROTATION

                    if (transform.position.x <= startPos.x + 0.1f && transform.position.x >= startPos.x - 0.1f)
                    {
                        touchManager.cupDrop.Play();
                        //print("back to start positon after failed stack!");
                        transform.position = startPos;
                        transform.localEulerAngles = startRot;
                        anim = 0f;

                        hasChanged = false;
                        moveLeft = false;
                        isMoving = false;
                        hasCollided = false;

                        CollisionObject = null;

                        revertToStartRotationState();
                    }
                }
            }
        }
        
        
        if(moveUp)////                           ==========================================CUP MOVING UP==============================================================
        {
            if (!hasCollided)
            {
                readyCupToMoveUp();

                isMoving = true;
                transform.GetComponent<MeshCollider>().enabled = true;
                anim += Time.deltaTime;

                transform.position = Parabola(startPos, endPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION

                transform.localEulerAngles = Vector3.Lerp(startRot, endRot, anim * rotationSpeed);//LERP ROTATION

                collisionPos = transform.position;//use as start position to lerp back to start if cant cant cups
                collisionRot = transform.eulerAngles;
            }
            else///                            ------------has Collided with cup------------
            {
                bool allowStack = canAllowStack();

                if (allowStack)//------------------------------------------------------------CAN STACK CUPS
                {
                    anim += Time.deltaTime;

                    transform.position = Parabola(startPos, endPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION

                    transform.localEulerAngles = Vector3.Lerp(startRot, endRot, anim * rotationSpeed);//LERP ROTATION

                    if (transform.position.z <= endPos.z + 0.1f && transform.position.z >= endPos.z - 0.1f)//CHECK IF HAS ALMOST RAECHED END POSITION
                    {
                        touchManager.blop.Play();
                        //print("has moved up");
                        //endPos.y += 0.4f;//stacking effect?
                        transform.position = endPos;
                        transform.localEulerAngles = endRot;
                        
                        anim = 0f;

                        if (readyForNewParent)//PARENT TO NEW CUP WHEN ANIMATION COMPLETED
                        {
                            readyForNewParent = false;
                            //transform.SetParent(CollisionObject.transform);
                            stackController();
                            //transform.SetParent(rootCup.transform);

                            //print("COLLIDERS ROOT: " + colliderRootCup.GetComponent<cupMovement>().stackAmount);
                            //colliderRootCup.GetComponent<cupMovement>().stackAmount += this.rootCup.GetComponent<cupMovement>().stackAmount;
                            //print("THIS ROOT: " + this.rootCup.GetComponent<cupMovement>().stackAmount);

                            //print("new parent is - " + CollisionObject.name);
                            isStacked = true;//MBY DISABLE SCRIPT, INSTEAD OF THIS???
                            CollisionObject = null;
                        }

                        hasChanged = false;//FALSE, SO WE ALLOW TO UPDATE START AND NED POSITIONS
                        moveUp = false;
                        isMoving = false;
                        hasCollided = false;

                        //transform.GetComponent<MeshCollider>().enabled = false;
                    }
                }
                else//-----------------------------------------------------------------------CANT STACK CUPS
                {
                    //print("cant");//return cup to its start position and rotation
                    resetAnim();

                    anim += Time.deltaTime;//continue to animate cup till the end

                    transform.position = Parabola(collisionPos, startPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION
                    transform.localEulerAngles = Vector3.Lerp(endRot, -startRot, anim * rotationSpeed);//LERP ROTATION

                    if (transform.position.z <= startPos.z + 0.1f && transform.position.z >= startPos.z - 0.1f)
                    {
                        touchManager.cupDrop.Play();
                        //print("back to start positon after failed stack!");
                        transform.position = startPos;
                        transform.localEulerAngles = startRot;
                        anim = 0f;

                        hasCollided = false;
                        hasChanged = false;
                        moveUp = false;
                        isMoving = false;

                        CollisionObject = null;

                        revertToStartRotationState();
                    }
                }
            }
        }

        if(moveDown)////                           ==========================================CUP MOVING DOWN==============================================================
        {
            if(!hasCollided)
            {
                readyCupToMoveDown();

                isMoving = true;
                transform.GetComponent<MeshCollider>().enabled = true;
                anim += Time.deltaTime;
                //anim = anim % 2f;

                transform.position = Parabola(startPos, endPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION

                transform.localEulerAngles = Vector3.Lerp(startRot, endRot, anim * rotationSpeed);//LERP ROTATION
                //print("start rotation: " + startRot);
                //print("end rotation: " + endRot);

                collisionPos = transform.position;//use as start position to lerp back to start if cant cant cups
                collisionRot = transform.eulerAngles;
            }
            else///                            ------------has Collided with cup------------
            {
                bool allowStack = canAllowStack();

                if (allowStack)//------------------------------------------------------------CAN STACK CUPS
                {
                    anim += Time.deltaTime;//continue to animate cup till the end

                    transform.position = Parabola(startPos, endPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION

                    transform.localEulerAngles = Vector3.Lerp(startRot, endRot, anim * rotationSpeed);

                    if (transform.position.z <= endPos.z + 0.1f && transform.position.z >= endPos.z - 0.1f)//CHECK IF HAS ALMOST RAECHED END POSITION
                    {
                        touchManager.blop.Play();
                        //print("has moved down");
                        //endPos.y += 0.4f;//stacking effect?
                        transform.position = endPos;
                        transform.localEulerAngles = endRot;
                        anim = 0f;

                        if (readyForNewParent)//PARENT TO NEW CUP WHEN ANIMATION COMPLETED
                        {
                            readyForNewParent = false;
                            //transform.SetParent(CollisionObject.transform);

                            //transform.SetParent(rootCup.transform);
                            stackController();
                            //print("COLLIDERS ROOT: " + colliderRootCup.GetComponent<cupMovement>().stackAmount);
                            //colliderRootCup.GetComponent<cupMovement>().stackAmount += this.rootCup.GetComponent<cupMovement>().stackAmount;
                            //print("THIS ROOT: " + this.rootCup.GetComponent<cupMovement>().stackAmount);

                            //print("new parent is - " + CollisionObject.name);
                            isStacked = true;//MBY DISABLE SCRIPT, INSTEAD OF THIS???

                            //addToStack(gameObject, CollisionObject);
                            CollisionObject = null;
                        }

                        hasChanged = false;//FALSE, SO WE ALLOW TO UPDATE START AND NED POSITIONS
                        moveDown = false;
                        isMoving = false;
                        hasCollided = false;

                        //transform.GetComponent<MeshCollider>().enabled = false;
                    }
                }
                else//-----------------------------------------------------------------------CANT STACK CUPS
                {
                    //print("cant");//return cup to its start position and rotation
                    resetAnim();
                    
                    anim += Time.deltaTime;//continue to animate cup till the end

                    transform.position = Parabola(collisionPos, startPos, nextStackHeight + 5f, anim * positionSpeed);//LERP POSITION

                    transform.localEulerAngles = Vector3.Lerp(endRot, startRot, anim * rotationSpeed);

                    if(transform.position.z <= startPos.z + 0.1f && transform.position.z >= startPos.z - 0.1f)
                    {
                        touchManager.cupDrop.Play();
                        //print("back to start positon after failed stack!");
                        transform.position = startPos;
                        transform.localEulerAngles = startRot;
                        anim = 0f;

                        hasCollided = false;
                        hasChanged = false;
                        moveDown = false;
                        isMoving = false;

                        CollisionObject = null;

                        revertToStartRotationState();
                    }
                }
            }
        }
        /*TESTING AREA?
        */
        if (standingNormal)
        {
            //print( gameObject.name + " rotation state is NORMAL");
            //transform.localEulerAngles = Vector3.Lerp(startRot, upsideRotation, anim);
        }
        else
        {
            //print( gameObject.name + " rotation state is FLIPPED");
            //transform.localEulerAngles = Vector3.Lerp(startRot, normalRotation, anim);
        }
        
        if(stackAmount == touchManager.numberOfStacksForWin)
        {
            touchManager.hasWon = true;
        }
    }


    void readyCupToMoveRight()
    {
        if(!hasChanged)
        {   
            touchManager.jump.Play();
            //readyCupRotation();
            rotateCupToRight();
            flipCupRotationState();
            hasChanged = true;///TURN ON, SO WE UPDATE POSITION ONLY ONCE!!!
            startPos = transform.position;
            endPos = new Vector3(transform.position.x + 2f, transform.position.y + ((nextStackHeight * 0.2f) + (stackAmount * 0.2f)), transform.position.z);
            print("RIGHT: nextStact: currentStack [" + nextStackHeight + ": " + stackAmount + "] and end pos [" + endPos + "]");
        }
    }

    void readyCupToMoveLeft()
    {
        if(!hasChanged)
        {   
            touchManager.jump.Play();
            //readyCupRotation();
            rotateCupToLeft();
            flipCupRotationState();
            hasChanged = true;///TURN ON, SO WE UPDATE POSITION ONLY ONCE!!!
            startPos = transform.position;
            endPos = new Vector3(transform.position.x - 2f, transform.position.y + ((nextStackHeight * 0.2f) + (stackAmount * 0.2f)), transform.position.z);
            print("LEFT: nextStact: currentStack [" + nextStackHeight + ": " + stackAmount + "] and end pos [" + endPos + "]");
        }
    }

    void readyCupToMoveUp()
    {
        if(!hasChanged)
        {   
            touchManager.jump.Play();
            rotateCupToUp();
            flipCupRotationState();
            hasChanged = true;///TURN ON, SO WE UPDATE POSITION ONLY ONCE!!!
            startPos = transform.position;
            endPos = new Vector3(transform.position.x, transform.position.y + ((nextStackHeight * 0.2f) + (stackAmount * 0.2f)), transform.position.z + 2f);
            print("UP: nextStact: currentStack [" + nextStackHeight + ": " + stackAmount + "] and end pos [" + endPos + "]");
        }
    }

    void readyCupToMoveDown()
    {
        if(!hasChanged)
        {   
            touchManager.jump.Play();
            rotateCupToDown();
            flipCupRotationState();
            hasChanged = true;///TURN ON, SO WE UPDATE POSITION ONLY ONCE!!!
            startPos = transform.position;
            endPos = new Vector3(transform.position.x, transform.position.y + ((nextStackHeight * 0.2f) + (stackAmount * 0.2f)), transform.position.z - 2f);
            print("DOWN: nextStact: currentStack [" + nextStackHeight + ": " + stackAmount + "] and end pos [" + endPos + "]");
        }
    }

    void flipCupRotationState()
    {
        standingNormal = !standingNormal;

        //startRot = transform.localEulerAngles;
        if(standingNormal)
        {
            print( gameObject.name + " rotation state is NORMAL");
        }else
        {
            print( gameObject.name + " rotation state is FLIPPED");
        }
        
    }

    void revertToStartRotationState()
    {
        standingNormal = !standingNormal;
    }

    void resetAnim()
    {
        if(hasChanged)
        {   
            touchManager.cantStack.Play();
            anim = 0f;
            hasChanged = false;///TURN ON, SO WE UPDATE POSITION ONLY ONCE!!!
            print(" anim reset");
        }
    }

    void rotateCupToRight()
    {
        startRot = transform.localEulerAngles;
        startRot.y = 0f;

        endRot = new Vector3(transform.localEulerAngles.x, 0f, transform.localEulerAngles.z - 180f);
    }

    void rotateCupToLeft()
    {
        startRot = transform.localEulerAngles;
        startRot.y = 0f;

        endRot = new Vector3(transform.localEulerAngles.x, 0f, startRot.z + 180f);
    }

    void rotateCupToUp()
    {
        startRot = transform.localEulerAngles;
        startRot.y = 0f;

        endRot = new Vector3( startRot.x + 180f, 0f, transform.localEulerAngles.z);
    }

    void rotateCupToDown()
    {
        startRot = transform.localEulerAngles;
        startRot.y = 0f;

        endRot = new Vector3( startRot.x - 180f, 0f, transform.localEulerAngles.z);
    }

    public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }

    void OnTriggerEnter(Collider collider)//CHECK COLLISION WITH NEW CUP
    {
        if (isMoving)
        {
            if (collider.gameObject.CompareTag("Cup"))
            {
                //print("new collision " + collider.name);
                
                //transform.SetParent(collider.transform);
                //collider.gameObject.GetComponent<cupMovement>().stackAmount += stackAmount;
                hasCollided = true;

                CollisionObject = collider.gameObject;//cup has collided with this object, but

                
                //rootCup = collider.gameObject.GetComponent<cupMovement>().rootCup;//assign this cup to collisions cup rootCup
                //rootCup.GetComponent<cupMovement>().stackAmount += 1;//
                //stackAmount = 0;
                
                //twas here
                


                //print("root cups cup amount: " + rootCup.GetComponent<cupMovement>().stackAmount);
                //print("cupamount : " + rootCup.GetComponent<cupMovement>().stackAmount);

                readyForNewParent = true;
                //this.enabled = false;
                //print(collider.name + " has " + collider.gameObject.GetComponent<cupMovement>().stackAmount + " cups");
                //print(gameObject.name + " has " + stackAmount + " cups");


            }
        }
    }

    public bool canAllowStack()
    {
        if (CollisionObject != null)//HAVE CUPS COLLIDED YET?
        {
            if (CollisionObject.GetComponent<cupMovement>().rootCup.GetComponent<cupMovement>().standingNormal == standingNormal)//COMPARE CUP ROTATION STATES
            {
                //print("allow stacking!!!");
                return true;
            }
            else
            {
                //print("DONT allow stacking!!!");
                return false;
            }
        }else//if there is no collision yet, return false
        {
            return false;
        }
    }

    void stackController()
    {
        //STACK CONTROLLLLLLLL!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        collidersRootCup = CollisionObject.GetComponent<cupMovement>().rootCup;//get collisions root cup
        //collidersRootCup.GetComponent<cupMovement>().stackAmount += stackAmount;//transfer all stacks from this root to collision root cup!

        if (transform.childCount > 0)
        {
            int i = 0;

            List<GameObject> allChildren = new List<GameObject>();

            foreach (Transform child in transform)//get all children
            {
                allChildren.Add(child.gameObject);

                i++;
            }

            int test = 0;
            foreach (GameObject go in allChildren)
            {
                test++;

                go.GetComponent<cupMovement>().rootCup = collidersRootCup;
                go.transform.SetParent(collidersRootCup.transform);
                print("test: " + test);
            }

            collidersRootCup.GetComponent<cupMovement>().stackAmount += stackAmount;//transfer stacks
            stackAmount = 0;
            rootCup = collidersRootCup;
            transform.SetParent(rootCup.transform);//when all children are parented to rootCup, parent this root to collidersRoot
            print("TEST: all children has been counted: " + test);

            correctStackingEffect();
        }
        else
        {
            print("TEST: 1 CUP ONLY " + transform.name);
            collidersRootCup.GetComponent<cupMovement>().stackAmount += stackAmount;
            stackAmount = 0;
            transform.SetParent(collidersRootCup.transform);
            rootCup = collidersRootCup;

            correctStackingEffect();//rearrange all colliders children
        }
    }

    void correctStackingEffect()//correct???
    {
        List<GameObject> allChildren = new List<GameObject>();
        int i = 0;

        foreach (Transform child in collidersRootCup.transform)//get all children of colliders root cup
        {
            i++;
            allChildren.Add(child.gameObject);
            //print("colliders new child is: " + child.name);
            child.transform.position = new Vector3(collidersRootCup.transform.position.x, collidersRootCup.transform.position.y + (i * 0.4f), collidersRootCup.transform.position.z);
        }
    }

    public void ResetAllStats()
    {
        transform.GetComponent<MeshCollider>().isTrigger = true;
        transform.GetComponent<TrailRenderer>().enabled = false;
        
        transform.position = OriginalPos;
        standingNormal = OriginalState;
        rootCup = OriginalRoot;

        if(standingNormal)
        {
            transform.localEulerAngles = normalRotation;
        }else
        {
            transform.localEulerAngles = upsideRotation;
        }

        transform.SetParent(OriginalPlatform.transform);

        stackAmount = 1;

        hasChanged = false;//FALSE, SO WE ALLOW TO UPDATE START AND NED POSITIONS
        moveRight = false;
        isMoving = false;
        hasCollided = false;
        collidersRootCup = null;
        CollisionObject = null;

        transform.GetComponent<TrailRenderer>().enabled = true;
    }

    private void addToStack(GameObject currentGameObject, GameObject collisionObject)
    {
        cupMovement currentCup = currentGameObject.GetComponent<cupMovement>();
        print("current stack before: " + currentCup.cupStack.Count);
        //currentCup.cupStack.RemoveAt(currentCup.cupStack.Count - 1);
        print("current stack now: " + currentCup.cupStack.Count);

        int i = 0;
        cupMovement collisionCup = collisionObject.GetComponent<cupMovement>();
        foreach(GameObject go in currentCup.cupStack)
        {
            i++;
            collisionCup.cupStack.Add(go);
            //currentCup.cupStack.Remove(go);
            print(i + ". current cup count: " + currentCup.cupStack.Count + " -> " + collisionCup.cupStack.Count);
        }

    }
}
