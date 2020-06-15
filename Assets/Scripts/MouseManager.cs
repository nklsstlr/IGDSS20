﻿using DefaultNamespace;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    private Vector3 _dragOrigin;
    public Camera mainCamera;
    public float speed = 1.5f;
    public BoxCollider boxCollider;    // boundaries can be changed by scaling the BoxCollider cube in the scene
    public GameManager gameManager;

    public float minFov = 15f;
    public float maxFov = 90f;
    public float scrollSensitivity = 10f;

    private Perspective _lastPers = Perspective.Schräg;

    

    private void Start()
    {
        // Set initial position and rotation of camera
        mainCamera.transform.position = new Vector3(75, 30, 0);
        mainCamera.transform.rotation = Quaternion.Euler(36.0f, 0.0f, 0.0f);
    }

    private void LateUpdate()
    {
        ChangeFov();

        if (Input.GetMouseButtonDown(0)) GetTile();
        if (Input.GetMouseButton(1)) MoveCameraInBoundary();
        if (Input.GetMouseButtonDown(2)) ChangePerspective();    // Additional support of middle mouse button to switch camera perspective
    }
    
    private Perspective _actualPerspective
    {
        get
        {
            //Qauternion comparison is to complicated --> bad lastPers property...
            if (_lastPers==Perspective.Schräg) //if(mainCamera.transform.rotation.x ==90)....
                _lastPers = Perspective.Draufsicht;
            else
                _lastPers = Perspective.Schräg;
            return _lastPers;
        }
    }

    private void ChangePerspective()
    {
        var actualPosition = mainCamera.transform.position;
        var newPosition = new Vector3();
        var newQuaternion = new Quaternion();
        switch (_actualPerspective)
        {
            case Perspective.Schräg:
            {
                newPosition = new Vector3(actualPosition.x, 50, actualPosition.z);
                newQuaternion = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                break;
            }
            case Perspective.Draufsicht:
            {
                newPosition = new Vector3(actualPosition.x, 20, actualPosition.z);
                newQuaternion = Quaternion.Euler(36.0f, 0.0f, 0.0f);
                break;
            }
        }
        mainCamera.transform.position = newPosition;
        mainCamera.transform.rotation = newQuaternion;
    }

    private void GetTile()
    {
        // This would cast rays only against collider in layer 8.
        int layerMask = 1 << 8;
        var cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(cameraRay, out hit, Mathf.Infinity, layerMask))
        {
            Debug.Log($"Clicked on: {hit.collider.name}");
            
            Tile t = hit.collider.gameObject.GetComponent<Tile>() as Tile;
            Debug.Log("Tile type: " + t._type +"height: "+t._coordinateHeight+"width: "+t._coordinateWidth);
            gameManager.TileClicked(t._coordinateHeight,t._coordinateWidth);
        }
    }

    private void ChangeFov()
    {
        float fov = mainCamera.fieldOfView;
        fov += Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        fov = Mathf.Clamp(fov, minFov, maxFov);
        mainCamera.fieldOfView = fov;
    }

    private void MoveCameraInBoundary()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _dragOrigin = Input.mousePosition;
            return;
        }

        Vector3 pos = mainCamera.ScreenToViewportPoint(Input.mousePosition - _dragOrigin);
        Vector3 move = new Vector3(
            -pos.x * speed,
            0,
            -pos.y * speed
        );

        mainCamera.transform.Translate(move, Space.World);

        Vector3 newPos = mainCamera.transform.position;
        Bounds areaBounds = boxCollider.bounds;

        mainCamera.transform.position = new Vector3(
            Mathf.Clamp(newPos.x, areaBounds.min.x, areaBounds.max.x),
            newPos.y,
            Mathf.Clamp(newPos.z, areaBounds.min.z, areaBounds.max.z)
        );
    }
}