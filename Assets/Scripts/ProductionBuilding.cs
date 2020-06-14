using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProductionBuilding : Building
{
    #region Enumerations
    
    #endregion
    
    #region Attributes
    
    public float upkeep; // The money cost per minute
    public float buildCostMoney; // placement money cost
    public float buildCostPlanks; // placement planks cost
    
    
    public float efficiency = 1f; // Calculated based on the surrounding tile types
    public float resourceGenerationInterval = 0f; // If operating at 100% efficiency, this is the time in seconds it takes for one production cycle to finish
    public float resourceGenerationProgress = 0f; // This is the time spent in production so far
    public float outputCount; // The number of output resources per generation cycle(for example the Sawmill produces 2 planks at a time)
    
    
    public Tile.TileTypes efficiencyScalesWithNeighboringTiles = Tile.TileTypes.Empty; // A choice if its efficiency scales with a specific type of surrounding tile
    public int minimumNeighbors; // The minimum number of surrounding tiles its efficiency scales with(0-6)
    public int maximumNeighbors; // The maximum number of surrounding tiles its efficiency scales with(0-6)
    public List<ResourceTypes> inputResources = new List<ResourceTypes>(); // A choice for input resource types(0, 1 or 2 types)
    public ResourceTypes outputResource; // A choice for output resource type
    #endregion
    
    #region Resources

    #endregion
    
    
    
    #region economy
    
    public void EconomyForBuilding(Store store,List<Tile>neighborTiles)
    {
        
        if (store.HasResourceInWarehouse(ResourceTypes.Money, upkeep))
        {
            store.RemoveResource(ResourceTypes.Money, upkeep);
            
            // calculate efficiency
            if (efficiencyScalesWithNeighboringTiles != Tile.TileTypes.Empty)
            {
            
                int neighborCount = neighborTiles.Count(x =>
                    x._type == efficiencyScalesWithNeighboringTiles &&
                    x._building == null);
                if (neighborCount < minimumNeighbors)
                {
                    efficiency = 0f;
                }
                else if (neighborCount >= maximumNeighbors)
                {
                    efficiency = 1f;
                }
                else
                {
                    efficiency = (float) neighborCount / maximumNeighbors;
                }
            }
            float generationInterval = resourceGenerationInterval / efficiency;
            resourceGenerationProgress += 1f; //add 1 second each cycle

            bool hasInput = inputResources.All(store.HasResourceInWarehouse);
            bool hasProgress = resourceGenerationProgress >= generationInterval;

            if (hasInput && hasProgress)
            {
                resourceGenerationProgress = 0f; // reset

                // input
                foreach (var res in inputResources)
                    store.RemoveResource(res,-1);
                    
                // output
                store.AddResource(outputResource,outputCount);
                
            }
        }
        
    }

    #endregion economy
    
}
