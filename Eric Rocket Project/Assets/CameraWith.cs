using UnityEngine;
using System.Collections.Generic;

public class CameraWith : MonoBehaviour
{
    public List<Transform> targets;
    public Transform currentTarget;
    private int targetIndex = 0;
    public Vector3 offset;
    public float zoomSpeed = 4f;
    public float currentZoom = 10f;
    public float minZoom = 5f;
    public float maxZoom = 15f;
    public float xSpeed = 100f;
    public float ySpeed = 100f;

    private float x;
    private float y;
    private Vector3 lastPosition;

    void NextTarget() {
        if (targetIndex < targets.Count - 1) {
            targetIndex++;
        } else {
            targetIndex = 0;
        }
        currentTarget = targets[targetIndex];
    }

    void PreviousTarget() {
        if (targetIndex > 0) {
            targetIndex--;
        } else {
            targetIndex = targets.Count - 1;
        }
        currentTarget = targets[targetIndex];
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTarget = targets[targetIndex];

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTarget == null) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)){
            PreviousTarget();
        } else if (Input.GetKeyDown(KeyCode.RightArrow)){
            NextTarget();
        }

        if (Input.GetMouseButton(0)) {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }

        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0, 0, -currentZoom) + currentTarget.position + offset;

        transform.rotation = rotation;
        transform.position = position;
    }
}
