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
    
    public Texture2D heightMap;
    public float heightFactor = 10f;

    private List<GameObject> _tiles = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {    
        #region MapGeneration
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
        for (var z = 0; z < heightMapHeight; z++)
        {
            for (var x = 0; x < heightMapWidth; x++)
            {
                _tileMap[z, x]._neighborTiles = FindNeighborsOfTile(_tileMap[z, x]);
                
                //For Logging
                Debug.Log($"Z: {z} X:{x}");
                foreach (var neighbor in _tileMap[z,x]._neighborTiles)
                {
                    Debug.Log(neighbor._type + $"Z:{neighbor._coordinateHeight} X: {neighbor._coordinateWidth}");
                }
            }
        }
        #endregion MapGeneration

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
