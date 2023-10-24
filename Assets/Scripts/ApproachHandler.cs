using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ApproachHandler : MonoBehaviour
{

    [SerializeField] private Transform stoppingPoint;//This is the point the agent slows down towards to make intersection approach more realistic
    private Queue<CarAI> trafficQueue = new();
    public CarAI currentCar;
    public bool currentCarGoing = false;

    private IntersectionLogic intersectionLogic;
    public IntersectionLogic IntersectionLogic
    {
        get { return intersectionLogic; }
        set { intersectionLogic = value; }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            if (other.TryGetComponent<CarAI>(out CarAI car))
            {
                if (car != currentCar && !car.IsThisLastPathIndex())
                {
                    //provide car with intersection logic approach handler recieved from the intersection
                    car.IntersectionLogic = intersectionLogic;
                    trafficQueue.Enqueue(car);
                }
            }
        }
    }

    private void Update()
    {
        if (currentCar == null)
        {
            if (trafficQueue.Count > 0)
            {
                currentCar = trafficQueue.Dequeue();
                currentCar.UpdateVertex();
                currentCar.SetStoppingPoint(stoppingPoint.position);
                CheckCanGo();
            }
        }
        else
        {
            CheckCanGo();
        }
    }

    private void CheckCanGo()
    {
        if (!currentCar.IsTakingIntersection() && !currentCar.MakeIntersectionDecision())
        {
            currentCar.SetStoppingPoint(stoppingPoint.position);
        }
    }

    public void RemoveCar(CarAI car)
    {
        if (car == currentCar)
        {
            currentCar = null;
        }
    }

    internal CarAI GetCar()
    {
        return currentCar;
    }

    internal Turning GetCurrentCarTurn()
    {        
        if(currentCar != null)
        {
            return currentCar.GetNextTurn();
            
        } else
        {
            return Turning.NONE;
        }
    }

    internal bool GetCurrentCarGoing()
    {
        if(currentCar != null)
        {
            currentCarGoing = currentCar.IsTakingIntersection();
            return currentCarGoing;
        } else { return false; }

        
    }
}
