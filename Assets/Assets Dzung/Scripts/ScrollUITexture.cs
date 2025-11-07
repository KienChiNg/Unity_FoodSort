using UnityEngine;
using UnityEngine.UI;

public class ScrollUITexture : MonoBehaviour
{
    public RawImage rawImage;   
    public Vector2 scrollSpeed; 

    private Rect uvRect;

    void Start()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        uvRect = rawImage.uvRect;
    }

    void Update()
    {
        uvRect.position += scrollSpeed * Time.deltaTime;
        rawImage.uvRect = uvRect;
    }
}
