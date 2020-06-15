using System;
using System.Collections.Generic;

using UnityEngine;

public class Store : MonoBehaviour
{
    
    
    //A representation of _resourcesInWarehouse, broken into individual floats. Only for display in inspector, will be removed and replaced with UI later
    [SerializeField]
    private float _ResourcesInWarehouse_Money;
    [SerializeField]
    private float _ResourcesInWarehouse_Fish;
    [SerializeField]
    private float _ResourcesInWarehouse_Wood;
    [SerializeField]
    private float _ResourcesInWarehouse_Planks;
    [SerializeField]
    private float _ResourcesInWarehouse_Wool;
    [SerializeField]
    private float _ResourcesInWarehouse_Clothes;
    [SerializeField]
    private float _ResourcesInWarehouse_Potato;
    [SerializeField]
    private float _ResourcesInWarehouse_Schnapps;

    private Dictionary<ResourceTypes, float> _resourcesInWarehouse = new Dictionary<ResourceTypes, float>(); //Holds a number of stored resources for every ResourceType
    
    
    public void AddResource(ResourceTypes resource, float amount)
    {
        _resourcesInWarehouse[resource] += amount;
    }

    
    public void RemoveResource(ResourceTypes resource, float amount)
    {
        _resourcesInWarehouse[resource] -= amount;
    }
    
    // Checks if there is at least one material for the queried resource type in the warehouse
    public bool HasResourceInWarehouse(ResourceTypes resource)
    {
        return _resourcesInWarehouse[resource] >= 1;
    }

    // Checks if there is sufficient material for the queried resource type in the warehouse
    public bool HasResourceInWarehouse(ResourceTypes resource, float amount)
    {
        return _resourcesInWarehouse[resource] >= amount;
    }
    
    //Updates the visual representation of the resource dictionary in the inspector. Only for debugging
    public void UpdateInspectorNumbersForResources()
    {
        _ResourcesInWarehouse_Money = _resourcesInWarehouse[ResourceTypes.Money];
        _ResourcesInWarehouse_Fish = _resourcesInWarehouse[ResourceTypes.Fish];
        _ResourcesInWarehouse_Wood = _resourcesInWarehouse[ResourceTypes.Wood];
        _ResourcesInWarehouse_Planks = _resourcesInWarehouse[ResourceTypes.Planks];
        _ResourcesInWarehouse_Wool = _resourcesInWarehouse[ResourceTypes.Wool];
        _ResourcesInWarehouse_Clothes = _resourcesInWarehouse[ResourceTypes.Clothes];
        _ResourcesInWarehouse_Potato = _resourcesInWarehouse[ResourceTypes.Potato];
        _ResourcesInWarehouse_Schnapps = _resourcesInWarehouse[ResourceTypes.Schnapps];
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var type in (ResourceTypes[])Enum.GetValues(typeof(ResourceTypes)))
            _resourcesInWarehouse.Add(type, 0);
        Debug.Log("called start method");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInspectorNumbersForResources();
    }
}
