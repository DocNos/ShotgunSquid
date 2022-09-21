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

        //var vecToMouse = worldMousePos - transform.position;
        //var angle = Mathf.Atan(transform.up.y / transform.up.x);
        //var rotate = transform.rotation.eulerAngles.z;

        
        if (shotCooldownTimer.atTime && parent.targetInRange)
        {
            parent.ammo.Shoot(EnemyAmmo.ShotPattern.straight);
            shotCooldownTimer.Reset();
            shotCooldownTimer.doUpdate = false;
        }
        else
        {
            shotCooldownTimer.doUpdate = true;
        }
    }
}
