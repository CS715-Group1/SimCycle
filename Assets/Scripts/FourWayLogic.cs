
public class FourWayLogic : IntersectionLogic
{
    private ApproachHandler rightLane;
    private ApproachHandler oppositeLane;
  

    public FourWayLogic(ApproachHandler rightLane, ApproachHandler oppositeLane )
    {
        this.rightLane = rightLane;
        this.oppositeLane = oppositeLane; 
    }

    public bool IsAbleToGo(Turning turn)
    {
        Turning rightLaneTurn = rightLane.GetCurrentCarTurn();
        Turning oppositeLaneTurn = oppositeLane.GetCurrentCarTurn();


        switch (turn)
        {
            case Turning.LEFT:
                if (rightLaneTurn == Turning.STRAIGHT) return false;
                break;

            case Turning.STRAIGHT:
                if (rightLaneTurn == Turning.STRAIGHT || rightLaneTurn == Turning.RIGHT) return false;
                break;

            case Turning.RIGHT:
                if (rightLaneTurn == Turning.STRAIGHT || rightLaneTurn == Turning.RIGHT || oppositeLaneTurn == Turning.STRAIGHT || oppositeLaneTurn == Turning.RIGHT) return false;
                break;

        }

        return true;
    }
}
