using UnityEngine;
using UnityEngine.UI;

public class TextureOffset : MonoBehaviour
{
    Image image;
    public float speedX = 0.5f;
    float offsetX = 0;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        offsetX += Time.deltaTime * speedX;
        if(offsetX > 0.5f)
        {
            offsetX = 0;
        }else if(offsetX < 0)
        {
            offsetX = 0.5f;
        }
        image.material.mainTextureOffset = new Vector2(offsetX, 0);
    }
}
