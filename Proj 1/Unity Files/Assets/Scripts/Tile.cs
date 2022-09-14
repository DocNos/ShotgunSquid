using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum Type 
    { floor, wall, special
            , portal, none, floorDirty
            , outerWall, exitPathN, exitPathE };
    // Dir checked -> From this tile, which are valid directions to take.
    public enum DirectionValid { north, east, south, west, none };
    // Index in the level tile array
    public int horzIndex;
    public int vertIndex;
    // Branch tile?
    public bool branch;
    public Dictionary<DirectionValid, bool> dirs;
    public Type type;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Check if player is within tile, spawn enemy on this tile, etc. 
    void Update()
    {
        
    }
}
