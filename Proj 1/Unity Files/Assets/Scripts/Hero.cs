using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Hero : MonoBehaviour
{
    
    public HeroStats stats;
    public Rigidbody2D rb;
    public SceneDirector.Teams team = SceneDirector.Teams.Player;
    public Vector3 startPos;
    public GameObject exitIndicator;
    public Vector2 portalPos;
    public Collider2D portalCol;
    public SceneDirector director;
    public Timer restartTimer;

    public HeroShoot shoot;
    public Ammo ammo;
    public GameObject shotgun;
    public int bulletsPerShot;
    public float bulletSpeed;

	public Canvas debugCanvas;

    // Start is called before the first frame update
    void Start()
    {
        shotgun = Instantiate
            (Resources.Load<GameObject>("Prefabs/Shotgun")
            , transform);
        var parentPos = 
            new Vector3(transform.position.x 
            , transform.position.y + 0.5f
            , transform.position.z - 0.5f);
        shotgun.transform.position = parentPos;
        shotgun.transform.localPosition =
            new Vector3(shotgun.transform.localPosition.x
                        , shotgun.transform.localPosition.y
                        , shotgun.transform.localPosition.z + 5);
        var shotSprite = shotgun.GetComponent<SpriteRenderer>();
        shotSprite.enabled = false;

        ammo.parent = this;

        rb = GetComponent<Rigidbody2D>();

        restartTimer = gameObject.AddComponent<Timer>();
        restartTimer.doUpdate = false;
        restartTimer.endTime = 0.25f;
        restartTimer.currTime = 0f;
        stats = GetComponent<HeroStats>();
        stats.parent = this;

    }

    // Update is called once per frame
    void Update()
    {
        
        MoveHero();

        var vecToExit = portalPos - new Vector2(transform.position.x, transform.position.y);
        vecToExit.Normalize();
        var angle = MathF.Atan(vecToExit.y / vecToExit.x);
        exitIndicator.transform.position = 
            new Vector3(transform.position.x, transform.position.y + 1.5f, 0);
        exitIndicator.transform.up = vecToExit;
    }
    // Handle collisions
    //  - Only with portal atm
    public void OnTriggerEnter2D(Collider2D col)
    {
        var collidedTile = col.gameObject.GetComponent<Tile>();
        if (collidedTile)
        {
            if(collidedTile.type == Tile.Type.portal)
            {
                ExitAndRestart();
                rb.velocity = new Vector2(0, 0);
            }
        }
    }

    public void ExitAndRestart()
    {
        restartTimer.doUpdate = true;
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);

    }

    public void EndCheat(Tile cheatTile)
    {
        var cheatPos = cheatTile.gameObject.transform.position;
        gameObject.transform.position =
            new Vector3(cheatPos.x, cheatPos.y, -2f);
    }

    void MoveHero()
    {
        var totalVel = rb.velocity;
        bool doMove = false;
        if(Input.GetKey(KeyCode.W))
        {
            totalVel += (Vector2.up * stats.Speed);
            doMove = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            totalVel += (Vector2.down * stats.Speed);
            doMove = true;
        }

        if (Input.GetKey(KeyCode.A))
        {
            totalVel += (Vector2.left * stats.Speed);
            doMove = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            totalVel += (Vector2.right * stats.Speed);
            doMove = true;
        }

        if(doMove)
        {
            var normalVel = (totalVel);
            var targetPos = rb.position + normalVel;
            var lerpVelocity = Vector2.Lerp(normalVel, totalVel.normalized, .5f);
            rb.velocity = lerpVelocity;
        }
        else
        {
            var lerpVelocity = Vector2.Lerp(totalVel, Vector2.zero, .6f);
            rb.velocity = lerpVelocity;
        }
        
            
    }
}
