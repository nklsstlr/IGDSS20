using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProductionBuilding : Building
{
    #region Enumerations
    
    #endregion
    
    #region Attributes
    
    
    public float resourceGenerationInterval = 0f; // If operating at 100% efficiency, this is the time in seconds it takes for one production cycle to finish
    public float resourceGenerationProgress = 0f; // This is the time spent in production so far
    public float outputCount; // The number of output resources per generation cycle(for example the Sawmill produces 2 planks at a time)
    
    public Tile.TileTypes efficiencyScalesWithNeighboringTiles = Tile.TileTypes.Empty; // A choice if its efficiency scales with a specific type of surrounding tile
    public int minimumNeighbors; // The minimum number of surrounding tiles its efficiency scales with(0-6)
    public int maximumNeighbors; // The maximum number of surrounding tiles its efficiency scales with(0-6)
    public List<ResourceTypes> inputResources = new List<ResourceTypes>(); // A choice for input resource types(0, 1 or 2 types)
    public ResourceTypes outputResource; // A choice for output resource type

    public int maxJobs; // The number of maximum jobs that the production building can offer
    
    #endregion
    
    #region Resources

    #endregion
    
    #region economy

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < maxJobs; i++)
        {
            _jobManager.AddAvailableJob(new Job(this));
        }
    }
    
    public override void EconomyForBuilding(Store store,List<Tile>neighborTiles)
    {
        if (store.HasResourceInWarehouse(ResourceTypes.Money, upkeep))
        {
            store.RemoveResource(ResourceTypes.Money, upkeep);

            float generationInterval = resourceGenerationInterval / CalcEfficiency();
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

    public override float CalcEfficiency()
    {
        var tile = gameObject.GetComponentInParent(typeof(Tile)) as Tile;//TODO fancy zeile
        
        // calculate efficiency
        if (efficiencyScalesWithNeighboringTiles != Tile.TileTypes.Empty)
        {
            
            int neighborCount = tile._neighborTiles.Count(x =>
                x._type == efficiencyScalesWithNeighboringTiles &&
                x._building == null);
            if (neighborCount < minimumNeighbors)
            {
                return 0f;
            }
            else if (neighborCount >= maximumNeighbors)
            {
                return 1f;
            }
            else
            {
                return (float) neighborCount / maximumNeighbors;
            }
        }

        return 1f;
    }

    #endregion economy
    
}
