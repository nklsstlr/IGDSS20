﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour
{
    #region Map generation
    private Tile[,] _tileMap; //2D array of all spawned tiles
    public Texture2D heightMap;
    public float heightFactor = 10f;

    private List<GameObject> _tiles = new List<GameObject>();
    #endregion
    
    
    #region Buildings
    public GameObject[] _buildingPrefabs; //References to the building prefabs
    public int _selectedBuildingPrefabIndex = 0; //The current index used for choosing a prefab to spawn from the _buildingPrefabs list
    public  Store _store;
    private float ecoTime= 0f;
    private float moneyIncome = 100f;
    public JobManager _jobManager; //Reference to the JobManager
    public NavigationManager _navigationManager;

    #endregion
    
    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
        FindNeighborsOfTile();
    }
    
    // Update is called once per frame
    void Update()
    {
        HandleKeyboardInput();
        
        
        StartEconomy();
    }

    #endregion
    
    private void StartEconomy()
    {
        ecoTime += Time.deltaTime;
        if (ecoTime >= 1f) // TODO: every second (should be 60 seconds, but we sped it up for testing purposes)
        {
            ecoTime %= 1f; // reset
            RunEconmyCycle();
        }
    }

    private void RunEconmyCycle()
    {
        _store.AddResource(ResourceTypes.Money,moneyIncome);
        
        //TODO: Ulala ob das funkt:
        // Check all tiles for buildings
        float workersIncome = 0f;
        foreach (var tile in _tileMap)
        {
            if (tile._building)
            {   
                foreach(Worker worker in tile._building._workers) {
                    workersIncome = worker.isAdultAge(worker._age) ? workersIncome += 20 : workersIncome += 10;
                }
            }
        }
        _store.AddResource(ResourceTypes.Money,workersIncome);
        
        EconomyForBuildings();
    }

    private void EconomyForBuildings()
    {
        // Check all tiles for buildings
        foreach (var tile in _tileMap)
        {
            if (tile._building)
            {
                float upkeep = tile._building.upkeep;
               
                if (_store.HasResourceInWarehouse(ResourceTypes.Money,upkeep))
                {
                    _store.RemoveResource(ResourceTypes.Money,upkeep);
                    
                    tile._building.EconomyForBuilding(_store,tile._neighborTiles);
                    
                }
            }
        }
    }
    
    #region Input

    //Is called by MouseManager when a tile was clicked
    //Forwards the tile to the method for spawning buildings
    public void TileClicked(int height, int width)
    {
        
        Tile t = _tileMap[height, width];
        Debug.Log("called"+t._type);

        PlaceBuildingOnTile(t);
    }
    
    //Sets the index for the currently selected building prefab by checking key presses on the numbers 1 to 0
    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _selectedBuildingPrefabIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _selectedBuildingPrefabIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _selectedBuildingPrefabIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _selectedBuildingPrefabIndex = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _selectedBuildingPrefabIndex = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            _selectedBuildingPrefabIndex = 5;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            _selectedBuildingPrefabIndex = 6;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            _selectedBuildingPrefabIndex = 7;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            _selectedBuildingPrefabIndex = 8;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _selectedBuildingPrefabIndex = 9;
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            //CHEAT
            _store.AddResource(ResourceTypes.Money,1000);
            _store.AddResource(ResourceTypes.Planks,1000);
            _store.AddResource(ResourceTypes.Potato,1000);
            _store.AddResource(ResourceTypes.Schnapps,1000);
            _store.AddResource(ResourceTypes.Wood,1000);
            _store.AddResource(ResourceTypes.Wool,1000);
            _store.AddResource(ResourceTypes.Clothes,1000);
            _store.AddResource(ResourceTypes.Fish,1000);
        }
    }

    #endregion input
    
    
    
    
    # region generateMap
    
    private void GenerateMap()
    {
        _tiles = Resources.LoadAll<GameObject>("Prefabs").ToList();

        int heightMapWidth = heightMap.width;
        int heightMapHeight = heightMap.height;
        _tileMap = new Tile[heightMapHeight,heightMapWidth];

        float xTranslate = _tiles[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.z;
        float zTranslate = _tiles[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.x * (0.75f);

        
        // loop through each pixel beginning top left
        for (var z = 0; z < heightMapHeight; z++)
        {
            for (var x = 0; x < heightMapWidth; x++)
            {
                float unevenSupport = 0f;

                if (z % 2 != 0) // If z is uneven, shift the row to right by half of tile width
                {
                    unevenSupport = _tiles[0].GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 2f;
                }

                Color pixelColor = heightMap.GetPixel(x, z);
                var tileToRender = GetTile(pixelColor.maxColorComponent);

                var newObject = Object.Instantiate(tileToRender.Item1,
                    new Vector3((x * xTranslate) + unevenSupport, pixelColor.maxColorComponent * heightFactor,
                        z * zTranslate), tileToRender.Item1.transform.rotation);
                
                var tile = newObject.GetComponent<Tile>();
                tile._type = tileToRender.Item2;
                tile._coordinateHeight = z;
                tile._coordinateWidth = x;
                _tileMap[z, x] = tile;
                Debug.Log("height z: "+tile._coordinateHeight+ "width x: "+tile._coordinateWidth + tile._type);

            }
        }
    }
    
    private void FindNeighborsOfTile()
    {
        for (var z = 0; z < heightMap.height; z++)
        {
            for (var x = 0; x < heightMap.width; x++)
            {
                _tileMap[z, x]._neighborTiles = FindNeighborsOfTile(_tileMap[z, x]);
                
            }
        }
    }
    //Returns a list of all neighbors of a given tile
    private List<Tile> FindNeighborsOfTile(Tile t)
    {
        List<Tile> result = new List<Tile>();

        int height = t._coordinateHeight;
        int width = t._coordinateWidth;

        //top, top-left, left, right, bottom, bottom-left
        //Check edge cases
        //top
        if (height > 0)
        {
            result.Add(_tileMap[height - 1, width]);
        }

        //bottom
        if (height < _tileMap.GetLength(0) - 1)
        {
            result.Add(_tileMap[height + 1, width]);
        }

        //left
        if (width > 0)
        {
            result.Add(_tileMap[height, width - 1]);
        }

        //right
        if (width < _tileMap.GetLength(1) - 1)
        {
            result.Add(_tileMap[height, width + 1]);
        }

        //if the column is even
        //top-left + bottom-left
        if (height % 2 == 0)
        {
            if (height > 0 && width > 0)
            {
                result.Add(_tileMap[height - 1, width - 1]);
            }

            if (height < _tileMap.GetLength(0) - 1 && width > 0)
            {
                result.Add(_tileMap[height + 1, width - 1]);
            }
        }
        //if the column is uneven
        //top-right + bottom-right
        else
        {
            if (height > 0 && width < _tileMap.GetLength(1) - 1)
            {
                result.Add(_tileMap[height - 1, width + 1]);
            }

            if (height < _tileMap.GetLength(0) - 1 && width < _tileMap.GetLength(1) - 1)
            {
                result.Add(_tileMap[height + 1, width + 1]);
            }
        }

        return result;
    }

     Tuple<GameObject,TileTypes> GetTile(float colorValue)
    {
        if (colorValue == 0f) return new Tuple<GameObject,TileTypes>(_tiles.First(x => x.name.Contains("WaterTile")),TileTypes.Water);
        else if (colorValue > 0f && colorValue <= 0.2f) return new Tuple<GameObject,TileTypes>(_tiles.First(x => x.name.Contains("SandTile")),TileTypes.Sand);
        else if (colorValue > 0.2f && colorValue <= 0.4f) return new Tuple<GameObject,TileTypes>(_tiles.First(x => x.name.Contains("GrassTile")),TileTypes.Grass);
        else if (colorValue > 0.4f && colorValue <= 0.6f) return new Tuple<GameObject,TileTypes>(_tiles.First(x => x.name.Contains("ForestTile")),TileTypes.Forest);
        else if (colorValue > 0.6f && colorValue <= 0.8f) return new Tuple<GameObject,TileTypes>(_tiles.First(x => x.name.Contains("StoneTile")),TileTypes.Stone);
        else if (colorValue > 0.8f && colorValue <= 1f) return new Tuple<GameObject,TileTypes>(_tiles.First(x => x.name.Contains("MountainTile")),TileTypes.Mountain);
        else throw new IndexOutOfRangeException("Color value not in range!");
    }
    #endregion

    #region gamePlay

    //Checks if the currently selected building type can be placed on the given tile and then instantiates an instance of the prefab
    private void PlaceBuildingOnTile(Tile t)
    {
        Debug.Log("index"+_selectedBuildingPrefabIndex);
        Debug.Log("length"+_buildingPrefabs.Length);
        //if there is building prefab for the number input
        if (_selectedBuildingPrefabIndex < _buildingPrefabs.Length)
        {
            Building prefab = _buildingPrefabs[_selectedBuildingPrefabIndex].GetComponent<Building>();
            if (BuildingCanBeBuiltOnTile(prefab, t))
            {
                // Insantiate with parent
                GameObject newBuildingObject = Instantiate(_buildingPrefabs[_selectedBuildingPrefabIndex], t.gameObject.transform);
                
                Building b = newBuildingObject.GetComponent<Building>();
                t._building = b;//TODO überprüfen ob referenz gesetzt wird
                b._jobManager = _jobManager;
                b._GameManager = this;
                b._Store = _store;
                b._navManager = _navigationManager;
                b.potentialFieldMap = _navigationManager.GeneratePotentialFieldMap(t,_tileMap.GetLength(0), _tileMap.GetLength(1));
                

                _store.RemoveResource(ResourceTypes.Money,prefab.buildCostMoney);
                _store.RemoveResource(ResourceTypes.Planks,prefab.buildCostPlanks);
                
            }
        }
    }
    private bool BuildingCanBeBuiltOnTile(Building building, Tile tile)
    {
        return tile._building == null && building.canBeBuiltOnTileTypes.Contains(tile._type) &&
               _store.HasResourceInWarehouse(ResourceTypes.Money, building.buildCostMoney) &&
               _store.HasResourceInWarehouse(ResourceTypes.Planks, building.buildCostPlanks);
    }
    #endregion gamePlay
    
}

