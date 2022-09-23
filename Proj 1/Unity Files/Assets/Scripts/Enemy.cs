using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public enum eType { antEasy, antMedium, healer, bomb, boss, slime, chicken, skeleton};
    public eType type;
    public Rigidbody2D rb;

    public GameObject EnemyHealthBarPrefab;
    private GameObject HealthBar;
    private HealthBar HealthBarComp;
    public EnemyAmmo ammo;
    public int bulletsPerShot;
    public Hero target;
    public bool targetInRange;
    public SceneDirector director;
    public Timer idleTimer;
    public float idleTime;

    public float moveSpeed;
    public Vector2 targetPoint;
    public Vector2 idleCenter;

    public float aggroRange;

    public int StartingHealth = 3;
    public int Health
    {
        get { return _Health; }

        set
        {
            HealthBarComp.Health = value;
            _Health = value;
        }

    }
    private int _Health;

    // Start is called before the first frame update
    void Start()
    {
        //HealthBar = Instantiate(EnemyHealthBarPrefab);
        //HealthBar.GetComponent<ObjectFollow>().ObjectToFollow = transform;
        //HealthBarComp = HealthBar.GetComponent<HealthBar>();
        //HealthBarComp.MaxHealth = StartingHealth;
        //HealthBarComp.Health = StartingHealth;
        //Health = StartingHealth;
        rb = GetComponentInChildren<Rigidbody2D>();
        targetInRange = false;
        idleTimer.endTime = idleTime;
        idleTimer.currTime = idleTime;
        idleTimer.doUpdate = false;
        target = director.hero;
        idleCenter = gameObject.transform.position;
        ammo = GetComponentInChildren<EnemyAmmo>();
        
    }

    // Update is called once per frame
    void Update()
    {
        TargetInRange();
        if(targetInRange)
        {
            ChasePlayer();
        }
        else
        {
            Idle(2.0f);
        }
        
    }

    public void TargetInRange()
    {
        var targetPos = 
            new Vector2(target.gameObject.transform.position.x
                      , target.gameObject.transform.position.y);

        var pos =
            new Vector2(gameObject.transform.position.x
                , gameObject.transform.position.y);

        
        targetInRange = (Vector2.Distance(pos, targetPos) <= aggroRange);

        if(targetInRange)
        {
            idleCenter = gameObject.transform.position;
        }
        
    }

    public void Idle(float idleRadius)
    {
        
        if(idleTimer.atTime)
        {
            var randCircle = Random.Range(0.0f, Mathf.PI * 2.0f);

            // y = r * sin(theta)
            // x = r * cos(theta)
            // Decompose shot angle vector
            var x0 = idleRadius * Mathf.Cos(randCircle);
            var y0 = idleRadius * Mathf.Sin(randCircle);

            var target = idleCenter;
            target.x += x0;
            target.y += y0;
            targetPoint = target;
            
            idleTimer.currTime = 0.0f;
            idleTimer.doUpdate = false;
        }
        else
        {
            if(MoveToPoint(targetPoint))
            {
                idleTimer.doUpdate = true;
            }
            
        }
        
    }


    public void ChasePlayer()
    {
        
    }
    // True if at destination
    public bool MoveToPoint(Vector2 point)
    {
        var pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        var direction = (point - pos).normalized;
        var velocity  = direction * moveSpeed;
        var distance = Vector2.Distance(pos, point);
        bool doMove = (distance >= 0.5f);

        if (doMove)
        {
            var targetPos = rb.position + direction;
            var lerpVelocity = Vector2.Lerp(rb.velocity, rb.velocity + velocity, .2f);
            rb.velocity = lerpVelocity;
            return false;
        }
        else
        {
            var lerpVelocity = Vector2.Lerp(rb.velocity, Vector2.zero, .6f);
            if (Mathf.Abs(lerpVelocity.x) < 0.5 && Mathf.Abs(lerpVelocity.y) < 0.5)
                lerpVelocity = Vector2.zero;
            rb.velocity = lerpVelocity;
            return true;
        }
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        var collidedTile = col.gameObject.GetComponent<Tile>();
        if (collidedTile)
        {
            if (collidedTile.type == Tile.Type.wall 
                || collidedTile.type == Tile.Type.outerWall)
            {
                var tileCenter = 
                    new Vector2(col.gameObject.transform.position.x, col.gameObject.transform.position.y);
                var pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
                var maxBounds = new Vector2(col.bounds.max.x, col.bounds.max.y);
                var minBounds = new Vector2(col.bounds.min.x, col.bounds.min.y);

                var maxVec = pos - maxBounds;
                var minVec = pos - minBounds;

                var reflect = tileCenter + (-rb.velocity);
                targetPoint = reflect;
            }
        }
    }

    private void OnDestroy()
    {
        Destroy(HealthBar);
    }
}
