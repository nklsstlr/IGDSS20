using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HousingBuilding : Building
{
    private GameObject _characterPrefab;
    
    public int maximumWorkers= 10;

    // Start is called before the first frame update
    void Start()
    {
        SpawnWorker();
    }

    private void SpawnWorker()
    {
        _characterPrefab = Resources.Load<GameObject>("Prefabs/Character/char_worker");
        var characterGameObj = Instantiate(_characterPrefab, transform);
        var worker = characterGameObj.GetComponent<Worker>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void EconomyForBuilding(Store store, List<Tile> neighborTiles)
    {
        return;//TODO
    }

    public override float CalcEfficiency()
    {
        throw new System.NotImplementedException();
    }
}
