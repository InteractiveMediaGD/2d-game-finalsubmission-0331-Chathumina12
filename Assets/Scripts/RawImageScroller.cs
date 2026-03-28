using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A simple continuously scrolling script for Canvas RawImage elements.
/// Perfect for seamless animated main menu backgrounds.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class RawImageScroller : MonoBehaviour
{
    public float scrollSpeedX = 0.05f;
    public float scrollSpeedY = 0f;

    private RawImage targetImage;

    private void Start()
    {
        targetImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (targetImage != null)
        {
            Rect r = targetImage.uvRect;
            r.x += scrollSpeedX * Time.deltaTime;
            r.y += scrollSpeedY * Time.deltaTime;
            targetImage.uvRect = r;
        }
    }
}
