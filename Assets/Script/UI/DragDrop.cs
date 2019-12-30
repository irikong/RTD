using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDrop : MonoBehaviour
{
    Vector3 firstPosition;

    private void OnMouseDown()
    {
        Debug.Log("OnMouseDown");

        firstPosition = this.transform.position;
    }

    private void OnMouseUp()
    {
        Debug.Log("OnMouseUp");

            this.transform.position = firstPosition;
    }

    private void OnMouseDrag()
    {
        Debug.Log("OnMouseDrag");

        float dist = 10.0f;

        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist);
        this.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
