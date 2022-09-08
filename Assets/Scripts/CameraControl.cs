using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform target;
    // [SerializeField] private float distanceToTarget = 13.4f;

    private float distanceToTarget;

    private Vector3 previousPosition = Vector3.zero;
    // Start is called before the first frame update

    private void MoveCamera()
    {
        // Will be true as long as the mouse is down or a touch is happening.
        Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        Vector3 direction = previousPosition - newPosition;

        float rotationAroundYAxis = -direction.x * 180; // camera moves horizontally
        float rotationAroundXAxis = direction.y * 180; // camera moves vertically

        cam.transform.position = target.position;

        cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
        cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World); // <— This is what makes it work!

        cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));

        previousPosition = newPosition;
    }

    void Start()
    {
        distanceToTarget = Vector3.Distance(cam.transform.position, target.position);
        cam.transform.position = target.position;
        cam.transform.Rotate(new Vector3(1, 0, 0), 0f);
        cam.transform.Rotate(new Vector3(0, 1, 0), 0f, Space.World);
        cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            Vector3 direction = (target.position - cam.transform.position).normalized;
            cam.transform.position = cam.transform.position + direction;
            distanceToTarget = Vector3.Distance(cam.transform.position, target.position);

            //wheel goes up
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            Vector3 direction = (target.position - cam.transform.position).normalized;
            cam.transform.position = cam.transform.position - direction;
            distanceToTarget = Vector3.Distance(cam.transform.position, target.position);

            //wheel goes down
        }

        if (Input.GetMouseButtonDown(1))
        {
            // Will be true only in the 1st frame in which it detects the mouse is down (or a tap is happening)
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(1))
        {
            // Will be true as long as the mouse is down or a touch is happening.
            MoveCamera();
        }
    }
}
