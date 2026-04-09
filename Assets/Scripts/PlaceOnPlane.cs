using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class PlaceOnPlane : MonoBehaviour
{
    public GameObject objectToPlace;
    public Camera arCamera;

    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool objectPlaced = false;

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();

        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }

    void Update()
    {
        if (objectPlaced) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Quaternion laneRotation = Quaternion.LookRotation(arCamera.transform.forward, Vector3.up);
                laneRotation = Quaternion.Euler(0, laneRotation.eulerAngles.y, 0);

                Instantiate(objectToPlace, pose.position, laneRotation);
                objectPlaced = true;
            }
        }
    }
}