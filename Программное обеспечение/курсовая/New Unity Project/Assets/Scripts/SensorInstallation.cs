using System.Collections.Generic;
using UnityEngine;

public class SensorInstallation : MonoBehaviour
{
    //вершины сечения
    public static Vector2[] vertices = new Vector2[18];

    //стороны сечения
    public static List<Vector2[]> sides = new List<Vector2[]>();

    //коэффициенты уравнений сторон сечения
    public static float[] kSides = new float[18];
    public static float[] bSides = new float[18];

    //датчик
    public GameObject sensor;

    void Start()
    {
        //запись в массив вершин сечения
        Vector2 forAdd = new Vector2(10, 10);
        vertices[0] = forAdd;
        forAdd = new Vector2(10, 20);
        vertices[1] = forAdd;
        forAdd = new Vector2(65, 32);
        vertices[2] = forAdd;
        forAdd = new Vector2(74, 54);
        vertices[3] = forAdd;
        forAdd = new Vector2(74, 140);
        vertices[4] = forAdd;
        forAdd = new Vector2(70, 145);
        vertices[5] = forAdd;
        forAdd = new Vector2(48, 155);
        vertices[6] = forAdd;
        forAdd = new Vector2(48, 175);
        vertices[7] = forAdd;
        forAdd = new Vector2(60, 190);
        vertices[8] = forAdd;
        forAdd = new Vector2(110, 190);
        vertices[9] = forAdd;
        forAdd = new Vector2(122, 175);
        vertices[10] = forAdd;
        forAdd = new Vector2(122, 155);
        vertices[11] = forAdd;
        forAdd = new Vector2(100, 145);
        vertices[12] = forAdd;
        forAdd = new Vector2(96, 145);
        vertices[13] = forAdd;
        forAdd = new Vector2(96, 54);
        vertices[14] = forAdd;
        forAdd = new Vector2(105, 32);
        vertices[15] = forAdd;
        forAdd = new Vector2(160, 20);
        vertices[16] = forAdd;
        forAdd = new Vector2(160, 10);
        vertices[17] = forAdd;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //запись сторон
    public void Sides()
    {
        sides.Clear();
        for (int i = 0; i < 17; i++)
        {
            Vector2[] pair = new Vector2[2];
            pair[0] = vertices[i];
            pair[1] = vertices[i + 1];
            sides.Add(pair);
        }
        //запись в массив стороны, соединяющей первую и последнюю точки
        Vector2[] point = new Vector2[2];
        point[0] = vertices[0];
        point[1] = vertices[17];
        sides.Add(point);
    }

    //вычисление коэффициентов уравнений сторон
    public void EquationCoeff()
    {        
        for (int i = 0; i < 18; i++)
        {
            kSides[i] = (float)(sides[i][1].y - sides[i][0].y) / (sides[i][1].x - sides[i][0].x);
            bSides[i] = sides[i][0].y - ((float)(sides[i][1].y - sides[i][0].y) 
                        / (sides[i][1].x - sides[i][0].x)) * sides[i][0].x;
        }
    }

    //установка датчика
    public void Install() 
    {
        Sides();
        EquationCoeff();

        //начальное положение датчика
        float startX = sensor.transform.position.x;
        float startY = sensor.transform.position.y;

        //результат работы метода - точка установки по осям x и y
        float xSensor = 0;
        float ySensor = 0;

        //промежуточные результаты
        float x = 0;
        float y = 0;
        float minDistance = 10000000;
        float alpha;

        for (int i = 0; i < 18; i++)
        {
            //угол, под которым проходит текущая сторона
            if (sides[i][0].x == sides[i][1].x)
            {
                alpha = 90;
            }
            else if (sides[i][0].y == sides[i][1].y)
            {
                alpha = 0;
            }
            else
            {
                alpha = (float)(Mathf.Atan(kSides[i]) * 180) / Mathf.PI;
            }
            alpha %= 360;
            if (alpha >= 180)
                alpha -= alpha - 180;
            if (alpha < 0)
                alpha += 180;
            //угол, соответствующий прямой, перпендикулярной к текущей стороне
            float beta = alpha + 90;
            if (beta >= 180)
                beta -= 180;
            
            //коэффициенты прямой, перпендикулярной текущей в цикле стороне
            float kOrt = (float)Mathf.Tan((float)(beta * Mathf.PI) / 180);
            float bOrt = sensor.transform.position.y - kOrt * startX;

            //точка пересечения текущей стороны и перпендикуляра, проведенного к ней от датчика
            float KSIDES = kSides[i];
            float test = (kSides[i] - kOrt);

            //проверка условия парллельности осям
            if (beta == 0)
            {
                x = sides[i][0].x;
                y = startY;
            }

            else if (beta == 90)
            {
                x = startX;
                y = sides[i][0].y;
            }

            else
            {
                x = (float)(bOrt - bSides[i]) / (kSides[i] - kOrt);
                y = (float)(kOrt * (bOrt - bSides[i])) / (kSides[i] - kOrt) + bOrt;
            }

            //если расстояние меньше текущего минимума
            if (Mathf.Sqrt(Mathf.Pow(Mathf.Abs(x - startX), 2) +
                Mathf.Pow(Mathf.Abs(y - startY), 2)) < minDistance)
            {
                if (((x <= sides[i][0].x && x >= sides[i][1].x) || (x >= sides[i][0].x && x <= sides[i][1].x))
                    && ((y <= sides[i][0].y && y >= sides[i][1].y) || (y >= sides[i][0].y && y <= sides[i][1].y)))
                {
                    minDistance = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(x - startX), 2)
                              + Mathf.Pow(Mathf.Abs(y - startY), 2));
                    xSensor = x;
                    ySensor = y;
                }
            }
            else {  }
        }
        Vector3 sensorInstallation;
        sensorInstallation.x = xSensor;
        sensorInstallation.y = ySensor;
        sensorInstallation.z = sensor.transform.position.z;
        sensor.transform.position = sensorInstallation;
    }
}
