
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

    public bool IsAbleToGo(Turning turn)
    {
        Turning rightLaneTurn = rightLane.GetCurrentCarTurn();
        Turning oppositeLaneTurn = oppositeLane.GetCurrentCarTurn();
        Turning leftLaneTurn = leftLane.GetCurrentCarTurn();
        bool oppositeGoing = oppositeLane.GetCurrentCarGoing();
        bool leftGoing = leftLane.GetCurrentCarGoing();


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
