using UnityEngine;
using UnityEngine.EventSystems;

public class HoverDebug : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Button hovered");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Button hover exit");
    }
}
