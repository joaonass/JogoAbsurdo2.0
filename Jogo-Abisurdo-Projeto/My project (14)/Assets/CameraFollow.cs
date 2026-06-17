using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset;

    void LateUpdate()
    {
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            transform.position.y,
            offset.z
        );

        desiredPosition.z = -10f;

        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.position = smoothedPosition;
    }

    void Start()
    {
        
    }

}
