using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ApproachHandler : MonoBehaviour
{
    private Queue<CarAI> trafficQueue = new();
    public CarAI currentCar;
    private List<CarAI> trafficList = new();
    private IntersectionLogic intersectionLogic;
    public IntersectionLogic IntersectionLogic { get; set; }


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
                currentCar.MakeIntersectionDecision();           
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            if (other.TryGetComponent<CarAI>(out CarAI car))
            {
                RemoveCar(car);
            }
        }
    }

    private void RemoveCar(CarAI car)
    {
        if (car == currentCar)
        {
            currentCar = null;
        }
    }

    internal Turning GetCurrentCarTurn()
    {
        return currentCar.Turning;
    }
}
