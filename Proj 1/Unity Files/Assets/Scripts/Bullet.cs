using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public SceneDirector.Teams team;
    [HideInInspector]
    public GameObject BulletPrefab;
    public Hero parent;

    public float BulletRangeLeft;
    public float BulletSpeed = 5.0f;
    public float BulletRange = 20.0f;
    public float BulletSpreadAngle = 0.1f;
    public Timer liveTimer;
    public float endTime;
    public Rigidbody2D RB;

    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        parent = GetComponentInParent<Hero>();
        liveTimer.endTime = endTime;
        liveTimer.doUpdate = true;
    }

    void Update()
    {
        //Destroy the bullet after it has travelled far enough
        BulletRangeLeft -= (Time.deltaTime * RB.velocity.magnitude);
        if (BulletRangeLeft < 0 || liveTimer.atTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        ////No friendly fire
        if (col.isTrigger || col.tag == team.ToString()
            || col.GetComponent<Bullet>())
        {
            return;
        }
            
        Destroy(gameObject);
    }

    public Vector2 RotateVector(Vector2 vec, float Angle)
    {
        //x2 = cos(A) * x1 - sin(A) * y1
        var newX = Mathf.Cos(Angle) * vec.x - Mathf.Sin(Angle) * vec.y;

        //y2 = sin(A) * x1 + cos(B) * y1;
        var newY = Mathf.Sin(Angle) * vec.x + Mathf.Cos(Angle) * vec.y;

        return new Vector2(newX, newY);
    }
}

//void FireBullet(float rotate)
//{
//    //Spawn Bullet
//    var bullet = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
//    //Rotate bullet to match player direction
//    var fwd = RotateVector(transform.up, rotate);
//    bullet.transform.up = fwd.normalized;
//    //Add bullet velocity
//    bullet.GetComponent<Rigidbody2D>().velocity = fwd * (BulletSpeed + (GetComponent<HeroStats>().Speed - GetComponent<HeroStats>().StartingSpeed) * 2);
//    //Set bullet's range
//    bullet.GetComponent<BulletLogic>().BulletRangeLeft = BulletRange;
//
//
//}