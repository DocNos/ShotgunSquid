using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    public Hero parent;
    public GameObject shellSprite;
    public GameObject[] ammoSprites;
    public GameObject bulletPrefab;
    public List<GameObject> bullets;
    public int currBullets;
    public int maxShots;
    public Timer reloadTimer;
    public float reloadTime;
	public TextMesh debug;

    void Start()
    {
        reloadTimer.endTime = reloadTime;
        reloadTimer.currTime = reloadTimer.endTime;
        reloadTimer.doUpdate = false;
        currBullets = maxShots;

        ammoSprites = new GameObject[maxShots];
        shellSprite = Resources.Load<GameObject>("Prefabs/AmmoSprite");
        float offset = 0f;
        // Ammo Display
        for(int i = 0; i < currBullets; ++i)
        {
            ammoSprites[i] = Instantiate(shellSprite);
            ammoSprites[i].transform.position 
                = new Vector3(parent.transform.position.x + offset
                , parent.transform.position.y + 1f
                , parent.transform.position.z);
            //ammoSprites[i].transform.rotation 
            //    = parent.transform.rotation;
            offset += 0.15f;
        }
        bulletPrefab = Resources.Load<GameObject>("Prefabs/BulletPrefab");
    }

    // Update is called once per frame
    void Update()
    {
        var offset = Vector3.zero;
        offset.y = 0.75f; offset.x = -1.75f;
        foreach (var ammo in ammoSprites)
        {
            ammo.transform.position = parent.transform.position + offset;
            offset.x += 0.5f;            
        }
        for(int i = 0; i < currBullets; ++i)
        {
            ammoSprites[currBullets - 1].GetComponent<SpriteRenderer>().enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }

    }

    public void Shoot(SceneDirector.Teams team, Vector3 shotAngle)
    {
		var canvas = parent.debugCanvas.gameObject;
		if(!debug)
		{
			var textMesh = canvas.AddComponent<TextMesh>();
			debug = textMesh;
		}
		

        if(currBullets > 0)
        {
            var trans = parent.shotgun.GetComponentInChildren<RectTransform>();
            Vector3 playerVel = parent.rb.velocity;
            var  bulletSpawn = trans.position;
            bulletSpawn.z = 0;

            var bulletVelocity = shotAngle * parent.bulletSpeed;
            
            // Create origin bullet
            var centerBullet =
                Instantiate(bulletPrefab
                , new Vector3(bulletSpawn.x, bulletSpawn.y, bulletSpawn.z - 0.5f)
                , Quaternion.identity);
            centerBullet.transform.rotation = parent.transform.rotation;
            var bulletComp = centerBullet.GetComponent<Bullet>();
            bulletComp.RB =  centerBullet.GetComponent<Rigidbody2D>();
            bulletComp.team = team;
            bulletComp.BulletRangeLeft = bulletComp.BulletRange;
            bulletComp.RB.velocity = (playerVel + bulletVelocity);


            var numBullets = parent.bulletsPerShot;
            // Origin angle
            var startAngle = Mathf.Atan(shotAngle.y / shotAngle.x);
			debug.text = startAngle.ToString();
			//if(startAngle < 0)
            //{
            //    startAngle = Mathf.Abs(startAngle);
            //    if(shotAngle.x < 0 && shotAngle.y > 0)
            //    {
            //        startAngle += (Mathf.PI / 2);
            //    }
            //    if(shotAngle.x < 0 && shotAngle.y < 0)
            //    {
            //        startAngle += (Mathf.PI);
            //    }
            //    if (shotAngle.y < 0 && shotAngle.x > 0)
            //    {
            //        startAngle += (3 * Mathf.PI / 2);
            //    }
            //}
            // Half circle, split among bullets and centered on 
            // initial shot angle. 
            var angleDivision = (((Mathf.PI/2)) / numBullets) ;
            for (int i = 1; i <= numBullets / 2; ++i)
            {
                startAngle = (startAngle < 0) ? (-startAngle) : (startAngle);
                // Increment by this angle on the half-circle
                var angleCW = (angleDivision * i) + startAngle;
                // Decompose shot angle vector
                var x0 = Mathf.Cos(angleCW + shotAngle.x) + bulletSpawn.x; // Needs radius
                var y0 = Mathf.Sin(angleCW + shotAngle.y) + bulletSpawn.y;
                
                // Create bullet wrapping clockwise from origin \\
                var bulletCW =
                Instantiate(bulletPrefab
                , new Vector3(x0, y0, bulletSpawn.z - 0.5f)
                , Quaternion.identity);
                bulletCW.transform.rotation = parent.transform.rotation;
                var bulletCompCW = bulletCW.GetComponent<Bullet>();
                bulletCompCW.RB = bulletCW.GetComponent<Rigidbody2D>();
                // Set team
                bulletCompCW.team = team;
                // Unused range
                bulletCompCW.BulletRangeLeft = bulletCompCW.BulletRange;
                // Rotate velocity
                var velocityCW =
                    new Vector3(bulletVelocity.x * Mathf.Cos(angleCW)
                                , bulletVelocity.y * Mathf.Sin(angleCW));
                // Apply velocity and offset from player
                bulletCompCW.RB.velocity = (playerVel + velocityCW);


                // Create bullet wrapping counter clockwise from origin \\
                var angleCCW = (-(angleDivision * i)) + startAngle;
                var x1 = Mathf.Cos(angleCCW + shotAngle.x) + bulletSpawn.x;
                var y1 = Mathf.Sin(angleCCW + shotAngle.y) + bulletSpawn.y;
                var bulletCCW = 
                Instantiate(bulletPrefab
                , new Vector3(x1, y1, bulletSpawn.z - 0.5f)
                , Quaternion.identity);
                bulletCCW.transform.rotation = parent.transform.rotation;
                var bulletCompCCW = bulletCCW.GetComponent<Bullet>();
                bulletCompCCW.RB = bulletCCW.GetComponent<Rigidbody2D>();
                bulletCompCCW.team = team;
                bulletCompCCW.BulletRangeLeft = bulletCompCCW.BulletRange;

                // Rotate velocity
                var velocityCCW =
                    new Vector3(bulletVelocity.x * Mathf.Cos(angleCCW)
                        , bulletVelocity.y * Mathf.Sin(angleCCW));
                bulletCompCCW.RB.velocity = (playerVel + velocityCCW);

            }

            

            var pScale = parent.transform.localScale.x;
            
            


            //Rotate bullet to match player direction
            //var fwd = RotateVector(bulletSpawn + playerVel, rotate);
            //var fwdNrml = fwd.normalized;
            ////bullet.transform.up = (fwd);
            ////bullet.transform.position = new Vector3(fwd.x, fwd.y, parent.transform.position.z + 0.5f);
            ////Add bullet velocity
            //bullet.GetComponent<Rigidbody2D>().velocity 
            //    = (fwdNrml * (BulletSpeed + (parent.GetComponent<HeroStats>().Speed 
            //            - parent.GetComponent<HeroStats>().StartingSpeed) * 2));

            ammoSprites[currBullets - 1].GetComponent<SpriteRenderer>().enabled = false;
            --currBullets;
            reloadTimer.doUpdate = true;
        }
        else
        {
            Reload();
        }
    }

    public void Reload()
    {
        if(reloadTimer.atTime && currBullets != maxShots)
        {
            ++currBullets;
            reloadTimer.currTime = 0f;
            reloadTimer.doUpdate = true;
        }
    }

    public Vector2 RotateVector(Vector2 vec, float angle)
    {
        //x2 = cos(A) * x1 - sin(A) * y1
        var newX = Mathf.Cos(angle) * vec.x - Mathf.Sin(angle) * vec.y;

        //y2 = sin(A) * x1 + cos(B) * y1;
        var newY = Mathf.Sin(angle) * vec.x + Mathf.Cos(angle) * vec.y;

        return new Vector2(newX, newY);
    }
}
