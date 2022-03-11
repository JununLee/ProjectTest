using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseOverUI : MonoBehaviour {

    public GraphicRaycaster CanvasUI;
    public EventSystem eventSystem;

    private void Update()
    {


        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(EventSystem.current.IsPointerOverGameObject());
            CheckSecondUI(Input.mousePosition);
        }
    }
    public void CheckSecondUI(Vector2 pos)
    {
        List<GameObject> objList = new List<GameObject>();
        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.pressPosition = pos;
        eventData.position = pos;

        List<RaycastResult> list = new List<RaycastResult>();
        CanvasUI.Raycast(eventData, list);
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Debug.Log(list[i].gameObject.name);
            }
        }
        else
        {
            Debug.Log("没有UI");
        }
    }
}
