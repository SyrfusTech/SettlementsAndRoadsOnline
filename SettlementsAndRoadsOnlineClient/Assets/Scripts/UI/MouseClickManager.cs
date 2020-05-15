using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class MouseClickManager : MonoBehaviour
{
    public Camera m_camera;

    private void Update()
    {
        bool lmb = Input.GetMouseButtonDown((int)MouseButton.LeftMouse);
        bool rmb = Input.GetMouseButtonDown((int)MouseButton.RightMouse);
        if (lmb || rmb)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            CastRay(lmb, rmb);
        }
    }

    private void CastRay(bool _lmb, bool _rmb)
    {
        Vector3 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        int layerMask = LayerMask.GetMask("Board");

        RaycastHit2D raycastHit = Physics2D.Raycast(mousePos2D, Vector2.zero, layerMask);
        if (raycastHit.collider != null)
        {
            var hitClickableObjects = raycastHit.collider.GetComponents<IClickableObject>();

            if (_lmb)
            {
                foreach (var clickableObject in hitClickableObjects)
                    clickableObject.OnLMBClick(raycastHit.point);
            }
            if (_rmb)
            {
                foreach (var clickableObject in hitClickableObjects)
                    clickableObject.OnRMBClick(raycastHit.point);
            }
        }
    }
}
