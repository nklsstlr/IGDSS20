using UnityEngine.Rendering;

public class Job
{
    private Worker _worker; //The worker occupying this job
    private Building _building; //The building offering the job

    //Constructor. Call new Job(this) from the Building script to instanciate a job
    public Job(Building building)
    {
        _building = building;
    }

    public void AssignWorker(Worker w)
    {
        _worker = w;
        _building.WorkerAssignedToBuilding(w);
    }

    public void RemoveWorker(Worker w)
    {
        _worker = null;
        _building.WorkerRemovedFromBuilding(w);
    }

    public Worker getWorker()
    {
        return _worker;
    }

    public Building getBuildingOfjob()
    {
        return _building;
    }
        
}