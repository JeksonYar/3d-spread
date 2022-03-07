using UnityEngine;

public class MovingVector : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform startPosition;
    public Vector3 EndPoint;
    public float speed = 0.01f;

    public Vector3[] vectors = new Vector3[4];

    //здесь будет вычисляться очередная точка
    public void EndPointInit()
    {
        Vector3 result;
        result = vectors[Random.Range(0, 3)];
        EndPoint = result;
    }

    public void Test()
    {
        Vector3 point1;
        point1.x = 95;
        point1.y = 22;
        point1.z = 140;

        Vector3 point2;
        point2.x = 64;
        point2.y = 90;
        point2.z = 280;

        Vector3 point3;
        point3.x = 86;
        point3.y = 180;
        point3.z = 80;

        Vector3 point4;
        point4.x = 112;
        point4.y = 145;
        point4.z = 210;

        vectors[0] = point1;
        vectors[1] = point2;
        vectors[2] = point3;
        vectors[3] = point4;

    }

    //проврека, не встретил ли луч конечную точку
    public bool Checker(Vector3 currentPosition)
    {
        if (currentPosition == EndPoint)
        {
            return false;
        }
        else return true;
    }

    //здесь будет инициализирована точка начала движения и первая точка столкновения
    void Start()
    {
        Test();
        transform.position = startPosition.position;
        EndPoint.x = 0;
        EndPoint.y = 0;
        EndPoint.z = 50;
    }



    // Update is called once per frame
    void Update()
    {
        if (Checker(transform.position))
            transform.position = Vector3.MoveTowards(transform.position, EndPoint, 1.5f);    
        else EndPointInit();
    }
}
