using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public Tile CalculateNextTile(Building destinationBuilding, Tile currentTile)
    {
        var tile = currentTile._neighborTiles.First();
        foreach (var neighbor in currentTile._neighborTiles)
        {
            if (destinationBuilding.potentialFieldMap[neighbor._coordinateHeight, neighbor._coordinateWidth] <
                destinationBuilding.potentialFieldMap[tile._coordinateHeight, tile._coordinateWidth])
            {
                tile = neighbor;
            }
        }

        return tile;
    }

    public int[,] GeneratePotentialFieldMap(Tile tileWithBuilding, int height, int width)
    {
        var building = tileWithBuilding._building;
        var fieldMap = new int[height, width];

        
        GetPotentialFieldsForTile(tileWithBuilding, fieldMap);
        //Set own weight to 0
        fieldMap[tileWithBuilding._coordinateHeight, tileWithBuilding._coordinateWidth] = 0;
        return fieldMap;
    }

    private int[,] GetPotentialFieldsForTile(Tile t, int[,] fieldMap,List<Tile> alreadyVisitedTiles=null)
    {
        //BoundaryTiles gets ignored
        if (t._neighborTiles.Count < 6)
        {
            return fieldMap;
        }

        if (alreadyVisitedTiles is null)
        {
            alreadyVisitedTiles = new List<Tile>();
        }
        alreadyVisitedTiles.Add(t);

        foreach (var neighbor in t._neighborTiles)
        {
            if (fieldMap[neighbor._coordinateHeight, neighbor._coordinateWidth] == 0)
            {
                fieldMap[neighbor._coordinateHeight, neighbor._coordinateWidth] =
                    neighbor._weightOfTile + fieldMap[t._coordinateHeight, t._coordinateWidth];
            }

        }

        //Can be moved to upper foreach, but here for better understanding of recursive call
        foreach (var neighbor in t._neighborTiles)
        {
            if (!alreadyVisitedTiles.Contains(neighbor))
            {
                GetPotentialFieldsForTile(neighbor, fieldMap,alreadyVisitedTiles);
            }
        }
        
        return fieldMap;
    }
    
}
