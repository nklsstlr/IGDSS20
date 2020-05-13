using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour
{
    public Texture2D heightMap;
    public float heightFactor = 10f;

    private List<GameObject> _tiles = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {    
        _tiles = Resources.LoadAll<GameObject>("Prefabs").ToList();

        int heightMapWidth = heightMap.width;
        int heightMapHeight = heightMap.height;

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
                GameObject tileToRender = GetTile(pixelColor.maxColorComponent);
                
                Object.Instantiate(tileToRender, new Vector3((x * xTranslate) + unevenSupport, pixelColor.maxColorComponent * heightFactor, z * zTranslate), tileToRender.transform.rotation);
            }
        }
    }

    GameObject GetTile(float colorValue)
    {
        if (colorValue == 0f) return _tiles.First(x => x.name.Contains("WaterTile"));
        else if (colorValue > 0f && colorValue <= 0.2f) return _tiles.First(x => x.name.Contains("SandTile"));
        else if (colorValue > 0.2f && colorValue <= 0.4f) return _tiles.First(x => x.name.Contains("GrassTile"));
        else if (colorValue > 0.4f && colorValue <= 0.6f) return _tiles.First(x => x.name.Contains("ForestTile"));
        else if (colorValue > 0.6f && colorValue <= 0.8f) return _tiles.First(x => x.name.Contains("Stone"));
        else if (colorValue > 0.8f && colorValue <= 1f) return _tiles.First(x => x.name.Contains("MountainTile"));
        else throw new IndexOutOfRangeException("Color value not in range!");
    }
}
