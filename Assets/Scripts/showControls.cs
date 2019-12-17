using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showControls : MonoBehaviour
{
    public GameObject firstControls;
    public bool showFirstControls;

    private touchManager touchManager;

    void Start()
    {
        showFirstControls = false;
        firstControls.SetActive(false);
        touchManager = FindObjectOfType<touchManager>();
        StartCoroutine(showStartControls(3f));
    }

    void Update()
    {
        
    }

    IEnumerator showStartControls(float time)
    {
        yield return new WaitForSeconds(time);
        firstControls.SetActive(true);
        touchManager.canTouch = true;
    }
}
