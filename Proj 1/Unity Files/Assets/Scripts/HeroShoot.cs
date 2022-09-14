/*******************************************************************************
File:      HeroShoot.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is added to the player and is responsible for the player's
    shoot ability and rotating the player to face the mouse position.

*******************************************************************************/
using UnityEngine;

//[RequireComponent(typeof(HeroStats))]
public class HeroShoot : MonoBehaviour
{
    public float ShotCooldown = 1.0f;
	[HideInInspector]
    public int BulletsPerShot = 1;

    public Timer shotCooldownTimer;
    public Hero parent;

    // Start is called before the first frame update
    void Start()
    {
	    shotCooldownTimer.endTime = ShotCooldown;
        shotCooldownTimer.currTime = ShotCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate player towards mouse position
        var worldMousePos = 
            Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, transform.position.z);
        var normalDir = (worldMousePos - transform.position).normalized;
        transform.up = new Vector3(normalDir.x, normalDir.y, 0f);
        // Flip sprites when appropriate
        if(normalDir.x < 0 && transform.localScale.x > 0)
        {
            transform.localScale = 
                new Vector3(-transform.localScale.x
                           , transform.localScale.y);
            parent.shotgun.transform.localScale = transform.localScale;
        }
        if (normalDir.x > 0 && transform.localScale.x < 0)
        {
            transform.localScale =
                new Vector3(-transform.localScale.x
                    , transform.localScale.y);
            parent.shotgun.transform.localScale = transform.localScale;
        }
        //else if(transform.localScale.x < 0)
        //{
        //    transform.localScale =
        //        new Vector3(-transform.localScale.x
        //            , transform.localScale.y);
        //}
        
        var vecToMouse = worldMousePos - transform.position;
        var angle = Mathf.Atan(transform.up.y / transform.up.x);
        var rotate = transform.rotation.eulerAngles.z;

        if (shotCooldownTimer.atTime && Input.GetMouseButton(0))
        {
			parent.ammo.Shoot(SceneDirector.Teams.Player, normalDir);
            shotCooldownTimer.currTime = 0.0f;
            shotCooldownTimer.doUpdate = true;
        }
    }

}


//int bulletsLeft = BulletsPerShot;
//float angleAdjust = 0.0f;
//Odd number of bullets means fire the first one straight ahead
//if (bulletsLeft%2 == 1)
//{
//    FireBullet(0.0f);
//	bulletsLeft--;
//}
//else //Even number of bullets means we need to adjust the angle slightly
//{
//	angleAdjust = 0.5f;
//}
////The rest of the bullets are spread out evenly
//while (bulletsLeft > 0)
//{
//    FireBullet(BulletSpreadAngle * (bulletsLeft/2) - (BulletSpreadAngle * angleAdjust));
//    FireBullet(-BulletSpreadAngle * (bulletsLeft/2) + (BulletSpreadAngle * angleAdjust));
//	bulletsLeft -= 2; //Must do this afterwards, otherwise the angle will be wrong
//}

//Reset shoot timer
//Timer = 0f;