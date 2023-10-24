
using System.Collections.Generic;
using UnityEngine;

public class FourWayLogic : IntersectionLogic
{
    private ApproachHandler rightLane;
    private ApproachHandler oppositeLane;
    private ApproachHandler leftLane;


    public FourWayLogic(ApproachHandler rightLane, ApproachHandler oppositeLane, ApproachHandler leftLane)
    {
        this.rightLane = rightLane;
        this.oppositeLane = oppositeLane; 
        this.leftLane = leftLane;
    }

    public bool IsAbleToGo(Turning turn, List<CarAI> carsSeen, bool useVision)
    {
        //Check that agent can see car? then that cars turn is properly retrieved : That cars turn is set to NONE and going is NONE
        //Get current carAI from Approaching regions 
        //Compare against carsSeen
        //If can't see car turn = TURNING.NONE and going = false 

        CarAI rightCar = rightLane.GetCar();
        CarAI leftCar = leftLane.GetCar();
        CarAI oppositeCar = oppositeLane.GetCar();

        Turning rightLaneTurn = Turning.NONE;
        Turning leftLaneTurn = Turning.NONE;
        Turning oppositeLaneTurn = Turning.NONE;
        bool leftGoing = false;
        bool oppositeGoing = false;


        //Only get the states of cars that the car can see if using vision
        if (!useVision || carsSeen.Contains(rightCar)){
            rightLaneTurn = rightLane.GetCurrentCarTurn();
        } 

        if (!useVision || carsSeen.Contains(leftCar))
        {
            leftLaneTurn = leftLane.GetCurrentCarTurn();
            leftGoing = leftLane.GetCurrentCarGoing();
        }

        if (!useVision || carsSeen.Contains(oppositeCar)){
            oppositeLaneTurn = oppositeLane.GetCurrentCarTurn();
            oppositeGoing = oppositeLane.GetCurrentCarGoing();
        }

        //Check for right of way, returning false if car does not have right of way
        switch (turn)
        {
            case Turning.LEFT:
                if (rightLaneTurn == Turning.STRAIGHT || (oppositeLaneTurn == Turning.RIGHT && oppositeGoing))
                {
                    return false;
                }
                break;

            case Turning.STRAIGHT:
                if (rightLaneTurn == Turning.STRAIGHT || rightLaneTurn == Turning.RIGHT || (oppositeLaneTurn == Turning.RIGHT && oppositeGoing) || ((leftLaneTurn == Turning.LEFT || leftLaneTurn == Turning.RIGHT ) && leftGoing))
                {
                    return false;
                }
                break;

            case Turning.RIGHT:
                if (rightLaneTurn == Turning.STRAIGHT || rightLaneTurn == Turning.RIGHT || oppositeLaneTurn == Turning.STRAIGHT || oppositeLaneTurn == Turning.LEFT || (oppositeLaneTurn == Turning.RIGHT && oppositeGoing) || ((leftLaneTurn == Turning.STRAIGHT || leftLaneTurn == Turning.RIGHT)))
                {
                    return false;
                }
                break;
        }

        return true;
    }
}
