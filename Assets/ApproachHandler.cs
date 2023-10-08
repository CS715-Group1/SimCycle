using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ApproachHandler : MonoBehaviour
{

    [SerializeField] private Transform stoppingPoint;
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
                currentCar.Approaching();
                currentCar.SetStoppingPoint(stoppingPoint.position);
                checkCanGo();
            }
        }
        else
        {
            checkCanGo();
        }
    }

    private void checkCanGo()
    {
        if (!currentCar.IsTakingIntersection() && !currentCar.MakeIntersectionDecision())
        {
            currentCar.SetStoppingPoint(stoppingPoint.position);
        }
    }

    //Potentially might need to add this back in case another car grazes the side of the collider and enters the queue without leaving the expected way

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Car"))
    //    {
    //        if (other.TryGetComponent<CarAI>(out CarAI car))
    //        {
    //            RemoveCar(car);
    //        }
    //    }
    //}

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
