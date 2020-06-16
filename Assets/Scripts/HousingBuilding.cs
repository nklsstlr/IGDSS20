using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HousingBuilding : Building
{
    private GameObject _characterPrefab;

    public int maximumWorkers= 10;
    
    private float _spawnTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
        _workers.Add(SpawnWorker());
        _workers.Add(SpawnWorker());
        
    }

    private Worker SpawnWorker()
    {
        _characterPrefab = Resources.Load<GameObject>("Prefabs/Character/char_worker");
        var characterGameObj = Instantiate(_characterPrefab,GetWorkerOffset(),transform.rotation);
        var worker =  characterGameObj.GetComponent<Worker>();
        worker._jobManager = _jobManager;
        worker._gameManager = _GameManager;
        worker._store = _Store;
        
        return worker;



    }

    private Vector3 GetWorkerOffset()
    {
        var x = transform.position.x + _workers.Count;
        return new Vector3(x,transform.position.y,transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        _spawnTime += Time.deltaTime;
        var timeEffiency = 15 / CalcEfficiency();
        if (!(_spawnTime >= timeEffiency)) return; // every 15 seconds
        {
            _spawnTime %= 1f; // reset
        }
        _workers.Add(SpawnWorker());
    }

    public override void EconomyForBuilding(Store store, List<Tile> neighborTiles)
    {
        return;//TODO
    }

    public override float CalcEfficiency()
    {
       var test =  _workers.Select(x => x.GetHappiness()).Average();
       
       if (_workers.Any())
           return _workers.Sum(worker => worker.GetHappiness()) / _workers.Count;
       return 0f;
    }
}
