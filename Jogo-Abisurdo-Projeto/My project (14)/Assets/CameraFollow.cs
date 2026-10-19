
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset;

    public bool seguirVertical = false;
    public float limiteY = -100f;

    private float alturaInicialCamera;

    void Start()
    {
        alturaInicialCamera = transform.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float posicaoY;

        if (seguirVertical)
        {
            posicaoY = target.position.y + offset.y;
        }
        else
        {
            // Mantém a altura inicial, mas permite acompanhar
            // o player se ele descer abaixo dela.
            posicaoY = Mathf.Min(
                alturaInicialCamera,
                target.position.y + offset.y
            );
        }

        posicaoY = Mathf.Max(posicaoY, limiteY);

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            posicaoY,
            -10f
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}