using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointDrag : MonoBehaviour
{
    EightBoxController eightBoxController;
    private void Awake()
    {
        eightBoxController = transform.parent.GetComponent<EightBoxController>();
    }
    
    public Vector3 position
    {
        set
        {
            this.transform.position = value;
            eightBoxController.updaataMesh();
        }
    }
}
