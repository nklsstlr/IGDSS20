using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Building : MonoBehaviour
{
    public BuildingTypes type; // The name of the building
    public List<TileTypes> canBeBuiltOnTileTypes; // A restriction on which types of tiles it can be placed on
    
    public float buildCostMoney; // placement money cost
    public float buildCostPlanks; // placement planks cost
    public float upkeep; // The money cost per minute

    #region Manager References
    public JobManager _jobManager; //Reference to the JobManager
    public GameManager _GameManager;
    public NavigationManager _navManager;
    public Store _Store;

    #endregion
    
    #region Workers
    public List<Worker> _workers; //List of all workers associated with this building, either for work or living
    #endregion

    #region Jobs
    public List<Job> _jobs; // List of all available Jobs. Is populated in Start()
    
    #endregion
    
    #region Path

    public int[,] potentialFieldMap;

    #endregion
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    #region Methods   
    public void WorkerAssignedToBuilding(Worker w)
    {
        _workers.Add(w);
    }

    public void WorkerRemovedFromBuilding(Worker w)
    {
        _workers.Remove(w);
    }
    
    #endregion

    public abstract void EconomyForBuilding(Store store, List<Tile> neighborTiles);

    public  float CalcAverageWorkerHappiness()
    {
        if (_workers.Any())
            return _workers.Sum(worker => worker.GetHappiness()) / _workers.Count;
        return 0f;
    }
    
    
    

}