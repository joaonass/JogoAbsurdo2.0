using UnityEngine;

public class CameraVerticalZone : MonoBehaviour
{
    private CameraFollow cameraFollow;

    void Start()
    {
        Camera cameraPrincipal = Camera.main;

        if (cameraPrincipal != null)
            cameraFollow = cameraPrincipal.GetComponent<CameraFollow>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (cameraFollow != null)
            cameraFollow.seguirVertical = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (cameraFollow != null)
            cameraFollow.seguirVertical = false;
    }
}