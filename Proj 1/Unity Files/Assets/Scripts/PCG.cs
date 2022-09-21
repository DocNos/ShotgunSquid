﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
using Random = System.Random;
using static UnityEngine.Random;

public class PCG : MonoBehaviour
{
	public float GridSize = 2.0f; //Size of floor and wall tiles in units
	public int MaxMapSize; //Maximum height and width of tile map


	public Dictionary<string, GameObject> Prefabs; //Dictionary of all PCG prefabs
	private Dictionary<Tile.Type, GameObject> tilePrefabs;
	private Dictionary<Enemy.eType, GameObject> enemyPrefabs;
	private GameObject[] TileMap; //Tilemap array to make sure we don't put walls over floors
	private int TileMapMidPoint; //The 0,0 point of the tile map array
	public System.Random RNG;
	public Camera cam;
	public Vector3[] corners;
	public SceneDirector parent;
	public GameObject[] floorTiles;

	public GameObject[,] allTiles;
	public List<Tile> branchTiles;
	public int branchChance; 
	private int maxBranches; // Currently tied to numRooms * 2
	private int numRooms;

	public Tile playerSpawnTile;
	// Start is called before the first frame update
	void Start()
	{
		numRooms = MaxMapSize / 2;
		maxBranches = numRooms * 2;
		
		corners = new Vector3[4];
		floorTiles = new GameObject[MaxMapSize * MaxMapSize];
		allTiles = new GameObject[MaxMapSize, MaxMapSize];
		tilePrefabs = new Dictionary<Tile.Type, GameObject>();
		enemyPrefabs = new Dictionary<Enemy.eType, GameObject>();
		branchTiles = new List<Tile>();

		tilePrefabs.Add(Tile.Type.none, Resources.Load<GameObject>("Prefabs/NoTile"));
		tilePrefabs[Tile.Type.none].transform.localScale = new Vector3(GridSize, GridSize, 1.0f);
		
		tilePrefabs.Add(Tile.Type.floor, Resources.Load<GameObject>("Prefabs/DungeonFloor"));
		tilePrefabs[Tile.Type.floor].transform.localScale = new Vector3(GridSize, GridSize, 1.0f); //Scale the floor properly
		
		tilePrefabs.Add(Tile.Type.floorDirty, Resources.Load<GameObject>("Prefabs/DungeonFloorDirty"));
		tilePrefabs[Tile.Type.floorDirty].transform.localScale = new Vector3(GridSize, GridSize, 1.0f);
		
		tilePrefabs.Add(Tile.Type.special, Resources.Load<GameObject>("Prefabs/FloorSpecial"));
		tilePrefabs[Tile.Type.special].transform.localScale = new Vector3(GridSize, GridSize, 1.0f); //Scale the floor properly
		
		tilePrefabs.Add(Tile.Type.wall, Resources.Load<GameObject>("Prefabs/DungeonWall"));
		tilePrefabs[Tile.Type.wall].transform.localScale = new Vector3(GridSize, GridSize, 1.0f); //Scale the wall properly
		
		tilePrefabs.Add(Tile.Type.outerWall, Resources.Load<GameObject>("Prefabs/DungeonWall"));
		tilePrefabs[Tile.Type.outerWall].transform.localScale = new Vector3(GridSize, GridSize, 1.0f);
		
		tilePrefabs.Add(Tile.Type.portal, Resources.Load<GameObject>("Prefabs/DungeonPortal"));
		tilePrefabs[Tile.Type.portal].transform.localScale = new Vector3(GridSize, GridSize, 1.0f);

		tilePrefabs.Add(Tile.Type.exitPathN, Resources.Load<GameObject>("Prefabs/DungeonExitPathNorth"));
		tilePrefabs[Tile.Type.exitPathN].transform.localScale = new Vector3(GridSize, GridSize, 1.0f);

		tilePrefabs.Add(Tile.Type.exitPathE, Resources.Load<GameObject>("Prefabs/DungeonExitPathEast"));
		tilePrefabs[Tile.Type.exitPathE].transform.localScale = new Vector3(GridSize, GridSize, 1.0f);

		enemyPrefabs.Add(Enemy.eType.ant, Resources.Load<GameObject>("Prefabs/antEnemy"));
		enemyPrefabs[Enemy.eType.ant].transform.localScale = new Vector3(GridSize, GridSize, 1.0f);

		enemyPrefabs.Add(Enemy.eType.bomb, Resources.Load<GameObject>("Prefabs/healerEnemy"));
		enemyPrefabs[Enemy.eType.bomb].transform.localScale = new Vector3(GridSize, GridSize, 1.0f);

		enemyPrefabs.Add(Enemy.eType.healer, Resources.Load<GameObject>("Prefabs/bombEnemy"));
		enemyPrefabs[Enemy.eType.healer].transform.localScale = new Vector3(GridSize, GridSize, 1.0f);

		enemyPrefabs.Add(Enemy.eType.boss, Resources.Load<GameObject>("Prefabs/boss"));
		enemyPrefabs[Enemy.eType.boss].transform.localScale = new Vector3(GridSize * 1.5f, GridSize * 1.5f , 1.0f);

		//Load all the prefabs we need for map generation (note that these must be in a "Resources" folder)
		Prefabs = new Dictionary<string, GameObject>();
		Prefabs.Add("ExitIndicator", Resources.Load<GameObject>("Prefabs/ExitIndicator"));
		Prefabs["ExitIndicator"].transform.localScale = new Vector3(GridSize/2, GridSize/2, -2.0f);

		Prefabs.Add("portal", Resources.Load<GameObject>("Prefabs/Portal"));
		Prefabs.Add("enemy", Resources.Load<GameObject>("Prefabs/BaseEnemy"));
		Prefabs.Add("fast", Resources.Load<GameObject>("Prefabs/FastEnemy"));
		Prefabs.Add("spread", Resources.Load<GameObject>("Prefabs/SpreadEnemy"));
		Prefabs.Add("tank", Resources.Load<GameObject>("Prefabs/TankEnemy"));
		Prefabs.Add("ultra", Resources.Load<GameObject>("Prefabs/UltraEnemy"));
		Prefabs.Add("boss", Resources.Load<GameObject>("Prefabs/BossEnemy"));
		Prefabs.Add("heart", Resources.Load<GameObject>("Prefabs/HeartPickup"));
		Prefabs.Add("healthboost", Resources.Load<GameObject>("Prefabs/HealthBoost"));
		Prefabs.Add("shotboost", Resources.Load<GameObject>("Prefabs/ShotBoost"));
		Prefabs.Add("speedboost", Resources.Load<GameObject>("Prefabs/SpeedBoost"));
		Prefabs.Add("silverkey", Resources.Load<GameObject>("Prefabs/SilverKey"));
		Prefabs.Add("goldkey", Resources.Load<GameObject>("Prefabs/GoldKey"));
		Prefabs.Add("silverdoor", Resources.Load<GameObject>("Prefabs/SilverDoor"));
		Prefabs["silverdoor"].transform.localScale = new Vector3(GridSize / 2.0f, 1.0f, 1.0f); //Scale the door properly
		Prefabs.Add("golddoor", Resources.Load<GameObject>("Prefabs/GoldDoor"));
		Prefabs["golddoor"].transform.localScale = new Vector3(GridSize / 2.0f, 1.0f, 1.0f); //Scale the door properly

		//Delete everything visible except the hero when reloading       
		//var objsToDelete = FindObjectsOfType<SpriteRenderer>();
		//int totalObjs = objsToDelete.Length;
		//for (int i = 0; i < totalObjs; i++)
		//{
		//	if (objsToDelete[i].gameObject.ToString().StartsWith("Hero") == false)
		//		UnityEngine.Object.DestroyImmediate(objsToDelete[i].gameObject);
		//}

		//Create the tile map
		TileMap = new GameObject[MaxMapSize * MaxMapSize];
		TileMapMidPoint = (MaxMapSize * MaxMapSize) / 2;
		RNG = new System.Random();

		//Create the starting tile
		// SpawnTile(0, 0);

		//Add more PCG logic here...
	}

	public void SpawnNoneTiles()
	{
		Vector3 start = corners[0];
		Vector3 curr = start;
		var halfMap = MaxMapSize / 2;
		var halfGrid = GridSize / 2;
		curr.x += halfGrid;
		curr.y += halfGrid;
		var horzIndex = 0;
		var vertIndex = 0;
		// Vertical
		for (int i = 0; i < MaxMapSize; ++i)
		{
			// Horizontal
			for (int j = 0; j < MaxMapSize; ++j)
			{
				allTiles[horzIndex, vertIndex] =
					Instantiate(tilePrefabs[Tile.Type.none]
						, new Vector3(curr.x, curr.y, 2f)
						, Quaternion.identity);
				curr.x += GridSize;
				allTiles[horzIndex, vertIndex]
					.GetComponent<Tile>().horzIndex
					= horzIndex;
				allTiles[horzIndex, vertIndex]
						.GetComponent<Tile>().vertIndex
					= vertIndex;
				allTiles[horzIndex, vertIndex]
						.GetComponent<Tile>().dirs = 
						new Dictionary<Tile.DirectionValid, bool>();
				++horzIndex;
			}
			horzIndex = 0;
			++vertIndex;
			curr.x = start.x + halfGrid;
			curr.y += GridSize;
		}
		

	}

	public void SpawnPCGFloors()
	{
		Vector3 start = corners[0];
		Vector3 curr = start;
		var halfMap = MaxMapSize / 2;
		var halfGrid = GridSize / 2;
		start.x += halfGrid; start.y += halfGrid;
		var currTileObj = allTiles[halfMap, halfMap];
		currTileObj = 
			CreateTileIndex(Tile.Type.floor, halfMap, halfMap, currTileObj);
		curr.x += halfGrid;
		curr.y += halfGrid;
		Tile.DirectionValid dirs = Tile.DirectionValid.north;
		var currTile = currTileObj.GetComponent<Tile>();
		
		// Portal is created & placed in SceneDirector.cs

		// Rooms with random size & location
		// Rooms will always have an entrance & exit.
		//		- Both of these are branch tiles
		//		- As it stands, rooms must contain an odd number of floors.
		for(int i = 0; i < numRooms; ++i)
		{
			var randIndX = RNG.Next(0, MaxMapSize - 1);
			var randIndY = RNG.Next(0, MaxMapSize - 1);
			var randW = RNG.Next(5, 10);
			var randH = RNG.Next(5, 10);
			randW = (randW % 2 == 0) ? (++randW) : (randW);
			randH = (randH % 2 == 0) ? (++randH) : (randH);
			var randTile = GetTile(randIndX, randIndY);
			SpawnRoom(randTile.GetComponent<Tile>(), randW, randH);

		}
		


		while(branchTiles.Count > 0)
		{
			List<Tile> prevBranch = new List<Tile>(branchTiles);
			List<Tile> toDelete = new List<Tile>();
			for (int i = 0; i < prevBranch.Count; ++i)
			{
				var tile = prevBranch[i];
		
				// TODO: TryPlaceRandom is broken. 
				SpawnHallway(tile, RNG.Next(15, 30));
				SpawnHallway(tile, RNG.Next(8, 15));
				////SpawnHallway(ref tile, RNG.Next(8, 15));
				var dir = GetRandomDirection();
				if(dir != Tile.DirectionValid.none)
				{
					SpawnHallWayTwisty(tile, RNG.Next(10, 26), 2, dir);
					SpawnHallWayTwisty(tile, RNG.Next(10, 26), 4, dir);
				}
				
				toDelete.Add(tile);
			}
			foreach(var tile in toDelete)
			{
				branchTiles.Remove(tile);
			}
		}

		// Place map boundaries
		GameObject prevTileObj = allTiles[0, 0].gameObject;
		for (int i = 0; i < MaxMapSize; ++i)
		{

			CreateTileIndex_Ignore(Tile.Type.outerWall, i, 0);
			prevTileObj = allTiles[0, i].gameObject;
			CreateTileIndex_Ignore(Tile.Type.outerWall, 0, i);
		}
		for (int i = MaxMapSize - 1; i >= 0; --i)
		{
			CreateTileIndex_Ignore(Tile.Type.outerWall, i, MaxMapSize - 1);
			CreateTileIndex_Ignore(Tile.Type.outerWall, MaxMapSize - 1, i);
		}
		// Fill remainder with walls
		foreach (GameObject tileObj in allTiles)
		{
			var tile = tileObj.GetComponent<Tile>();
			if(tile.type == Tile.Type.none)
			{
				Destroy(tile);
				CreateTileIndex_Ignore(Tile.Type.wall, tile.horzIndex, tile.vertIndex);
				
			}
		}

		
	}
	
	public void SpawnHallWayTwisty(Tile curr, int hallLength
								 , int twistIntensity, Tile.DirectionValid dir)
	{
		Tile endTile;
		Vector2 startCoord = new Vector2(curr.horzIndex, curr.vertIndex);
		Vector2 endCoord = startCoord;
		var topLeft = startCoord;
		var bottomRight = startCoord;
		bool no = false;
		var currCoord = new Vector2(curr.horzIndex, curr.vertIndex);
		if (dir==Tile.DirectionValid.east || dir == Tile.DirectionValid.west)
		{
			// Using ternary here to cut down on code length
			endCoord.x = (dir == Tile.DirectionValid.east) ?
				(endCoord.x + hallLength) : (endCoord.x - hallLength);
			var incrementX = (dir == Tile.DirectionValid.east) ? (1) : (-1);
			
			currCoord = new Vector2(curr.horzIndex, curr.vertIndex);
			var flip = RNG.Next(0, 2);

			// Tails go north first
			
			var numTwists = 0;
			var incrementY = (flip == 0) ? (1) : (-1);
			while (numTwists < twistIntensity)
			{
				for (int i = 0; i < (hallLength / twistIntensity); ++i)
				{
					var type = (RNG.Next(0, 10) == 1) ?
						(Tile.Type.floorDirty) : (Tile.Type.floor);

					var currObj = GetTile((int)currCoord.x, (int)currCoord.y);
					var currTile = currObj.GetComponent<Tile>();
					if (Check360(currTile, Tile.Type.wall, 2) >= 2)
					{
						FillArea(
							new Vector2(currTile.horzIndex - 1, currTile.vertIndex + 1)
							, new Vector2(currTile.horzIndex + 1, currTile.vertIndex - 1)
							, Tile.Type.floor, true);

					}
					// L Shape 
					currCoord.x += incrementX;
					currObj = CreateTileIndex(type, (int)currCoord.x, (int)currCoord.y, currObj);

					currCoord.y += incrementY;
					currObj = CreateTileIndex(type, (int)currCoord.x, (int)currCoord.y, currObj);
					curr = currObj.GetComponent<Tile>();
					++numTwists;
				}
				for (int i = 0; i < (hallLength / twistIntensity); ++i)
				{
					var type = (RNG.Next(0, 10) == 1) ?
						(Tile.Type.floorDirty) : (Tile.Type.floor);
					var currObj = GetTile((int)currCoord.x, (int)currCoord.y);
					var currTile = currObj.GetComponent<Tile>();
					if (Check360(currTile, Tile.Type.wall, 2) >= 2)
					{
						FillArea(
							new Vector2(currTile.horzIndex - 1, currTile.vertIndex + 1)
							, new Vector2(currTile.horzIndex + 1, currTile.vertIndex - 1)
							, Tile.Type.floor, true);

					}
					currCoord.x += incrementX;
					currObj = CreateTileIndex(type, (int)currCoord.x, (int)currCoord.y, currObj);
					currCoord.y += -(incrementY);
					currObj = CreateTileIndex(type, (int)currCoord.x, (int)currCoord.y, currObj);
					curr = currObj.GetComponent<Tile>();
					++numTwists;
				}
			}
			
			SpawnHallwayDir(GetTile((int)endCoord.x, (int)endCoord.y).GetComponent<Tile>()
							, MaxMapSize / 5, dir);
		}
		
		if (dir==Tile.DirectionValid.north || dir == Tile.DirectionValid.south)
		{
			endCoord.y = (dir == Tile.DirectionValid.north) ?
				(endCoord.y + hallLength) : (endCoord.y - hallLength);
			var incrementY = (dir == Tile.DirectionValid.north) ? (1) : (-1);
			currCoord = new Vector2(curr.horzIndex, curr.vertIndex);
			var flip = RNG.Next(0, 2);
			// Tails go north first
			var incrementX = (flip == 0) ? (1) : (-1);
			var numTwists = 0;
			while(numTwists < twistIntensity)
			{
				for (int i = 0; i < (hallLength / twistIntensity); ++i)
				{
					var type = (RNG.Next(0, 10) == 1) ?
						(Tile.Type.floorDirty) : (Tile.Type.floor);
					var currObj = GetTile((int)currCoord.x, (int)currCoord.y);
					var currTile = currObj.GetComponent<Tile>();
					if (Check360(currTile, Tile.Type.wall, 2) >= 2)
					{
						FillArea(
							new Vector2(currTile.horzIndex - 1, currTile.vertIndex + 1)
							, new Vector2(currTile.horzIndex + 1, currTile.vertIndex - 1)
							, Tile.Type.floor, true);

					}
					currCoord.y += incrementY;
					currObj = CreateTileIndex(type, (int)currCoord.x, (int)currCoord.y, currObj);
					currCoord.x += incrementX;
					currObj = CreateTileIndex(type, (int)currCoord.x, (int)currCoord.y, currObj);
					curr = currObj.GetComponent<Tile>();
					++numTwists;
				}
				for (int i = 0; i < (hallLength / twistIntensity); ++i)
				{
					var type = (RNG.Next(0, 10) == 1) ?
						(Tile.Type.floorDirty) : (Tile.Type.floor);
					var currObj = GetTile((int)currCoord.x, (int)currCoord.y);
					var currTile = currObj.GetComponent<Tile>();
					if (Check360(currTile, Tile.Type.wall, 2) >= 2)
					{
						FillArea(
							new Vector2(currTile.horzIndex - 1, currTile.vertIndex + 1)
							, new Vector2(currTile.horzIndex + 1, currTile.vertIndex - 1)
							, Tile.Type.floor, true);

					}
					currCoord.y += incrementY;
					currObj = CreateTileIndex(type, (int)currCoord.x, (int)currCoord.y, currObj);
					currCoord.x += -(incrementX);
					currObj = CreateTileIndex(type, (int)currCoord.x, (int)currCoord.y, currObj);
					curr = currObj.GetComponent<Tile>();
					++numTwists;
				}
				
			}
			SpawnHallwayDir(GetTile((int)endCoord.x, (int)endCoord.y).GetComponent<Tile>()
				, MaxMapSize/ 5, dir);

		}
		
		
		//FillArea(topLeft, bottomRight, Tile.Type.wall);
		
		if(RNG.Next(0, 4) == 1)
		{
			AddBranch(curr);
		}
		

	}

	public void ContextSpawnTile(ref Tile curr
									 , Tile.Type type
									 , Tile.DirectionValid direction)
	{
		if (curr == null)
			return;
		int hIndex = curr.horzIndex;
		int vIndex = curr.vertIndex;
		Vector3 pos = new Vector3(0, 0, 0);
		bool canPlace = true;
		switch (direction)
		{
			case (Tile.DirectionValid.north):
			{
				canPlace = CheckNorth(curr);
				if(canPlace)
				{
					vIndex += 1;
					pos = allTiles[hIndex, vIndex].transform.position;
				}

			}break;
			case (Tile.DirectionValid.west):
			{
				canPlace = CheckWest(curr);
				if(canPlace)
				{
					hIndex -= 1;
					pos = allTiles[hIndex, vIndex].transform.position;
				}

			}break;
			case (Tile.DirectionValid.south):
			{
				canPlace = CheckSouth(curr);
				if(canPlace)
				{
					vIndex -= 1;
					pos = allTiles[hIndex, vIndex].transform.position;
				}

			}break;
			case (Tile.DirectionValid.east):
			{
				canPlace = CheckEast(curr);
				if(canPlace)
				{
					hIndex += 1;
					pos = allTiles[hIndex, vIndex].transform.position;
				}
				
				
			}break;
		}

		CreateTileIndex_Replace(GetTile(hIndex, vIndex) .GetComponent<Tile>(), Tile.Type.floorDirty);
		curr = allTiles[hIndex, vIndex].GetComponent<Tile>();
		

	}

	public void SpawnExitPaths(Tile portalTile)
	{
		var x = portalTile.horzIndex;
		var y = portalTile.vertIndex;
		var halfMap = MaxMapSize / 4;

		for(int i = 1; i < halfMap; ++i)
		{
			if(CheckIndex(x, y + i, true))
			{
				CreateTileIndex_Ignore(Tile.Type.exitPathN, x, y + i);
			}
			
		}
		for (int i = 1; i < halfMap; ++i)
		{
			if(CheckIndex(x, y-i, true))
			{
				CreateTileIndex_Ignore(Tile.Type.exitPathN, x, y - i);
			}
		}
		for (int i = 1; i < halfMap; ++i)
		{
			if (CheckIndex(x + i, y, true))
			{
				CreateTileIndex_Ignore(Tile.Type.exitPathE, x + i, y);
			}

		}
		for (int i = 1; i < halfMap; ++i)
		{
			if (CheckIndex(x - i, y, true))
			{
				CreateTileIndex_Ignore(Tile.Type.exitPathE, x - i, y);
			}
		}
	}
	
	public void SpawnHallwayDir(Tile curr, int hallLength, Tile.DirectionValid dir)
	{
		
		for (int i = 0; i < hallLength; ++i)
		{
			var floorType =
					(RNG.Next(0, 10) == 1) ? (Tile.Type.floorDirty) : (Tile.Type.floor);
			
			ContextSpawnTile(ref curr, floorType, dir);
			if(curr.type == Tile.Type.outerWall)
			{
				break;
			}
		}

	}

	public Tile.DirectionValid SpawnHallway(Tile curr, int hallLength)
	{
		var dir = GetRandomDirection();
		if(dir != Tile.DirectionValid.none)
		{
			for (int i = 0; i < hallLength; ++i)
			{
				var floorType =
					(RNG.Next(0, 10) == 1) ? (Tile.Type.floorDirty) : (Tile.Type.floor);
				ContextSpawnTile(ref curr, floorType, dir);
			}
			
		}
		return dir;
	}

	// Spawn a room from center tile
	public Tile SpawnRoom(Tile center, int roomWidth, int roomHeight)
	{
		GameObject curr = center.gameObject;
		int hIndex = center.horzIndex;
		int vIndex = center.vertIndex;
		// Corners
		int minH = (hIndex - (roomWidth/2)+ 1) ;
		int maxH = (hIndex + (roomWidth/2) + 1) ;
		int minV = (vIndex - (roomHeight/2) + 1);
		int maxV = (vIndex + (roomHeight/2) + 1);
		// Room boundaries
		for (int i = 0; i < roomWidth; ++i)
		{
			int top = minH + i;
			// North and South Walls
			if(CheckIndex(top, minV, false))
			{
				curr = CreateTileIndex(Tile.Type.wall, top, minV, curr);
			}
			if(CheckIndex(top, maxV, false))
			{
				curr = CreateTileIndex(Tile.Type.wall, top, maxV, curr);
			}
			
		}
		for (int i = 0; i < roomHeight; ++i)
		{
			int v = minV + i;
			// East and West walls
			if(CheckIndex(minH, v, false))
			{
				curr = CreateTileIndex(Tile.Type.wall, minH, v, curr);
			}
			if(CheckIndex(maxH, v, false))
			{
				curr = CreateTileIndex(Tile.Type.wall, maxH, v, curr);
			}
		}

		// Fill room with floor
		Tile.Type floorType =
			(RNG.Next(0, 10) == 1) ? (Tile.Type.floorDirty) 
			                       : (Tile.Type.floor);
		var fillY = minV + 1;
		var fillX = minH + 1;
		for(int i = 0; i < roomWidth - 1; ++i)
		{
			for(int j = 0; j < roomHeight - 1; ++j)
			{
				if(CheckIndex(fillX + i, fillY + j, false))
				{
					CreateTileIndex_Ignore(floorType, fillX + i, fillY + j);
				}
				floorType =
					(RNG.Next(0, 10) == 1) ? (Tile.Type.floorDirty) 
					                       : (Tile.Type.floor);

			}
		}


		// If center vert index < MaxMapWidth / 2
		//	branch off north over south, else vice versa
		// if center horz index < MaxMapWidth / 2
		//	Branch off east over west, else vice versa
		var tile = curr.GetComponent<Tile>();
		var indexY = 0;
		var indexX = 0;
		int variantX = RNG.Next(2, (roomWidth / 2));
		int variantY = RNG.Next(2, (roomHeight / 2));

		if (center.vertIndex < (MaxMapSize / 2))
		{
			indexY = center.vertIndex + (roomHeight / 2);
		}
		else
		{
			indexY = center.vertIndex - (roomHeight / 2);
			variantY = -variantY;
			
		}
		if (center.horzIndex < (MaxMapSize / 2))
		{
			indexX = center.horzIndex + (roomWidth / 2);
		}
		else
		{
			indexX = center.horzIndex - (roomWidth / 2);
			variantX = -variantX;
		}
		++indexY;
		++indexX;

		// Randomly use dirty floor texture
		floorType =
			(RNG.Next(0, 10) == 1) ? (Tile.Type.floorDirty) : (Tile.Type.floor);

		var branchTile0 = allTiles[indexX, center.vertIndex];
		Destroy(branchTile0);
		branchTiles.Add
		(CreateTileIndex_Ignore(floorType
		, indexX, center.vertIndex).GetComponent<Tile>());
		// Doorway N/S
		if(variantY < 0)
		{
			for (int i = variantY; i < 0; ++i)
			{
				floorType =
					(RNG.Next(0, 10) == 1) ? (Tile.Type.floorDirty) : (Tile.Type.floor);
				var branchTile1 = allTiles[indexX, center.vertIndex + i];
				var newBranch = CreateTileIndex_Ignore(floorType
						, indexX, center.vertIndex + i)
					.GetComponent<Tile>();
				if(AddBranch(newBranch))
				{
					Destroy(branchTile1);
				}
				
			}
		}
		else
		{
			for (int i = 0; i < variantY; ++i)
			{
				floorType =
					(RNG.Next(0, 10) == 1) ? (Tile.Type.floorDirty) : (Tile.Type.floor);
				var branchTile1 = allTiles[indexX, center.vertIndex + i];
				var newBranch = CreateTileIndex_Ignore(floorType
						, indexX, center.vertIndex + i)
					.GetComponent<Tile>();
				if(AddBranch(newBranch))
				{
					Destroy(branchTile1);
				}
				
				
			}
		}

		floorType =
			(RNG.Next(0, 10) == 1) ? (Tile.Type.floorDirty) : (Tile.Type.floor);
		var branchTile2 = allTiles[center.horzIndex, indexY];
		var newBranch0 = CreateTileIndex_Ignore(floorType
			, center.horzIndex, indexY).GetComponent<Tile>();
		if(AddBranch(newBranch0))
		{
			Destroy(branchTile2);
		}
		
		
		
		//Doorway W/E
		if(variantX < 0)
		{
			for (int i = variantX; i < 0; ++i)
			{
				floorType =
					(RNG.Next(0, 10) == 1) ? (Tile.Type.floorDirty) : (Tile.Type.floor);
				var branchTile3 = allTiles[center.horzIndex + i, indexY];
				var newBranch = CreateTileIndex_Ignore(floorType, center.horzIndex + i, indexY)
					.GetComponent<Tile>();
				if(AddBranch(newBranch))
				{
					Destroy(branchTile3);
				}
				
				
			}
		}
		else
		{
			for (int i = 0; i < variantX; ++i)
			{
				floorType =
					(RNG.Next(0, 10) == 1) ? (Tile.Type.floorDirty) : (Tile.Type.floor);
				var branchTile3 = allTiles[center.horzIndex + i, indexY];
				var newBranch = CreateTileIndex_Ignore(floorType, center.horzIndex + i, indexY).GetComponent<Tile>();
				if (AddBranch(newBranch))
				{
					Destroy(branchTile3);
				}
				
			}
		}

		if(center != playerSpawnTile)
		{
			createEnemy(Enemy.eType.ant, center.gameObject.transform.position);
		}
		
		
		
		return tile;
	}

	public Enemy createEnemy(Enemy.eType type, Vector3 pos)
	{
		var enemy =
			Instantiate(enemyPrefabs[type]);
		enemy.transform.position = new Vector3(pos.x, pos.y, 1.0f);
		var enemyComp = enemy.GetComponent<Enemy>();
		enemyComp.director = parent;
		return enemy.GetComponent<Enemy>();
	}


	/////////////////////////////////////////////////////////////
	/// UTILITY

	public void Replace360(Tile curr, Tile.Type type)
	{
		var tile = GetTile(curr.horzIndex, curr.vertIndex).GetComponent<Tile>();
		for (int i = 0; i < 1; ++i)
		{
			for (int j = 0; j < 1; ++j)
			{
				tile = GetTile(curr.horzIndex + i, curr.vertIndex + j)
					.GetComponent<Tile>();
				CreateTileIndex_Replace(tile, type);
				tile = GetTile(curr.horzIndex - i, curr.vertIndex - j)
					.GetComponent<Tile>();
				CreateTileIndex_Replace(tile, type);
				

			}
		}
	}

	public int Check360(Tile curr, Tile.Type checkFor, int radius)
	{
		int numPresent = 0;
		var tile = GetTile(curr.horzIndex + 1, curr.vertIndex).GetComponent<Tile>();
		for (int i = 0; i < radius; ++i)
		{
			for(int j = 0; j < radius; ++j)
			{
				tile = GetTile(curr.horzIndex + i, curr.vertIndex + j)
					.GetComponent<Tile>();
				if (tile.type == checkFor)
					++numPresent;
				tile = GetTile(curr.horzIndex - i, curr.vertIndex - j)
					.GetComponent<Tile>();
				if (tile.type == checkFor)
					++numPresent;

			}
		}
		return numPresent;
	}

	// Put walls next to all floors
	public void CreateAreaWalls(Vector2 TLCorner, Vector2 BRCorner)
	{
		var width = BRCorner.x - TLCorner.x;
		var height = TLCorner.y - BRCorner.y;
		int startX = (int)TLCorner.x;
		int startY = (int)TLCorner.y;
		int currX = startX;
		int currY = startY;

		for (int i = 1; i < width; ++i)
		{
			for (int j = 1; j < height; ++j)
			{
				var prevX = currX - i;
				var prevY = currY - j;
				var nextX = currX + i;
				var nextY = currY + j;
				var currTile = GetTile(currX, currY).GetComponent<Tile>();
				if(currTile.type == Tile.Type.floor 
					|| currTile.type == Tile.Type.floorDirty)
				{
					CreateTileIndex(Tile.Type.wall, prevX, prevY, currTile.gameObject);
					CreateTileIndex(Tile.Type.wall, nextX, nextY, currTile.gameObject);
				}
			}
		}
	}

	public void FillArea(Vector2 TLCorner, Vector2 BRCorner, Tile.Type fillType, bool ignore)
	{
		var width = Math.Abs(BRCorner.x - TLCorner.x);
		var height = Math.Abs(TLCorner.y - BRCorner.y);
		int startX = (int)TLCorner.x;
		int startY = (int)TLCorner.y;
		for(int i = 0; i < width; ++i)
		{
			for(int j = 0; j < height; ++j)
			{
				var currTile = GetTile(startX + i, startY + j)
					.GetComponent<Tile>();
				if(currTile.type == Tile.Type.none || ignore)
				{
					CreateTileIndex_Replace(currTile, fillType);
				}

			}
		}
	}

	public GameObject GetTile(int horzIndex, int vertIndex)
	{
		if(CheckIndex(horzIndex, vertIndex, false))
		{
			return allTiles[horzIndex, vertIndex];
		}
		var validIndexHorz = horzIndex;
		var validIndexVert = vertIndex;
		if(horzIndex >= MaxMapSize)
		{
			validIndexHorz = MaxMapSize - 2;
		}
		if(horzIndex < 0)
		{
			validIndexHorz = 2;
		}
		if(vertIndex >= MaxMapSize)
		{
			validIndexVert = MaxMapSize - 2;
		}
		if(vertIndex < 0)
		{
			validIndexVert = 2;
		}
		return allTiles[validIndexHorz, validIndexVert];
	}

	public bool AddBranch(Tile tile)
	{
		if(branchTiles.Count < maxBranches)
		{
			branchTiles.Add(tile);
			return true;
		}
		return false;
	}

	public Tile.DirectionValid TryPlaceRandom(Tile curr)
	{
		CheckNorth(curr);
		CheckEast(curr);
		CheckSouth(curr);
		CheckWest(curr);
		return GetRand(curr);
	}
	
	public Tile.DirectionValid GetRandomDirection()
	{
		switch(RNG.Next(0,4))
		{
			case (0): return Tile.DirectionValid.north;
			case (1): return Tile.DirectionValid.east;
			case (2): return Tile.DirectionValid.south;
			case (3): return Tile.DirectionValid.west;
			default: return Tile.DirectionValid.north;
		}
	}

	public Tile.DirectionValid GetRand(Tile curr)
	{
		bool nValid = curr.dirs[Tile.DirectionValid.north];
		bool eValid = curr.dirs[Tile.DirectionValid.east];
		bool sValid = curr.dirs[Tile.DirectionValid.south];
		bool wValid = curr.dirs[Tile.DirectionValid.west];
		List<Tile.DirectionValid> dirsValid = new List<Tile.DirectionValid>();
		
		foreach(Tile.DirectionValid valid in curr.dirs.Keys)
		{
			var branch = RNG.Next(0, 10);
			if(curr.dirs[valid])
			{
				dirsValid.Add(valid);
			}
		}
		if(dirsValid.Count == 0)
		{
			return Tile.DirectionValid.none;
		}
		int rand = RNG.Next(0, dirsValid.Count);
		return dirsValid[rand];

	}
	
	public bool CheckIndex(int hIndex, int vIndex, bool ignoreRules)
	{
		if(hIndex >= MaxMapSize || hIndex < 0)
		{
			return false;
		}
		if (vIndex >= MaxMapSize || vIndex < 0)
		{
			return false;
		}
		if (hIndex > MaxMapSize - 2 
			|| hIndex < 1)
		{
			return false;
		}
		if(vIndex > MaxMapSize - 2
			|| vIndex < 1)
		{
			return false;
		}
		var tile = allTiles[hIndex, vIndex].GetComponent<Tile>();
		if (tile.type == Tile.Type.outerWall)
		{
			return false;
		}
		if (tile.type != Tile.Type.none && !ignoreRules)
		{
			return false;
		}
		return true;
	}

	public bool CheckNorth(Tile curr)
	{
		if (!curr)
			return false;
		if (curr.vertIndex + 1 > MaxMapSize - 2)
		{
			curr.dirs[Tile.DirectionValid.north] = false;
			return false;
		}

		var northTile = 
			GetTile(curr.horzIndex, curr.vertIndex + 1).GetComponent<Tile>();

		if (northTile.type == Tile.Type.outerWall)
		{
			//curr.dirs[Tile.DirectionValid.north] = false;
			return false;
		}

		if(northTile.type != Tile.Type.none)
		{
			//curr.dirs[Tile.DirectionValid.north] = false;
			return false;
		}
		curr.dirs[Tile.DirectionValid.north] = true;
		return true;
	
	}

	public bool CheckEast(Tile curr)
	{
		if (!curr)
			return false;
		if (curr.horzIndex + 1 > MaxMapSize - 2)
		{
			curr.dirs[Tile.DirectionValid.east] = false;
			return false;
		}
		var eastTile = 
			GetTile(curr.horzIndex + 1, curr.vertIndex).GetComponent<Tile>();

		if (eastTile.type == Tile.Type.outerWall)
		{
			curr.dirs[Tile.DirectionValid.east] = false;
			return false;
		}
		
		if (eastTile.type != Tile.Type.none)
		{
			curr.dirs[Tile.DirectionValid.east] = false;
			return false;
		}
		curr.dirs[Tile.DirectionValid.east] = true;
		return true;
	}

	public bool CheckSouth(Tile curr)
	{
		if (!curr)
			return false;
		if (curr.vertIndex - 1 < 1)
		{
			curr.dirs[Tile.DirectionValid.south] = false;
			return false;
		}
		var southTile = 
			GetTile(curr.horzIndex, curr.vertIndex - 1).GetComponent<Tile>();

		if (southTile.type == Tile.Type.outerWall)
		{
			curr.dirs[Tile.DirectionValid.south] = false;
			return false;
		}

		if (southTile.type != Tile.Type.none)
		{
			curr.dirs[Tile.DirectionValid.south] = false;
			return false;
		}
		curr.dirs[Tile.DirectionValid.south] = true;
		return true;
	}

	public bool CheckWest(Tile curr)
	{
		if (!curr)
			return false;
		if (curr.horzIndex - 1 < 1)
		{
			curr.dirs[Tile.DirectionValid.west] = false;
			return false;
		}
		var westTile = GetTile(curr.horzIndex - 1, curr.vertIndex).GetComponent<Tile>();

		if (westTile.type == Tile.Type.outerWall)
		{
			curr.dirs[Tile.DirectionValid.west] = false;
			return false;
		}

		if (westTile.type != Tile.Type.none)
		{
			curr.dirs[Tile.DirectionValid.west] = false;
			return false;
		}
		curr.dirs[Tile.DirectionValid.west] = true;
		return true;

	}


	// Replaces specified tile
	// TODO: Need to verify input replace doesnt need to be a ref
	public GameObject CreateTileIndex_Replace(Tile replaceTile, Tile.Type type)
	{
		var tile = Instantiate(tilePrefabs[type]
			, new Vector3(replaceTile.transform.position.x
				, replaceTile.transform.position.y, 2f)
			, Quaternion.identity).GetComponent<Tile>();
		tile.dirs = new Dictionary<Tile.DirectionValid, bool>();
		// Start with all directions valid
		tile.dirs.Add(Tile.DirectionValid.north, true);
		tile.dirs.Add(Tile.DirectionValid.east, true);
		tile.dirs.Add(Tile.DirectionValid.south, true);
		tile.dirs.Add(Tile.DirectionValid.west, true);
		tile.dirs.Add(Tile.DirectionValid.none, false);
		tile.type = type;
		tile.horzIndex = replaceTile.horzIndex;
		tile.vertIndex = replaceTile.vertIndex;
		tile.branch = false;

		Destroy(allTiles[replaceTile.horzIndex, replaceTile.vertIndex]);
		allTiles[replaceTile.horzIndex, replaceTile.vertIndex] = tile.gameObject;

		// Chance determined by branchChance
		// IE, branchChance == 10 , 10% chance, 
		//					== 5  , 20%
		//					== 100, 1% 
		if (type == Tile.Type.floor || type == Tile.Type.floorDirty
		                            || type == Tile.Type.exitPathN || tile.type == Tile.Type.exitPathE)
		{
			var doBranch = RNG.Next(0, branchChance);
			if (doBranch == 0 && branchTiles.Count < maxBranches
			                  && TryPlaceRandom(tile.GetComponent<Tile>())
			                  != Tile.DirectionValid.none)
			{
				tile.branch = true;
				AddBranch(tile);
			}
		}

		return tile.gameObject;
	}

	// Replaces a none Tile, ignores indexing and wall rules
	public GameObject CreateTileIndex_Ignore(Tile.Type type, int hIndex, int vIndex)
	{
		var replaceTile = allTiles[hIndex, vIndex];
		var tile = Instantiate(tilePrefabs[type]
			, new Vector3(replaceTile.transform.position.x
				, replaceTile.transform.position.y, 2f)
			, Quaternion.identity).GetComponent<Tile>();
		tile.dirs = new Dictionary<Tile.DirectionValid, bool>();
		// Start with all directions valid
		tile.dirs.Add(Tile.DirectionValid.north, true);
		tile.dirs.Add(Tile.DirectionValid.east, true);
		tile.dirs.Add(Tile.DirectionValid.south, true);
		tile.dirs.Add(Tile.DirectionValid.west, true);
		tile.dirs.Add(Tile.DirectionValid.none, false);
		tile.type = type;
		tile.horzIndex = hIndex;
		tile.vertIndex = vIndex;
		tile.branch = false;

		Destroy(allTiles[hIndex, vIndex]);
		allTiles[hIndex, vIndex] = tile.gameObject;

		// Chance determined by branchChance
		// IE, branchChance == 10 , 10% chance, 
		//					== 5  , 20%
		//					== 100, 1% 
		if (type == Tile.Type.floor || type == Tile.Type.floorDirty
		                            || type == Tile.Type.exitPathN || tile.type == Tile.Type.exitPathE)
		{
			var doBranch = RNG.Next(0, branchChance);
			if (doBranch == 0 && branchTiles.Count < maxBranches
			                  && TryPlaceRandom(tile.GetComponent<Tile>())
			                  != Tile.DirectionValid.none)
			{
				tile.branch = true;
				AddBranch(tile);
			}
		}

		return tile.gameObject;
	}

	public GameObject CreateTileIndex(Tile.Type type, int hIndex, int vIndex, GameObject prev)
	{
		// Will return false if tile to replace is not none or if is outerwall
		if (!CheckIndex(hIndex, vIndex, false))
			return prev;
		var replaceTile = allTiles[hIndex, vIndex];
		var tile = Instantiate(tilePrefabs[type]
			, new Vector3(replaceTile.transform.position.x
				                , replaceTile.transform.position.y, 2f)
			, Quaternion.identity).GetComponent<Tile>();
		tile.dirs = new Dictionary<Tile.DirectionValid, bool>();
		// Start with all directions valid
		tile.dirs.Add(Tile.DirectionValid.north, true);
		tile.dirs.Add(Tile.DirectionValid.east, true);
		tile.dirs.Add(Tile.DirectionValid.south, true);
		tile.dirs.Add(Tile.DirectionValid.west, true);
		tile.dirs.Add(Tile.DirectionValid.none, false);
		tile.type = type;
		tile.horzIndex = hIndex;
		tile.vertIndex = vIndex;
		tile.branch = false;

		Destroy(allTiles[hIndex, vIndex]);
		allTiles[hIndex, vIndex] = tile.gameObject;

		// Chance determined by branchChance
		// IE, branchChance == 10 , 10% chance, 
		//					== 5  , 20%
		//					== 100, 1% 
		if(type == Tile.Type.floor || type == Tile.Type.floorDirty
			|| type == Tile.Type.exitPathN || tile.type == Tile.Type.exitPathE)
		{
			var doBranch = RNG.Next(0, branchChance);
			if (doBranch == 0 && branchTiles.Count < maxBranches
			                  && TryPlaceRandom(tile.GetComponent<Tile>())
			                  != Tile.DirectionValid.none)
			{
				tile.branch = true;
				AddBranch(tile);
			}
		}

		return tile.gameObject;
	}

	public GameObject CreateTilePostion(Tile.Type type, Vector2 pos)
	{
		var tile = Instantiate(tilePrefabs[type]
			, new Vector3(pos.x, pos.y, 2f)
			, Quaternion.identity).GetComponent<Tile>();
		tile.dirs = new Dictionary<Tile.DirectionValid, bool>();
		// Start with all directions valid
		tile.dirs.Add(Tile.DirectionValid.north, true);
		tile.dirs.Add(Tile.DirectionValid.east, true);
		tile.dirs.Add(Tile.DirectionValid.south, true);
		tile.dirs.Add(Tile.DirectionValid.west, true);
		tile.dirs.Add(Tile.DirectionValid.none, false);
		tile.branch = false;
		tile.type = type;
		return tile.gameObject;
	}

	//Spawn a tile object if one isn't already there
	void SpawnTile(int x, int y)
	{
		if (GetTile(x,y) != null)
			return;
		TileMap[(y * MaxMapSize) + x + TileMapMidPoint] = Spawn("floor", x, y);
	}

	//Spawn any object
	GameObject Spawn(string obj, float x, float y)
	{
		return Instantiate(Prefabs[obj], new Vector3(x * GridSize, y * GridSize, 0.0f), Quaternion.identity);
	}

	//Spawn any object rotated 90 degrees left
	GameObject SpawnRotateLeft(string obj, float x, float y)
	{
		return Instantiate(Prefabs[obj], new Vector3(x * GridSize, y * GridSize, 0.0f), Quaternion.AngleAxis(-90, Vector3.forward));
	}

	//Spawn any object rotated 90 degrees right
	GameObject SpawnRotateRight(string obj, float x, float y)
	{
		return Instantiate(Prefabs[obj], new Vector3(x * GridSize, y * GridSize, 0.0f), Quaternion.AngleAxis(90, Vector3.forward));
	}
}


//case (Tile.DirectionValid.east):
//			{
//	Vector2 endCoord = new Vector2(curr.horzIndex + hallLength, curr.vertIndex);
//	var currCoord = new Vector2(curr.horzIndex, curr.vertIndex);
//	var prevFlip = RNG.Next(0, 2);
//	for (int i = 0; i < hallLength; ++i)
//	{
//		// Coin flip. 
//		// 0 heads == go north
//		// 1 tails == go south
//		if (prevFlip == 0)
//		{
//			++currCoord.y;
//		}
//		else
//		{
//			--currCoord.y;
//		}
//		var tileType = (RNG.Next(0, 10) == 1) ?
//				(Tile.Type.floorDirty) : (Tile.Type.floor);
//
//		//++currCoord.x;
//		if (CheckIndex((int)currCoord.x, (int)currCoord.y, false))
//		{
//			curr =
//				CreateTileIndex(tileType, (int)currCoord.x, (int)currCoord.x)
//					.GetComponent<Tile>();
//		}
//		currCoord.x += i;
//		if (CheckIndex((int)currCoord.x, (int)currCoord.y, false))
//		{
//			curr =
//			CreateTileIndex(tileType, (int)currCoord.x, (int)currCoord.x)
//			.GetComponent<Tile>();
//		}
//		prevFlip = RNG.Next(0, 2);
//	}
//
//}
//break;
//		case (Tile.DirectionValid.west):
//			{
//	Vector2 endCoord = new Vector2(curr.horzIndex - hallLength, curr.vertIndex);
//	var currCoord = new Vector2(curr.horzIndex, curr.vertIndex);
//	var prevFlip = RNG.Next(0, 2);
//	while (currCoord.x != endCoord.x)
//	{
//		// Coin flip. 
//		// 0 heads == go north
//		// 1 tails == go south
//		if (prevFlip == 0)
//		{
//			++currCoord.y;
//		}
//		else
//		{
//			--currCoord.y;
//		}
//		var tileType = (RNG.Next(0, 10) == 1) ?
//			(Tile.Type.floorDirty) : (Tile.Type.floor);
//
//		//++currCoord.x;
//		if (CheckIndex((int)currCoord.x, (int)currCoord.y, false))
//		{
//			curr =
//				CreateTileIndex(tileType, (int)currCoord.x, (int)currCoord.x)
//					.GetComponent<Tile>();
//		}
//		--currCoord.x;
//		if (CheckIndex((int)currCoord.x, (int)currCoord.y, false))
//		{
//			curr =
//				CreateTileIndex(tileType, (int)currCoord.x, (int)currCoord.x)
//					.GetComponent<Tile>();
//		}
//		prevFlip = RNG.Next(0, 2);
//	}
//
//}
//break;
//		case (Tile.DirectionValid.north):
//			{
//	Vector2 endCoord = new Vector2(curr.horzIndex, curr.vertIndex + hallLength);
//	var currCoord = new Vector2(curr.horzIndex, curr.vertIndex);
//	var prevFlip = RNG.Next(0, 2);
//	while (currCoord.y != endCoord.y)
//	{
//		// Coin flip. 
//		// 0 heads == go north
//		// 1 tails == go south
//		if (prevFlip == 0)
//		{
//			++currCoord.x;
//		}
//		else
//		{
//			--currCoord.x;
//		}
//		var tileType = (RNG.Next(0, 10) == 1) ?
//			(Tile.Type.floorDirty) : (Tile.Type.floor);
//
//		//++currCoord.x;
//		if (CheckIndex((int)currCoord.x, (int)currCoord.y, false))
//		{
//			curr =
//				CreateTileIndex(tileType, (int)currCoord.x, (int)currCoord.x)
//					.GetComponent<Tile>();
//		}
//		++currCoord.y;
//		if (CheckIndex((int)currCoord.x, (int)currCoord.y, false))
//		{
//			curr =
//				CreateTileIndex(tileType, (int)currCoord.x, (int)currCoord.x)
//					.GetComponent<Tile>();
//		}
//		prevFlip = RNG.Next(0, 2);
//	}
//
//}
//break;
//		case (Tile.DirectionValid.south):
//			{
//	Vector2 endCoord = new Vector2(curr.horzIndex, curr.vertIndex - hallLength);
//	var currCoord = new Vector2(curr.horzIndex, curr.vertIndex);
//	var prevFlip = RNG.Next(0, 2);
//	while (currCoord.y != endCoord.y)
//	{
//		// Coin flip. 
//		// 0 heads == go north
//		// 1 tails == go south
//		if (prevFlip == 0)
//		{
//			++currCoord.x;
//		}
//		else
//		{
//			--currCoord.x;
//		}
//		var tileType = (RNG.Next(0, 10) == 1) ?
//			(Tile.Type.floorDirty) : (Tile.Type.floor);
//
//		//++currCoord.x;
//		if (CheckIndex((int)currCoord.x, (int)currCoord.y, false))
//		{
//			curr =
//				CreateTileIndex(tileType, (int)currCoord.x, (int)currCoord.x)
//					.GetComponent<Tile>();
//		}
//		--currCoord.y;
//		if (CheckIndex((int)currCoord.x, (int)currCoord.y, false))
//		{
//			curr =
//				CreateTileIndex(tileType, (int)currCoord.x, (int)currCoord.x)
//					.GetComponent<Tile>();
//		}
//		prevFlip = RNG.Next(0, 2);
//	}
//
//}
//break;
//	}