using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaScript : MonoBehaviour {

    public Transform cyline;

    Vector3 mouswPos;
    Vector3 cylPos;
    Vector3 topOrBottom;
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            mouswPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.worldToLocalMatrix.MultiplyPoint3x4(cyline.position).z));
            Vector3 x = Vector3.ProjectOnPlane(cyline.up, -transform.forward);
            Vector3 dir = mouswPos - cyline.position;
            float angle = Vector3.Dot(x.normalized, dir.normalized);
            if (angle < 0)
            {
                x= Vector3.ProjectOnPlane(-cyline.up, -transform.forward);
                angle = Vector3.Dot(x.normalized, dir.normalized);
            }
            if (Vector3.Dot(Vector3.Cross(x.normalized, dir.normalized), -transform.forward) >= 0)
                angle = Mathf.Rad2Deg * Mathf.Acos(angle);
            else
                angle = -Mathf.Rad2Deg * Mathf.Acos(angle);
            if (!float.IsNaN(angle))
                cyline.RotateAround(cyline.position, -transform.forward, angle);
        }
    }
}
