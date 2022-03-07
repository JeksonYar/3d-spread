using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseAnimation : MonoBehaviour
{
    private bool paused = true;
    public KeyCode stopKey;

    // Start is called before the first frame update
    void Start()
    {      
        
    }

    // Update is called once per frame
    void Update()
    {
        if (paused)
        {
            if (Input.GetKeyUp(stopKey))
            {
                Time.timeScale = 0f;
                paused = false;
            }
        }

        else
        {
            if (Input.GetKeyUp(stopKey))
            {
                Time.timeScale = 1f;
                paused = true;
            }
        }
    }
}
