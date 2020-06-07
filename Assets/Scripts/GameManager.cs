using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour
{
    #region Map generation
    private Tile[,] _tileMap; //2D array of all spawned tiles
    #endregion
    
    #region Resources
    private Dictionary<ResourceTypes, float> _resourcesInWarehouse = new Dictionary<ResourceTypes, float>(); //Holds a number of stored resources for every ResourceType
    float ecoTime = 0f;

    private float moneyIncome = 100f;
    //A representation of _resourcesInWarehouse, broken into individual floats. Only for display in inspector, will be removed and replaced with UI later
    [SerializeField]
    private float _ResourcesInWarehouse_Fish;
    [SerializeField]
    private float _ResourcesInWarehouse_Wood;
    [SerializeField]
    private float _ResourcesInWarehouse_Planks;
    [SerializeField]
    private float _ResourcesInWarehouse_Wool;
    [SerializeField]
    private float _ResourcesInWarehouse_Clothes;
    [SerializeField]
    private float _ResourcesInWarehouse_Potato;
    [SerializeField]
    private float _ResourcesInWarehouse_Schnapps;
    #endregion
    
    #region Enumerations
    public enum ResourceTypes { None, Money, Fish, Wood, Planks, Wool, Clothes, Potato, Schnapps }; //Enumeration of all available resource types. Can be addressed from other scripts by calling GameManager.ResourceTypes
    #endregion
    
    #region Buildings
    public GameObject[] _buildingPrefabs; //References to the building prefabs
    public int _selectedBuildingPrefabIndex = 0; //The current index used for choosing a prefab to spawn from the _buildingPrefabs list
    #endregion
    
    public Texture2D heightMap;
    public float heightFactor = 10f;

    private List<GameObject> _tiles = new List<GameObject>();
    
    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {
        PopulateResourceDictionary();
        GenerateMap();
        FindNeighborsOfTile();
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
        HandleKeyboardInput();
        UpdateInspectorNumbersForResources();
        StartEconomy();
        

        UpdateInspectorNumbersForResources();
        
    }
    
    
    
    #endregion

    #region mouseEvents

    //Is called by MouseManager when a tile was clicked
    //Forwards the tile to the method for spawning buildings
    public void TileClicked(int height, int width)
    {
        
        Tile t = _tileMap[height, width];
        Debug.Log("called"+t._type);

        PlaceBuildingOnTile(t);
    }

    #endregion mouseEvents
    #region economy
    private void StartEconomy()
    {
        ecoTime += Time.deltaTime;
        if (ecoTime >= 1f) // every second
        {
            ecoTime %= 1f; // reset
            RunEconmyCycle();
        }
    }

    private void RunEconmyCycle()
    {
        _resourcesInWarehouse[ResourceTypes.Money] += moneyIncome;

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
                if (HasResourceInWarehouse(ResourceTypes.Money, upkeep))
                {
                    _resourcesInWarehouse[ResourceTypes.Money] -= upkeep;
                    EconomyForProduction(tile._building);
                }
            }
        }
    }

    //TODO: Check and Refactor
    private void EconomyForProduction(Building building)
    {
        // calculate efficiency
        if (building.efficiencyScalesWithNeighboringTiles != Tile.TileTypes.Empty)
        {
            int count = building.tile._neighborTiles.Count(x =>
                x._type == building.efficiencyScalesWithNeighboringTiles &&
                x._building == null);
            if (count < building.minimumNeighbors)
            {
                building.efficiency = 0f;
            }
            else if (count >= building.maximumNeighbors)
            {
                building.efficiency = 1f;
            }
            else
            {
                building.efficiency = (float) count / building.maximumNeighbors;
            }
        }

        // maybe infinite
        float productionEvery = building.resourceGenerationInterval / building.efficiency;
        building.resourceGenerationInterval += 1f; // advance one cylce = 1 second

        bool hasInput = building.inputResources.All(x => HasResourceInWarehouse(x));
        bool hasProgress = building.resourceGenerationInterval >= productionEvery;

        if (hasInput && hasProgress)
        {
            building.resourceGenerationInterval = 0f; // reset

            // consume
            foreach (var res in building.inputResources)
                _resourcesInWarehouse[res] -= 1;
            // produce
            _resourcesInWarehouse[building.outputResource] += building.outputCount;
        }
    }

    // Checks if there is at least one material for the queried resource type in the warehouse
    public bool HasResourceInWarehouse(ResourceTypes resource)
    {
        return _resourcesInWarehouse[resource] >= 1;
    }

    // Checks if there is sufficient material for the queried resource type in the warehouse
    public bool HasResourceInWarehouse(ResourceTypes resource, float amount)
    {
        return _resourcesInWarehouse[resource] >= amount;
    }

    #endregion economy

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

                Object.Instantiate(tileToRender.Item1,
                    new Vector3((x * xTranslate) + unevenSupport, pixelColor.maxColorComponent * heightFactor,
                        z * zTranslate), tileToRender.Item1.transform.rotation);
                var tile = gameObject.AddComponent<Tile>();
                tile._type = tileToRender.Item2;
                tile._coordinateHeight = z;
                tile._coordinateWidth = x;
                _tileMap[z, x] = tile;

            }
        }
    }

    
    
    //Makes the resource dictionary usable by populating the values and keys
    void PopulateResourceDictionary()
    {
        foreach (var type in (ResourceTypes[])Enum.GetValues(typeof(ResourceTypes)))
            _resourcesInWarehouse.Add(type, 0);
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
    }

    //Updates the visual representation of the resource dictionary in the inspector. Only for debugging
    void UpdateInspectorNumbersForResources()
    {
        _ResourcesInWarehouse_Fish = _resourcesInWarehouse[ResourceTypes.Fish];
        _ResourcesInWarehouse_Wood = _resourcesInWarehouse[ResourceTypes.Wood];
        _ResourcesInWarehouse_Planks = _resourcesInWarehouse[ResourceTypes.Planks];
        _ResourcesInWarehouse_Wool = _resourcesInWarehouse[ResourceTypes.Wool];
        _ResourcesInWarehouse_Clothes = _resourcesInWarehouse[ResourceTypes.Clothes];
        _ResourcesInWarehouse_Potato = _resourcesInWarehouse[ResourceTypes.Potato];
        _ResourcesInWarehouse_Schnapps = _resourcesInWarehouse[ResourceTypes.Schnapps];
    }
    
    //Checks if there is at least one material for the queried resource type in the warehouse
    public bool HasResourceInWarehoues(ResourceTypes resource)
    {
        return _resourcesInWarehouse[resource] >= 1;
    }

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
                t._building = b;
                b.tile = t;

                // consume build costs
                _resourcesInWarehouse[ResourceTypes.Money] -= prefab.buildCostMoney;
                _resourcesInWarehouse[ResourceTypes.Planks] -= prefab.buildCostPlanks;
            }
        }
    }
    private bool BuildingCanBeBuiltOnTile(Building building, Tile tile)
    {
        return tile._building == null && building.canBeBuiltOnTileTypes.Contains(tile._type) &&
               HasResourceInWarehouse(ResourceTypes.Money, building.buildCostMoney) &&
               HasResourceInWarehouse(ResourceTypes.Planks, building.buildCostPlanks);
    }
    
    private void FindNeighborsOfTile()
    {
        for (var z = 0; z < heightMap.height; z++)
        {
            for (var x = 0; x < heightMap.width; x++)
            {
                _tileMap[z, x]._neighborTiles = FindNeighborsOfTile(_tileMap[z, x]);
                
                //For Logging
                // Debug.Log($"Z: {z} X:{x}");
                // foreach (var neighbor in _tileMap[z,x]._neighborTiles)
                // {
                //     Debug.Log(neighbor._type + $"Z:{neighbor._coordinateHeight} X: {neighbor._coordinateWidth}");
                // }
            }
        }
    }
    //Returns a list of all neighbors of a given tile
    private List<Tile> FindNeighborsOfTile(Tile t,string[] skip = null)
    {
        var neighbors = new List<Tile>();

        if (t._coordinateWidth - 1 >= 0)
                neighbors.Add(_tileMap[t._coordinateHeight, t._coordinateWidth - 1]);//links daneben
            
            if(t._coordinateWidth - 1 >= 0 && t._coordinateHeight -1 >=0)
                neighbors.Add(_tileMap[t._coordinateHeight - 1, t._coordinateWidth - 1]);//drüberLinks

            if (t._coordinateHeight + 1 < heightMap.height)
                neighbors.Add(_tileMap[t._coordinateHeight + 1, t._coordinateWidth ]);//drunterLinks‚
             
            if(t._coordinateWidth +1 < heightMap.width )
                neighbors.Add(_tileMap[t._coordinateHeight, t._coordinateWidth + 1]);//rechtsDaneben
            
            if(t._coordinateHeight -1 >=0)
                neighbors.Add(_tileMap[t._coordinateHeight - 1, t._coordinateWidth]);//drüberRechts
            
            if(t._coordinateWidth + 1 < heightMap.width && t._coordinateHeight +1 <heightMap.height)
                neighbors.Add(_tileMap[t._coordinateHeight + 1, t._coordinateWidth + 1]);//drunterRechts
            
        return neighbors;
    }

     Tuple<GameObject,Tile.TileTypes> GetTile(float colorValue)
    {
        if (colorValue == 0f) return new Tuple<GameObject,Tile.TileTypes>(_tiles.First(x => x.name.Contains("WaterTile")),Tile.TileTypes.Water);
        else if (colorValue > 0f && colorValue <= 0.2f) return new Tuple<GameObject,Tile.TileTypes>(_tiles.First(x => x.name.Contains("SandTile")),Tile.TileTypes.Water);
        else if (colorValue > 0.2f && colorValue <= 0.4f) return new Tuple<GameObject,Tile.TileTypes>(_tiles.First(x => x.name.Contains("GrassTile")),Tile.TileTypes.Grass);
        else if (colorValue > 0.4f && colorValue <= 0.6f) return new Tuple<GameObject,Tile.TileTypes>(_tiles.First(x => x.name.Contains("ForestTile")),Tile.TileTypes.Forest);
        else if (colorValue > 0.6f && colorValue <= 0.8f) return new Tuple<GameObject,Tile.TileTypes>(_tiles.First(x => x.name.Contains("StoneTile")),Tile.TileTypes.Stone);
        else if (colorValue > 0.8f && colorValue <= 1f) return new Tuple<GameObject,Tile.TileTypes>(_tiles.First(x => x.name.Contains("MountainTile")),Tile.TileTypes.Mountain);
        else throw new IndexOutOfRangeException("Color value not in range!");
    }
}
//
