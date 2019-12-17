using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    float rot;

    // Start is called before the first frame update
    void Start()
    {
        rot = 0;
    }

    // Update is called once per frame
    void Update()
    {
        rot += 60f * Time.deltaTime;
        transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, new Vector3(0f, 0f, 180f), 0.7f * Time.deltaTime);
    }
}
