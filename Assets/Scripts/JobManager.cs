using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{

    private readonly List<Job> _availableJobs = new List<Job>();
    private readonly List<Job> _occupiedJobs = new List<Job>();
    private readonly List<Worker> _unoccupiedWorkers = new List<Worker>();


    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion


    #region Methods

    private void HandleUnoccupiedWorkers()
    {
        if (_unoccupiedWorkers.Count > 0 && _availableJobs.Count > 0)
        {
            Debug.Log("Unoccupied Workes & Jobs" + _unoccupiedWorkers.Count + ", "+ _availableJobs.Count);
            int randomJobIndex = Random.Range(0, _availableJobs.Count);
            Worker workerToGiveJob = _unoccupiedWorkers[0];
            
            _availableJobs[randomJobIndex].AssignWorker(workerToGiveJob);
            _unoccupiedWorkers.Remove(workerToGiveJob);
            _occupiedJobs.Add(_availableJobs[randomJobIndex]);
            _availableJobs.RemoveAt(randomJobIndex);
        }
    }

    public void RegisterWorker(Worker w)
    {
        _unoccupiedWorkers.Add(w);
        HandleUnoccupiedWorkers();
    }

    public void AddAvailableJob(Job job)
    {
        _availableJobs.Add(job);
    }
    

    public void RemoveWorker(Worker w)
    {
        _unoccupiedWorkers.Remove(w);
    }
    
    public bool AmIAlreadyRegistered(Worker worker2find)
    {
        return _unoccupiedWorkers.Contains(worker2find);
    }

    public bool DoIHaveAJob(Worker worker2find)
    {
        return _occupiedJobs.Exists(job => job.getWorker() == worker2find);
    }

    #endregion
    
}