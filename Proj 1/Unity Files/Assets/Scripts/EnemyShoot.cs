using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public float ShotCooldown;
    public Timer shotCooldownTimer;
    
    public Enemy parent;
    public Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        shotCooldownTimer.endTime = ShotCooldown;
        shotCooldownTimer.currTime = ShotCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if(parent.targetInRange)
        {
            target = parent.target.gameObject.transform.position;
            //parent.targetPoint = target;
        }
        else
        {
            target = parent.targetPoint;
        }
        var normalDir = (target - transform.position).normalized;
        transform.up = new Vector3(normalDir.x, normalDir.y, 0f);
        // Flip sprites when appropriate
        if (normalDir.x < 0 && transform.localScale.x > 0)
        {
            transform.localScale =
                new Vector3(-transform.localScale.x
                           , transform.localScale.y);
        }
        if (normalDir.x > 0 && transform.localScale.x < 0)
        {
            transform.localScale =
                new Vector3(-transform.localScale.x
                    , transform.localScale.y);
        }

        EnemyAmmo.ShotPattern pattern = EnemyAmmo.ShotPattern.straight;

        switch(parent.type)
        {
            case (Enemy.eType.antEasy): pattern = EnemyAmmo.ShotPattern.straight; break;
            case (Enemy.eType.antMedium): pattern = EnemyAmmo.ShotPattern.straight; break;
            case (Enemy.eType.slime): pattern = EnemyAmmo.ShotPattern.circle; break;
            case (Enemy.eType.healer): pattern = EnemyAmmo.ShotPattern.straight; break;
            case (Enemy.eType.chicken): pattern = EnemyAmmo.ShotPattern.egg;  break;
            case (Enemy.eType.skeleton): pattern = EnemyAmmo.ShotPattern.skeleHead; break;
            case (Enemy.eType.boss): pattern = EnemyAmmo.ShotPattern.boss;  break;
        }
        var distanceToTarget = Vector2.Distance(parent.target.gameObject.transform.position, parent.gameObject.transform.position);
        if (shotCooldownTimer.atTime && parent.targetInRange)
        {
            if(parent.type != Enemy.eType.slime)
            {
                parent.ammo.Shoot(pattern);
                shotCooldownTimer.Reset();
                shotCooldownTimer.doUpdate = false;
            }
            else if(distanceToTarget <= 2.5f)
            {
                parent.ammo.Shoot(pattern);
                shotCooldownTimer.Reset();
                shotCooldownTimer.doUpdate = false;
            }
            
        }
        else
        {
            shotCooldownTimer.doUpdate = true;
        }
        
    }
}
