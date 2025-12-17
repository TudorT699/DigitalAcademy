using UnityEngine;
using UnityEngine.EventSystems;

public class UIPointerDebug : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("POINTER ENTER");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("POINTER CLICK");
    }
}