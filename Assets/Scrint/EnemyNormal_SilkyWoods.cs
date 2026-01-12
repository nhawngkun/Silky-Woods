using UnityEngine;

public class EnemyNormal_SilkyWoods : EnemyBase_SilkyWoods
{
    protected override void AIBehavior()
    {
        MoveTowardsPlayer();
    }
}