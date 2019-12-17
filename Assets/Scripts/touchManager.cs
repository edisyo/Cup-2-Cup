using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class touchManager : MonoBehaviour, IEndDragHandler {

    [HideInInspector] public bool canTouch;

    [Header ("UI ELEMENTS")]
    public GameObject firstControls;
    public GameObject gameLogo;
    public GameObject retryButton;
    public GameObject levelName;
    public GameObject continueText;
    public GameObject confeti;
    public GameObject stars;
    public GameObject shopButton;
    public GameObject shopPanel;

    private Animator cameraAnimator;
    private bool gameStarted;
    private Vector2 touchStart;
    private Vector2 touchEnd;
    private bool touchEnded;
    private bool isDragging;
    private float touchDeltaX;
    private float touchDeltaY;

    private coinManager coinManager;

    [HideInInspector] public bool hasWon;
    

    [HideInInspector] public int numberOfCups;
    

    [Header ("Touch options")]
    public bool detectSwipeOnlyAfterRelease;
    public float SWIPE_THRESHOLD = 20f;
    public int numberOfStacksForWin;//for next levels???

    [HideInInspector] public GameObject activeCup;

    private bool winController;

    [Header ("Sound")]
    public AudioSource victory;
    public AudioSource confetti;
    public AudioSource blop;
    public AudioSource cupDrop;
    public AudioSource cantStack;
    public AudioSource jump;

    private bool inputReady;
    private bool isMoving;

    void Start() 
    {
        retryButton.SetActive(false);
        levelName.SetActive(false);
        continueText.SetActive(false);
        confeti.SetActive(false);
        stars.SetActive(false);
        shopButton.SetActive(false);

        gameStarted = false;
        canTouch = false;
        touchEnded = false;
        isDragging = false;

        activeCup = null;

        cameraAnimator = Camera.main.GetComponent<Animator>();

        numberOfCups = 0;

        hasWon = false;
        winController = false;

        coinManager = FindObjectOfType<coinManager>();
        coinManager.turnOff();
        inputReady = false;
        isMoving = false;
    }

    void Update()
    {
        if (canTouch)
        {
            //print("waiting for tap");
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0) && !gameStarted) //START GAME FOR THE FIRST TIME
            {
                //print("touched screen");
                cameraAnimator.SetBool("startGame", true);
                startGame();
            }
        }

        if (gameStarted)
        {
            //print("Game started: ");
            foreach (Touch touch in Input.touches)//LOGIC THAT PROCESSES ALL TOUCHES
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position); //TOUCH POSITION FROM SCREEN TO RAY
                RaycastHit hit;
                int layer_mask = LayerMask.GetMask("Platform");

                if (Physics.Raycast(ray, out hit, 999f, layer_mask) && !EventSystem.current.IsPointerOverGameObject())//Is touch over UI?
                {
                    print("touched cup");
                    if (touch.phase == TouchPhase.Began && Input.GetMouseButtonDown(0) && activeCup == null)//FINGER DOWN
                    {
                        //print("TOUCH BEGAN");
                        touchStart = touch.position;
                        touchEnd = touch.position;
                        activeCup = hit.collider.gameObject;
                        activeCup.GetComponent<Platform>().setToHighlighColor();
                        //print("Touch Start: " + touchStart);
                    }

                    if (touch.phase == TouchPhase.Moved)//FINGER MOVING
                    {
                        //print("TOUCH MOVING");
                        isDragging = true;

                        if (!detectSwipeOnlyAfterRelease)
                        {
                            touchEnd = touch.position;
                            checkSwipeDirection();
                        }
                    }
                }

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled || Input.GetMouseButtonUp(0) && isDragging)//FINGER UP, EVEN IF NOT OVER OBJECT ANYMORE
                {
                    //print("TOUCH ENDED");
                    touchEnd = touch.position;
                    activeCup.GetComponent<Platform>().setToNormalColor();
                    checkSwipeDirection();
                    isDragging = false;

                    activeCup = null;
                }
            }
        }

        if (hasWon)///================================================================================HAS WON=======================================================================
        {
            playSound();
            gameStarted = false;
            retryButton.SetActive(false);
            levelName.SetActive(false);
            
            confeti.SetActive(true);
            stars.SetActive(true);
            shopButton.SetActive(true);

            print("has won");

            //coinManager.turnOff();
            //StartCoroutine(delayInput(1f));

            if (inputReady)
            {
                print("input ready");
                if (!shopPanel.activeSelf)
                {
                    print("panel inactive");
                    if (Input.touchCount > 0)
                    {
                        if (IsPointerOverUIObject())
                        {
                            print("selected object: " + EventSystem.current.currentSelectedGameObject);
                            return;
                        }

                        restartGame();
                        startGame();
                    }
                }

            }

            
        }
    }

    void playSound()
    {
        if(!winController)
        {
            inputReady = false;
            StartCoroutine(delayInput(3f));
            
            coinManager.addCoins(numberOfStacksForWin);

            winController = true;
            victory.Play();
            confetti.Play();
        }
    }

    IEnumerator delayInput(float time)
    {
        yield return new WaitForSeconds(time);
        continueText.SetActive(true);
        inputReady = true;
    }

    void resetTriggers()
    {
        GameObject[] allCups;
        allCups = GameObject.FindGameObjectsWithTag("Cup");
        foreach (GameObject cup in allCups)
        {
            cup.GetComponent<MeshCollider>().isTrigger = true;
        }

            
    }

    public void checkSwipeDirection()
    {
        touchDeltaX = touchEnd.x - touchStart.x;
        touchDeltaY = touchEnd.y - touchStart.y;

        //CHECK VERTICAL SWIPE
        if(verticalMove() > SWIPE_THRESHOLD && verticalMove() > horizontalMove())
        {
            if(touchDeltaY > 0)//SWIPE UP
            {
                onSwipeUp();
            }
            else if (touchDeltaY < 0)//SWIPE DOWN
            {
                onSwipeDown();
            }
        }
        else if(horizontalMove() > SWIPE_THRESHOLD && horizontalMove() > verticalMove())//CHECK HORIZONTAL SWIPE
        {
            if(touchDeltaX > 0)//SWIPE RIGHT
            {
                onSwipeRight();
            }
            else if(touchDeltaX < 0)//SWIPE LEFT
            {
                onSwipeLeft();
            }
        }
    }

    float verticalMove()
    {
        return Mathf.Abs(touchDeltaY);
    }

    float horizontalMove()
    {
        return Mathf.Abs(touchDeltaX);
    }

    void onSwipeUp()
    {
        print("swipe up");

        bool canSwipe;
        canSwipe = activeCup.GetComponent<Platform>().checkUpPlatformForCups();

        if(canSwipe && !isMoving)//there are platform with cups on the right side
        {
            isMoving = true;
            StartCoroutine(isMovingDelay(0.7f));
            //print("can swipe");
            if(!activeCup.GetComponentInChildren<cupMovement>().isMoving)
            {
                //print("can move");
                activeCup.GetComponentInChildren<cupMovement>().moveUp = true;
                activeCup.GetComponentInChildren<cupMovement>().nextStackHeight = activeCup.GetComponent<Platform>().numberOfCups;
            }
            else
            {
                //print("is already moving");
            }
        }
    }

    void onSwipeDown()
    {
        print("swipe down");
        
        bool canSwipe;
        canSwipe = activeCup.GetComponent<Platform>().checkDownPlatformForCups();

        if(canSwipe && !isMoving)//there are platform with cups on the right side
        {
            //print("can swipe");
            if(!activeCup.GetComponentInChildren<cupMovement>().isMoving)
            {
                isMoving = true;
                StartCoroutine(isMovingDelay(0.7f));
                //print("can move");
                activeCup.GetComponentInChildren<cupMovement>().moveDown = true;
                activeCup.GetComponentInChildren<cupMovement>().nextStackHeight = activeCup.GetComponent<Platform>().numberOfCups;
            }
            else
            {
                //print("is already moving");
            }
        }
    }

    void onSwipeLeft()
    {
        print("swipe left");
        bool canSwipe;
        canSwipe = activeCup.GetComponent<Platform>().checkLeftPlatformForCups();

        if(canSwipe && !isMoving)//there are platform with cups on the right side
        {
            //print("can swipe");
            if(!activeCup.GetComponentInChildren<cupMovement>().isMoving)
            {
                isMoving = true;
                StartCoroutine(isMovingDelay(0.7f));
                //print("can move");
                activeCup.GetComponentInChildren<cupMovement>().moveLeft = true;
                activeCup.GetComponentInChildren<cupMovement>().nextStackHeight = activeCup.GetComponent<Platform>().numberOfCups;
            }
            else
            {
                //print("is already moving");
            }
        }
    }

    void onSwipeRight()
    {
        print("swipe right");
        
        bool canSwipe;
        canSwipe = activeCup.GetComponent<Platform>().checkRightPlatformForCups();

        if(canSwipe && !isMoving)//there are platform with cups on the right side
        {
            //print("can swipe");
            if(!activeCup.GetComponentInChildren<cupMovement>().isMoving)
            {
                isMoving = true;
                StartCoroutine(isMovingDelay(0.7f));
                //print("can move");
                activeCup.GetComponentInChildren<cupMovement>().moveRight = true;
                activeCup.GetComponentInChildren<cupMovement>().nextStackHeight = activeCup.GetComponent<Platform>().numberOfCups;
                
            }
            else
            {
                //print("is already moving");
            }
        }
    }

    IEnumerator isMovingDelay(float time)
    {
        yield return new WaitForSeconds(time);
        isMoving = false;
    }

    public void OnEndDrag(PointerEventData eventData)//doesnt work, cant figure out why..
    {
        if(isDragging)
        {
            touchEnd = Input.GetTouch(0).position;
            print("Touch End: " + touchEnd);

            isDragging = false;
            activeCup = null;
        }

        touchEnd = Input.GetTouch(0).position;
        print("Touch End: " + touchEnd);

        isDragging = false;
        activeCup = null;
        
    }

    void startGame () 
    {
        hasWon = false;
        gameStarted = true;
        canTouch = false;
        winController = false;

        firstControls.SetActive (false);
        gameLogo.SetActive (false);
        retryButton.SetActive (true);
        levelName.SetActive (true);
        continueText.SetActive (false);
        confeti.SetActive(false);
        stars.SetActive(false);
        shopButton.SetActive(false);

        coinManager.turnOn();
    }

    public void restartGame()
    {
        GameObject[] allCups;
        allCups = GameObject.FindGameObjectsWithTag("Cup");
        foreach (GameObject cup in allCups)
        {
            cup.GetComponent<cupMovement>().ResetAllStats();
        }

        foreach (GameObject cup in allCups)
        {
            cup.GetComponent<cupMovement>().ResetAllStats();
        }
        
    }

    void checkForWin()
    {
        GameObject[] allPlatforms;
        List<GameObject> plat = new List<GameObject>();
        plat.AddRange(GameObject.FindGameObjectsWithTag("Platform"));

        //allPlatforms = GameObject.FindGameObjectsWithTag("Platform");//count all platforms, who has children CUP.
        foreach (GameObject platform in plat)
        {
            //plat.GetComponent<cupMovement>().ResetAllStats();
        }
    }

    public void clicked_retryButton()
    {
        retryButton.GetComponent<Animator>().SetTrigger("hasClicked");
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

}