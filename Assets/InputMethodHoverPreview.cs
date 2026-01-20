using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputMethodHoverPreview : MonoBehaviour
{
    [Header("Preview Image")]
    public Image previewImage;

    [Header("Sprites")]
    public Sprite digitalSprite; // Sprite 1 (default)
    public Sprite physicalSprite; // Sprite 2
    public Sprite phoneSprite; // Sprite 3

    void Start()
    {
        // Start on default sprite
        if (previewImage != null && digitalSprite != null)
            previewImage.sprite = digitalSprite;
    }

    // Call these from EventTrigger Hover events
    public void HoverDigital()
    {
        if (previewImage != null && digitalSprite != null)
            previewImage.sprite = digitalSprite;
    }

    public void HoverPhysical()
    {
        if (previewImage != null && physicalSprite != null)
            previewImage.sprite = physicalSprite;
    }

    public void HoverPhone()
    {
        if (previewImage != null && phoneSprite != null)
            previewImage.sprite = phoneSprite;
    }
}