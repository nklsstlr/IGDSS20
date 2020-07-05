using System;
using UnityEngine;
using UnityEngine.Experimental.Audio;

public class Worker : MonoBehaviour
{
    #region Manager References
    public JobManager _jobManager; //Reference to the JobManager
    public GameManager _gameManager;//Reference to the GameManager
    public NavigationManager _navManager;
    public Store _store;//Reference to the Store
    #endregion

    public float _age; // The age of this worker
    private float _consumeHappiness = 0f;
    private float _ageTime = 0f;
    private Tile _nextTileToMove;
    public Tile _currentTile;
    public Building _home;
    private Building _destination;
    private bool shouldWait = false;
    private float stayingTick = 0f;
    
    private float movingTick = 0f;
    private float movingProgess = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Age();
        WalkToBuilding();
    }

    private void WalkToBuilding()
    {
        if (!_jobManager.DoIHaveAJob(this)) return;

        if (_destination is null)
        {
            _destination = _jobManager.GetJob(this).getBuildingOfjob();
        }

        if (_nextTileToMove is null)
        {
            _nextTileToMove = _navManager.CalculateNextTile(_destination, _currentTile);
        }

        if (shouldWait)
        {
            stayingTick += Time.deltaTime;
            if (!(stayingTick >= 5f)) return;
            stayingTick %= 5f;
            shouldWait = false;
        }
        movingTick += Time.deltaTime;
        if (!(movingTick >= 0.1f)) return;
        movingTick %= 0.1f;
        if (Math.Abs(movingProgess) < 0.0001)
        {
            _nextTileToMove = _navManager.CalculateNextTile(_destination, _currentTile);
        }

        movingProgess += 0.1f;


        transform.position = Vector3.Lerp(_currentTile.transform.position, _nextTileToMove.transform.position, movingProgess);
        transform.LookAt(_destination.transform);
        if (Math.Abs(movingProgess - 1f) < 0.0001)
        {
            movingProgess = 0.0f;
            _currentTile = _nextTileToMove;
            CheckIfBuildingSwitch();
        }
        
    }

    private void CheckIfBuildingSwitch()
    {
        if (_destination == _currentTile._building)
        {
            if (_destination == _home)
            {
                _destination = _jobManager.GetJob(this).getBuildingOfjob();
            }
            else
            {
                _destination = _home;
            }
            shouldWait = true;
        }
    }
    


    private void Age()
    {
        //When becoming of age, the worker enters the job market, and leaves it when retiring.
        //Eventually, the worker dies and leaves an empty space in his home. His Job occupation is also freed up.

        _ageTime += Time.deltaTime;
        if (!(_ageTime >= 15f)) return; // every 15 seconds
        
        _ageTime %= 1f; // reset
        _age++;
        _consumeHappiness = 0;
        Consume();
        
        if (_age > 14 && _age <= 64)
        {
            BecomeOfAge();
        }

        if (_age > 64)
        {
            Retire();
        }

        if (_age > 100)
        {
            Die();
        }
    }
    

    public float GetHappiness()
    {
        var gaMa = _gameManager;
        var s = gaMa._store;
        var happy = 0f;
        
        if (_jobManager.DoIHaveAJob(this) && isAdultAge(_age)) {
            happy += 0.5f;
        } else if (isAdultAge(_age) && !_jobManager.DoIHaveAJob(this)) {
            happy += 0.2f;
        }
       
        if (!isAdultAge(_age)) {
            happy += 0.3f;
        }

        happy += _consumeHappiness;
        
        return happy;
    }

    public bool isAdultAge(float age)
    {
        return age >= 14 && age <=64 ? true : false;
    }

    private void Consume()
    {
        if (_store.HasResourceInWarehouse(ResourceTypes.Schnapps, 0.1f)) {
            _store.RemoveResource(ResourceTypes.Schnapps, 0.1f);
            _consumeHappiness += 0.166f;
        }

        if (_store.HasResourceInWarehouse(ResourceTypes.Fish, 0.1f)) {
            _store.RemoveResource(ResourceTypes.Fish, 0.1f);
            _consumeHappiness += 0.166f;
        }

        if (_store.HasResourceInWarehouse(ResourceTypes.Clothes, 0.1f)) {
            _store.RemoveResource(ResourceTypes.Clothes, 0.1f);
            _consumeHappiness += 0.166f;
        }
    }


    public void BecomeOfAge()
    {
        if (!_jobManager.AmIAlreadyRegistered(this) && !_jobManager.DoIHaveAJob(this))
        { 
            _jobManager.RegisterWorker(this);
        }
    }

    private void Retire()
    {
        _jobManager.RemoveWorker(this);
    }

    private void Die()
    {
        Destroy(this.gameObject, 1f);
    }
}