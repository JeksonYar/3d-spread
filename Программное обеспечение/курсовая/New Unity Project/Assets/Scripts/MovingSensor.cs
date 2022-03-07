using UnityEngine;
using UnityEngine.UI;

public class MovingSensor : MonoBehaviour
{
    //вершины сечения (координаты по x и y)
    public Slider slider;

    public GameObject capsule;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(-Vector3.left * 2f);  
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(-Vector3.right * 2f); 
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(-Vector3.up * 2f);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(-Vector3.down * 2f);  
        }

        else 
        {
            Vector3 position;
            position.x = transform.position.x;
            position.y = transform.position.y;
            position.z = slider.value;
            transform.position = position;
        }
    }
}
