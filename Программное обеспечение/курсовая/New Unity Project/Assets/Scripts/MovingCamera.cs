using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovingCamera : MonoBehaviour
{
    public Camera mainCamera;

    public Dropdown dropdown;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Rotate()
    {
        int val = dropdown.value;

        if (val == 0)
        {
            mainCamera.transform.localPosition = new Vector3(510, 90, 780);
            mainCamera.transform.rotation = Quaternion.Euler(0, 218, 0);
        }
        
        if (val == 1)
        {
            mainCamera.transform.localPosition = new Vector3(85, 100, 800);
            mainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        if (val == 2)
        {
            mainCamera.transform.localPosition = new Vector3(85, 900, 200);
            mainCamera.transform.rotation = Quaternion.Euler(-270, 180, 0);
        }
    }
}
