using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    private float startPosX;
    private float spriteLength;
    public Camera cam;
    public float parallaxEffect; // 0 = parado, 1 = acompanha a câmera

    void Start()
    {
        startPosX = transform.position.x;
        spriteLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float distanceMoved = cam.transform.position.x * parallaxEffect;
        transform.position = new Vector3(startPosX + distanceMoved, transform.position.y, transform.position.z);

        // Repetição infinita do fundo
        float temp = cam.transform.position.x * (1 - parallaxEffect);
        if (temp > startPosX + spriteLength) startPosX += spriteLength;
        else if (temp < startPosX - spriteLength) startPosX -= spriteLength;
    }
}
