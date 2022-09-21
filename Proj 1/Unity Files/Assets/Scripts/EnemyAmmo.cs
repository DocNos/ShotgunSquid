using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAmmo : MonoBehaviour
{
    public Enemy parent;
    public GameObject bulletPrefab;
    public int bulletsPerShot;
    public float bulletSpeed;
    public enum ShotPattern{ straight, circle, wave};

    void Start()
    {
        bulletPrefab = Resources.Load<GameObject>("Prefabs/BulletPrefab");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Shoot(ShotPattern pattern)
    {
        switch(pattern)
        {
            case (ShotPattern.straight): PatternLine(); break;
            case (ShotPattern.circle): PatternCircle(); break;
            case (ShotPattern.wave): PatternWave(); break;
        }
    }

    public void PatternLine()
    {
        var trans = parent.GetComponentInChildren<RectTransform>();
        Vector2 pos = gameObject.transform.position;
        Vector2 bulletSpawn = trans.position;
        Vector2 playerPos = parent.target.gameObject.transform.position;
        playerPos += (parent.target.rb.velocity.normalized * parent.target.stats.Speed);
        var direction =  (playerPos - pos).normalized;
        var bulletVelocity = direction * bulletSpeed;

        // Create origin bullet
        var centerBullet =
            Instantiate(bulletPrefab
                , new Vector3(bulletSpawn.x, bulletSpawn.y, -0.5f)
                , Quaternion.identity);
        var rotateVector = Vector3.Angle(bulletSpawn, playerPos);

        var rot = centerBullet.transform.rotation.eulerAngles;
       centerBullet.transform.rotation = 
            Quaternion.Euler(bulletVelocity.x, bulletVelocity.y, 0.0f);


        var bulletComp = centerBullet.GetComponent<Bullet>();
        bulletComp.GetComponent<Collider2D>().tag = "Enemy";
        bulletComp.RB = centerBullet.GetComponent<Rigidbody2D>();
        bulletComp.team = SceneDirector.Teams.Enemy;
        bulletComp.BulletRangeLeft = bulletComp.BulletRange;
        bulletComp.RB.velocity = (bulletVelocity + bulletComp.RB.velocity);
    }

    public void PatternCircle()
    {
        
    }

    public void PatternWave()
    {
        
    }

    public void ShootPattern(SceneDirector.Teams team, Vector3 shotAngle)
    {
        shotAngle.z = 0;
        var trans = parent.GetComponentInChildren<RectTransform>();
        Vector3 playerVel = parent.rb.velocity;
        var bulletSpawn = trans.position;
        bulletSpawn.z = 0;

        var bulletVelocity = shotAngle * bulletSpeed;
        bulletVelocity.z = 0;

        // Create origin bullet
        var centerBullet =
            Instantiate(bulletPrefab
            , new Vector3(bulletSpawn.x, bulletSpawn.y, bulletSpawn.z - 0.5f)
            , Quaternion.identity);
        centerBullet.transform.rotation = parent.transform.rotation;
        var bulletComp = centerBullet.GetComponent<Bullet>();
        bulletComp.RB = centerBullet.GetComponent<Rigidbody2D>();
        bulletComp.team = team;
        bulletComp.BulletRangeLeft = bulletComp.BulletRange;
        bulletComp.RB.velocity = (bulletVelocity);


        var numBullets = parent.bulletsPerShot;
        // Origin angle
        var startAngle = Mathf.Atan(shotAngle.y / shotAngle.x);
        if (shotAngle.x < 0 && shotAngle.y < 0)
        {
            startAngle += Mathf.PI;
            startAngle *= -1;
        }
        
        // Half circle, split among bullets and centered on 
        // initial shot angle. 
        var angleDivision = (((Mathf.PI / 2)) / numBullets);
        for (int i = 1; i <= numBullets / 2; ++i)
            {
                //startAngle = (startAngle < 0) ? (-startAngle) : (startAngle);
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
                bulletCompCW.RB.velocity = (velocityCW);


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
                bulletCompCCW.RB.velocity = (velocityCCW);

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
