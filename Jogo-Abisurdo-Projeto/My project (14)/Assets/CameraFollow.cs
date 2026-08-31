using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset;

    public bool seguirVertical = false;

    void LateUpdate()
    {
        float posicaoY;

        if (seguirVertical)
        {
            posicaoY = target.position.y + offset.y;
        }
        else
        {
            posicaoY = transform.position.y;
        }

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            posicaoY,
            -10f
        );

        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.position = smoothedPosition;
    }
}