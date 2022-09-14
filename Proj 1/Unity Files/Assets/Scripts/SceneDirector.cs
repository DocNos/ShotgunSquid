using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDirector : MonoBehaviour
{
    public enum Teams { Player, Enemy }
    public PCG procedural;
    public GameObject heroPrefab;
    public Canvas canvas;
    public GameObject pStart;
    public Camera cam;
    public Tile cheatToEnd;
    public Hero hero;

    // Start is called before the first frame update
    void Start()
    {
        procedural.cam = GetComponentInChildren<Camera>();
        // Give corners to PCG
        Transform trans = gameObject.transform;
        Vector3[] corners0 = new Vector3[4];
        canvas.GetComponent<RectTransform>().GetWorldCorners(procedural.corners);
        
        SpawnNewLevel();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if(Input.GetKeyDown(KeyCode.K))
        {
            hero.EndCheat(cheatToEnd);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
        
    }

    public void SpawnNewLevel()
    {
        procedural.SpawnNoneTiles();
        
        // Place player & starting room - should happen before tiles.
        var quarterSize = procedural.MaxMapSize / 4;
        var eigthSize = quarterSize / 2;
        var randY = procedural.RNG.Next(eigthSize, quarterSize);
        var randX = procedural.RNG.Next(eigthSize, procedural.MaxMapSize);
        var startTile = procedural.allTiles[randX, randY].GetComponent<Tile>();
        procedural.SpawnRoom(startTile, 7, 5);
        var heroSpawn = startTile.gameObject.transform.position;
        heroSpawn.z = -1;
        
        procedural.SpawnPCGFloors();


        // Create hero
        heroPrefab = Resources.Load<GameObject>("Prefabs/HeroPrefab");
        heroPrefab.transform.position =
            new Vector3(heroSpawn.x, heroSpawn.y, 0f);
        var heroObj = Instantiate(heroPrefab);
        hero = heroObj.GetComponent<Hero>();
        hero.director = this;

        // Set cam to follow hero
        cam.GetComponent<CameraFollow>().ObjectToFollow = heroObj.transform;
        cam.gameObject.transform.position = 
            new Vector3(heroObj.transform.position.x
                        , heroObj.transform.position.y
                        , heroObj.transform.position.z - 2);
        // Create exit indicator
        hero.exitIndicator =
            Instantiate(procedural.Prefabs["ExitIndicator"]);

        // Create portal & exit paths
        var pPos = GetPortalPos(heroSpawn);
        var portalObj = procedural.CreateTileIndex(Tile.Type.portal
            , (int)pPos.x
            , (int)pPos.y);
        // Give collider trigger to hero
        hero.portalCol = portalObj.GetComponent<Collider2D>();
        Tile portalTile = portalObj.GetComponent<Tile>();
        procedural.SpawnExitPaths(portalTile);
        cheatToEnd = procedural.allTiles[portalTile.horzIndex, portalTile.vertIndex - 2]
                        .GetComponent<Tile>();
        hero.portalPos = portalObj.transform.position;
    }
    

    public Vector2 GetPortalPos(Vector2 hSpawn)
    {
        var portalX = 0;
        var portalY = 0;
        var halfMap = procedural.MaxMapSize / 2;
        // To guarantee portal is not too close to edges
        var offsetY = 10; 
        var offsetX = 10;
        var xInc = 1;
        var yInc = 1;
        // Spawn portal in opposite quadrant of hero
        portalY = (hSpawn.y > halfMap) ? (offsetY)
                                       : (procedural.MaxMapSize - offsetY);
        portalX = (hSpawn.x > halfMap) ? (offsetX)
                                       : (procedural.MaxMapSize - offsetX);
        yInc = (hSpawn.y > halfMap) ? (yInc) : (-yInc);
        xInc = (hSpawn.x > halfMap) ? (xInc) : (-xInc);

        while (portalX != halfMap)
        {
            var currTile = procedural.allTiles[portalX, portalY]
                .GetComponent<Tile>();
            if (currTile.type == Tile.Type.floor
                || currTile.type == Tile.Type.floorDirty)
            {
                break;
            }
            portalX += xInc;
            portalY += yInc;
        }
        
        

        return new Vector2(portalX, portalY);
    }

    public Vector3 ConvertToWorld(Vector3 local, Transform localTrans)
    {
        Matrix4x4 localToWorld =
            localTrans.localToWorldMatrix;

        return localToWorld * local;
    }

    public Vector3 ConvertToLocal(Vector3 world, Transform worldTrans)
    {
        Matrix4x4 localToWorld = worldTrans.worldToLocalMatrix;

        return localToWorld * world;
    }

}


//procedural.corners[0] = procedural.cam.ViewportToWorldPoint
//    (new Vector3(1, 1, procedural.cam.nearClipPlane));