using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartRoad : MonoBehaviour
{
    Queue<CarAI> trafficQueue = new();
    public CarAI currentCar;
    [SerializeField] private List<ApproachHandler> approachHandlers = new();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            if (other.TryGetComponent<CarAI>(out CarAI car)){
                if (car != currentCar && !car.IsThisLastPathIndex())
                {
                    trafficQueue.Enqueue(car);
                    car.Stop = true;
                }
            }
        }
    }

    private void Update()
    {
        if(currentCar == null)
        {
            if(trafficQueue.Count > 0)
            {
                currentCar = trafficQueue.Dequeue();
                currentCar.Stop = false;
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
        if(car == currentCar)
        {
            currentCar = null;
        }
    }
}
