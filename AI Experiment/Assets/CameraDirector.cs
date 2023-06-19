using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDirector : MonoBehaviour
{
    private Transform target;

    public float funnyAngleChance = 0.1f;
    public Camera[] cameras;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private float startingFOV;

    public void Start()
    {
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        startingFOV = cameras[0].fieldOfView;
    }

    public void Reset()
    {
        target = null;
        cameras[0].fieldOfView = startingFOV;
    }

    void Update()
    {
        if (target != null)
            transform.position = target.position;
        else
        {
            transform.position = startingPosition;
            transform.rotation = startingRotation;
        }
    }

    public void CameraDice()
    {
        // funny angle % of the time (e.g. 10%) the camera will inherit the angle, position, and FOV of a random camera
        if (Random.value < funnyAngleChance)
        {
            int randomCameraIndex = Random.Range(0, cameras.Length);
            target = cameras[randomCameraIndex].transform;
            cameras[0].fieldOfView = cameras[randomCameraIndex].fieldOfView;
        }
        else
        {
            Reset();
        }

    }
}
