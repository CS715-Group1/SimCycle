
using System.Collections.Generic;

public interface IntersectionLogic
{
    bool IsAbleToGo(Turning turn, List<CarAI> carsSeen);
}
