using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Itcycle : Ride
{
    public override void SetEffect(bool state)
    {
        base.SetEffect(state);
        if (state)
        {
            if (rider.HasHelmet)
            {
                rider.actualScoreSpeed = 2 * rider.ScoreSpeed;
            }
            else
            {
                rider.actualScoreSpeed = rider.ScoreSpeed;
            }

        }
        else
        {
            rider.actualScoreSpeed = rider.ScoreSpeed;
        }
    }
}
