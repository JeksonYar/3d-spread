using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartAnimation : MonoBehaviour
{
    //текстовые поля для заполнения угла ввода
    public InputField X, Y, Z;

    //датчик
    public GameObject sensor;

    //следующая вычисленная точка
    public Vector3 followingPoint;

    //точка, оставляющая след
    public GameObject capsule;

    //ответ на то, установлен ли уже датчик и нужно ли рисовать след
    public bool answer;

    public TrailRenderer trail;

    //коэффициенты прямой для луча
    float kXy, bXy, kXz, bXz;

    //угол ввода
    float angleXY;
    float angleXZ;

    //координаты точки пересечения прямой и стороны
    float intersectX;
    float intersectY;
    float intersectZ;

    //координаты точки пересечения прямой и стороны
    float intersectXprev;
    float intersectYprev;
    float intersectZprev;

    int index;

    //пересчет угла
    bool recountXY;
    bool direction;
    bool isStart = true;

    // Start is called before the first frame update
    void Start()
    {
        answer = false;     
    }

    // Update is called once per frame
    void Update()
    {
        //пока не нажата кнопка старт
        if (!answer) 
        { 
            capsule.transform.position = sensor.transform.position;
            trail.enabled = false;
        }

        //после запуска анимации
        else
        {
            trail.enabled = true;
            if (Checker(capsule.transform.position, followingPoint))
                capsule.transform.position = Vector3.MoveTowards(capsule.transform.position, followingPoint, 0.5f);
            else
            {
                CoeffXY();
                CoeffXZ();
                Intersect();              
            }
        }
    }

    public void Angle()
    {
        //координаты вектора - угла ввода
        float angleX, angleY, angleZ;

        angleX = float.Parse(X.text.ToString());
        angleY = float.Parse(Y.text.ToString());
        angleZ = float.Parse(Z.text.ToString());

        //угол для проекции XY
        float angleInRadian = angleY / angleX;
        float alpha = (Mathf.Atan(angleInRadian) * 180) / Mathf.PI;

        alpha %= 360;
        if (alpha >= 180)
            alpha -= 180;
        if (alpha < 0)
            alpha += 180;
        angleXY = alpha;


        //угол для проекции XZ
        angleInRadian = angleZ / angleX;
        alpha = (Mathf.Atan(angleInRadian) * 180) / Mathf.PI;
        alpha %= 360;
        if (alpha >= 180)
            alpha -= 180;
        if (alpha < 0)
            alpha += 180;
        angleXZ = alpha;
    }

    public void StartOfAnimation()
    {
        answer = true;
        Angle();
        CoeffXY();
        CoeffXZ();

        intersectXprev = capsule.transform.position.x;
        intersectYprev = capsule.transform.position.y;
        intersectZprev = capsule.transform.position.z;

        Intersect();
        followingPoint.x = intersectX;
        followingPoint.y = intersectY;
        followingPoint.z = intersectZ;
    }

    //проверка на то, достиг ли сигнал стороны
    public bool Checker(Vector3 currentPosition, Vector3 endPoint)
    {
        if (currentPosition == endPoint)
        {
            return false;
        }
        else return true;
    }


    private float AngleOfReflection(Vector3 p1, Vector3 p2)
    {
        float k;
        float alpha;
        if (p1.x == p2.x || p1.y == p2.y)
        {
            alpha = -1 * angleXY;
        }

        else
        {
            k = ((float)(p2.y - p1.y) / (p2.x - p1.x));
            float beta = (float)(Mathf.Atan(k) * 180) / Mathf.PI;
            alpha = 180 - angleXY + 2 * beta;
        }

        alpha %= 360;
        if (alpha >= 180)
            alpha -= 180;
        if (alpha < 0)
            alpha += 180;
        return alpha;
    }

    //рассчет коэффициентов уравнения прямой в проекции XY (угол дан)
    private void CoeffXY()
    {
        //рассчет коэффициентов прямой для луча (проекция XY)
        kXy = (float)Mathf.Tan((float)angleXY * Mathf.PI / 180);
        bXy = capsule.transform.position.y - kXy * capsule.transform.position.x;
    }

    //рассчет коэффициентов уравнения прямой в проекции XZ (угол дан)
    private void CoeffXZ()
    {
        //рассчет коэффициентов прямых для каждого луча (проекция XZ)
        kXz = (float)Mathf.Tan((float)angleXZ * Mathf.PI / 180);
        bXz = capsule.transform.position.z - kXz * capsule.transform.position.x;
    }

    //поиск точки пересчения со сторонами
    private void Intersect()
    {
        float X = 0;
        float Y = 0;
        float Z = 0;

        float X_middle;
        float Y_middle;

        float x_middle;
        float y_middle;

        float x = 100;
        float y = 100;
        float z = 0;

        int INDEX = 0;
        int intersection_count = 0;
        int intersection_count2 = 0;

        recountXY = true;
        //direction = true;

        //поиск координат x и y
        //проходим по всем сторонам
        for (int i = 0; i < SensorInstallation.sides.Count; i++)
        {
            if (i == index)
            {
            }
            else
            {
                //если луч идет под 90 градусов
                if (angleXY == 90)
                {
                    //если сторона идет под 0 градусов
                    if (SensorInstallation.kSides[i] == 0)
                    {
                        x = capsule.transform.position.x;
                        y = SensorInstallation.sides[i][0].y;
                    }
                    //если сторона проходит под 90 градусов
                    else if (SensorInstallation.sides[i][0].x == SensorInstallation.sides[i][1].x)
                    {
                        x = SensorInstallation.sides[i][0].x;
                        y = capsule.transform.position.y;
                    }
                    else
                    {
                        x = capsule.transform.position.x;
                        y = SensorInstallation.kSides[i] * x + SensorInstallation.bSides[i];
                    }
                }


                //если луч идет под 0 градусов
                else if (kXy == 0)
                {
                    //если сторона идет под 90 градусов
                    if (SensorInstallation.sides[i][0].x == SensorInstallation.sides[i][1].x)
                    {
                        x = SensorInstallation.sides[i][0].x;
                        y = capsule.transform.position.y;
                    }
                    else
                    {
                        y = capsule.transform.position.y;
                        x = (float)(y - SensorInstallation.bSides[i]) / SensorInstallation.kSides[i];
                    }
                }
                //если луч не проходит под углом 90
                else
                {
                    //если сторона проходит под 90 градусов
                    if (SensorInstallation.sides[i][0].x == SensorInstallation.sides[i][1].x)
                    {
                        x = SensorInstallation.sides[i][0].x;
                        y = kXy * x + bXy;
                    }

                    //если сторона проходит под 0 градусов
                    else if (SensorInstallation.kSides[i] == 0)
                    {
                        y = SensorInstallation.sides[i][0].y;
                        x = (float)(y - bXy) / kXy;
                    }
                    else
                    {
                        //точка пересечения прямой, являющейся траекторией движения и i-ой стороны
                        x = (float)(SensorInstallation.bSides[i] - bXy) / (kXy - SensorInstallation.kSides[i]);
                        y = (float)(SensorInstallation.kSides[i] * (SensorInstallation.bSides[i] - bXy)) /
                            (kXy - SensorInstallation.kSides[i]) + SensorInstallation.bSides[i];
                    }
                }

                if (x > 0 && y > 0 && x < 200 && y < 200)
                {
                    //поиск середины отрезка на луче 
                    if (capsule.transform.position.x == x)
                    {
                        X_middle = x;
                        Y_middle = (float)Mathf.Abs(y - capsule.transform.position.y) / 2 + Mathf.Min(y, capsule.transform.position.y);
                    }

                    else if (capsule.transform.position.y == y)
                    {
                        Y_middle = y;
                        X_middle = (float)Mathf.Abs(x - capsule.transform.position.x) / 2 + Mathf.Min(x, capsule.transform.position.x);
                    }

                    else
                    {
                        X_middle = (float)Mathf.Abs(x - capsule.transform.position.x) / 2 + Mathf.Min(x, capsule.transform.position.x);
                        Y_middle = X_middle * kXy + bXy;
                    }


                    //поиск числа пересчений луча со сторонами 
                    for (int m = 0; m < SensorInstallation.sides.Count; m++)
                    {
                        //если сторона проходит под 90 градусов
                        if (SensorInstallation.sides[m][0].x == SensorInstallation.sides[m][1].x)
                        {
                            x_middle = SensorInstallation.sides[m][0].x;
                            y_middle = kXy * x_middle + bXy;
                        }

                        else if (SensorInstallation.sides[m][0].y == SensorInstallation.sides[m][1].y)
                        {
                            y_middle = SensorInstallation.sides[m][0].y;
                            x_middle = (float)(y_middle - bXy) / kXy;
                        }
                        else
                        {
                            x_middle = (float)(SensorInstallation.bSides[m] - bXy) /
                                (kXy - SensorInstallation.kSides[m]);
                            y_middle = (float)(SensorInstallation.kSides[m] * (SensorInstallation.bSides[m] - bXy)) /
                                (kXy - SensorInstallation.kSides[m]) + SensorInstallation.bSides[m];
                        }

                        //проверка первого условия (число пересчений со сторонами на отрезке от А до В)
                        if ((((x_middle < Mathf.Max(capsule.transform.position.x, x)) && (x_middle > Mathf.Min(capsule.transform.position.x, x)))
                            && ((y_middle < Mathf.Max(capsule.transform.position.y, y)) && (y_middle > Mathf.Min(capsule.transform.position.y, y))))
                            && (((x_middle <= Mathf.Max(SensorInstallation.sides[m][0].x, SensorInstallation.sides[m][1].x))
                            && (x_middle >= Mathf.Min(SensorInstallation.sides[m][0].x, SensorInstallation.sides[m][1].x)))
                            && ((y_middle <= Mathf.Max(SensorInstallation.sides[m][0].y, SensorInstallation.sides[m][1].y))
                            && (y_middle >= Mathf.Min(SensorInstallation.sides[m][0].y, SensorInstallation.sides[m][1].y)))))
                        {
                            intersection_count++;
                        }

                        //проверка второго условия (точка на середине отрезка приналежит многоугольнику)
                        if (
                            ((
                            (x_middle <= Mathf.Max(SensorInstallation.sides[m][0].x, SensorInstallation.sides[m][1].x))
                            && (x_middle >= Mathf.Min(SensorInstallation.sides[m][0].x, SensorInstallation.sides[m][1].x))
                            )
                            &&
                            (
                            (y_middle <= Mathf.Max(SensorInstallation.sides[m][0].y, SensorInstallation.sides[m][1].y))
                            && (y_middle >= Mathf.Min(SensorInstallation.sides[m][0].y, SensorInstallation.sides[m][1].y))
                            ))
                            &&
                            (
                            (x_middle < X_middle)
                            //x_middle < Mathf.Max(capsule.transform.position.x, x)
                            )
                          )
                        {
                            intersection_count2++;
                        }
                    }


                    if (intersection_count == 0 && intersection_count2 % 2 == 1)
                    {
                        if (((x <= Mathf.Max(SensorInstallation.sides[i][0].x, SensorInstallation.sides[i][1].x))
                                && (x >= Mathf.Min(SensorInstallation.sides[i][0].x, SensorInstallation.sides[i][1].x)))
                                && ((y <= Mathf.Max(SensorInstallation.sides[i][0].y, SensorInstallation.sides[i][1].y))
                                && (y >= Mathf.Min(SensorInstallation.sides[i][0].y, SensorInstallation.sides[i][1].y))))
                        {
                            X = x;
                            Y = y;
                            INDEX = i;
                        }
                    }
                    intersection_count = 0;
                    intersection_count2 = 0;
                }
            }
        }

        //поиск координаты z

        //задается направление движения по оси Х
        if (isStart)
        {
            isStart = false;
            if (X >= intersectXprev)
                direction = true;
            if (X <= intersectXprev)
                direction = false;
        }

        float newAngle;


        if (X >= intersectXprev)
        {
            if (direction == true)
            {
                z = kXz * X + bXz;

                if (z > 10 && z < 410)
                {
                    Z = z;
                }

                else 
                {
                    if (z <= 10)
                        Z = 10;
                    if (z >= 410)
                        Z = 410;

                    //меняется направление прямой при столкновении с границей
                    newAngle = -1 * angleXZ;
                    newAngle %= 360;
                    if (newAngle >= 180)
                        newAngle -= 180;
                    if (newAngle < 0)
                        newAngle += 180;
                    angleXZ = newAngle;

                    //пересчитывются оставшиеся координаты в зависимости от получившегося z
                    recountXY = false;
                    X = (Z - bXz) / kXz;
                    Y = kXy * X + bXy;
                }
            }

            else 
            {
                direction = true;

                newAngle = -1 * angleXZ;
                newAngle %= 360;
                if (newAngle >= 180)
                    newAngle -= 180;
                if (newAngle < 0)
                    newAngle += 180;
                angleXZ = newAngle;

                CoeffXZ();
                //пересчет координаты z
                z = kXz * X + bXz;

                if (z > 10 && z < 410)
                    Z = z;

                else 
                {
                    if (z <= 10)
                        Z = 10;
                    if (z >= 410)
                        Z = 410;

                    //меняется направление прямой при столкновении с границей
                    newAngle = -1 * angleXZ;
                    newAngle %= 360;
                    if (newAngle >= 180)
                        newAngle -= 180;
                    if (newAngle < 0)
                        newAngle += 180;
                    angleXZ = newAngle;

                    //пересчитывются оставшиеся координаты в зависимости от получившегося z
                    recountXY = false;
                    X = (Z - bXz) / kXz;
                    Y = kXy * X + bXy;
                }
            }
        }

        if (X <= intersectXprev)
        {
            if (direction == false)
            {
                z = kXz * X + bXz;

                if (z > 10 && z < 410)
                {
                    Z = z;
                }

                else
                {
                    if (z <= 10)
                        Z = 10;
                    if (z >= 410)
                        Z = 410;

                    //меняется направление прямой при столкновении с границей
                    newAngle = -1 * angleXZ;
                    newAngle %= 360;
                    if (newAngle >= 180)
                        newAngle -= 180;
                    if (newAngle < 0)
                        newAngle += 180;
                    angleXZ = newAngle;

                    //пересчитывются оставшиеся координаты в зависимости от получившегося z
                    recountXY = false;
                    X = (Z - bXz) / kXz;
                    Y = kXy * X + bXy;
                }
            }

            else 
            {
                direction = false;

                newAngle = -1 * angleXZ;
                newAngle %= 360;
                if (newAngle >= 180)
                    newAngle -= 180;
                if (newAngle < 0)
                    newAngle += 180;
                angleXZ = newAngle;

                CoeffXZ();

                //пересчет координаты z
                z = kXz * X + bXz;

                if (z > 10 && z < 410)
                    Z = z;

                else
                {
                    if (z <= 10)
                        Z = 10;
                    if (z >= 410)
                        Z = 410;

                    //меняется направление прямой при столкновении с границей
                    newAngle = -1 * angleXZ;
                    newAngle %= 360;
                    if (newAngle >= 180)
                        newAngle -= 180;
                    if (newAngle < 0)
                        newAngle += 180;
                    angleXZ = newAngle;

                    //пересчитывются оставшиеся координаты в зависимости от получившегося z
                    recountXY = false;
                    X = (Z - bXz) / kXz;
                    Y = kXy * X + bXy;
                }

            }

        }
    
        //в индекс записан номер стороны, с которой будет пересекаться луч 
        if (recountXY)
        {
            index = INDEX;
        }
        
        intersectX = X;
        intersectY = Y;
        intersectZ = Z;


        if (recountXY)
        {
            angleXY = AngleOfReflection(SensorInstallation.sides[INDEX][0], SensorInstallation.sides[INDEX][1]);
        }

        intersectXprev = X;
        intersectYprev = Y;
        intersectZprev = Z;

        followingPoint.x = intersectX;
        followingPoint.y = intersectY;
        followingPoint.z = intersectZ;
    }
}
