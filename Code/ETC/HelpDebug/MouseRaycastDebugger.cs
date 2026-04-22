using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class MouseRaycastDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 클릭 시
        {
            CheckWhatWasClicked();
        }
    }

    private void CheckWhatWasClicked()
    {
        // 1. UI 클릭 여부 먼저 검사
        if (EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                GameObject uiObject = results[0].gameObject;
                Debug.Log($"<color=cyan>[UI 클릭]</color> 성함: <b>{uiObject.name}</b> | 타입: {uiObject.GetComponent<Graphic>()?.GetType().Name}");
                return;
            }
        }

        // 2. 월드(3D/2D) 오브젝트 검사
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 3D 오브젝트
            Debug.Log($"<color=yellow>[3D 클릭]</color> 이름: <b>{hit.collider.name}</b> | 태그: {hit.collider.tag} | 좌표: {hit.point}");
            Debug.DrawLine(ray.origin, hit.point, Color.red, 1.0f);
        }
        else
        {
            // 2D 오브젝트 (Physics2D)
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit2d = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit2d.collider != null)
            {
                Debug.Log($"<color=lime>[2D 클릭]</color> 이름: <b>{hit2d.collider.name}</b> | 레이어: {LayerMask.LayerToName(hit2d.collider.gameObject.layer)}");
            }
            else
            {
                Debug.Log("<color=gray>[공중 클릭]</color> 아무것도 잡히지 않았습니다.");
            }
        }
    }
}