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
        if (_workers.Count < 5) {
            return new Vector3(x - 2, transform.position.y, transform.position.z - 2);
        } else {
            return new Vector3(x - 7, transform.position.y, transform.position.z - 4);
        }

    }

    // Update is called once per frame
    void Update()
    {
        _spawnTime += Time.deltaTime;
        var timeEffiency = 30 / CalcAverageHappiness();
        if (!(_spawnTime >= timeEffiency)) return; // every 30 seconds
        {
            _spawnTime %= 1f; // reset
        }

        if (maximumWorkers > _workers.Count) {
            _workers.Add(SpawnWorker());
        }
    }

    public override void EconomyForBuilding(Store store, List<Tile> neighborTiles)
    {
        return;
    }

    
}
