/*******************************************************************************
File:      BulletLogic.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is added to the bullet and controls all of its behavior,
    including how to handle when different objects are hit.

*******************************************************************************/
using UnityEngine;
using UnityEngine.Animations;

//public enum Teams { Player, Enemy }

public class BulletLogic : MonoBehaviour
{
    //public Teams Team = Teams.Player;
	[HideInInspector]
    public GameObject BulletPrefab;
    public Hero parent;
    
    public float BulletRangeLeft;
    public float BulletSpeed = 5.0f;
    public float BulletRange = 20.0f;
    public float BulletSpreadAngle = 0.1f;
    public int maxShots;
    public int currShots;

    private Rigidbody2D RB;

    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        currShots = maxShots;
    }
	
    void Update()
    {
		//Destroy the bullet after it has travelled far enough
		BulletRangeLeft -= (Time.deltaTime * RB.velocity.magnitude);
		if (BulletRangeLeft < 0)
			Destroy(gameObject);
	}

    private void OnTriggerEnter2D(Collider2D col)
    {
		////No friendly fire
        //if (col.isTrigger || col.tag == Team.ToString())
        //    return;
        //Destroy(gameObject);
    }

    public void Shoot()
    {
        if(currShots > 0)
        {
            //FireBullet(0.0f);
            --currShots;
        }
        else
        {
            //reload
        }
    }

    
}
