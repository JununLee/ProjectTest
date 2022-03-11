using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class T3DX : MonoBehaviour {

    public VolumeViewer.VolumeFileLoader fileload;
    private VolumeViewer.Volume volume;

    public Transform cube;
    public Transform plane;
    public Material m;
    public Transform parent;

    Vector4[] pPoints = new Vector4[] { new Vector3(0, 0, -0.25f), new Vector3(0, 0, 0.25f), new Vector3(-0.1f, 0, 0), new Vector3(0.1f, 0, 0), new Vector3(0, -0.15f, 0), new Vector3(0, 0.25f, 0) };
    Vector4[] pNormals = new Vector4[] { new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, -1, 0) };
    private void Start()
    {
        StartCoroutine(load());
        
    }
    private void Update()
    {
        if (volume != null)
        {
            m.SetTexture("_Volume", volume.texture);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            m.SetTexture("_Volume",cube.GetComponent<VolumeViewer.VolumeComponent>().data);
        }
        m.SetMatrix("_world2LocalMatrix", cube.worldToLocalMatrix);
        parent.LookAt(Camera.main.transform);
        m.SetVector("_forward", plane.forward);
        m.SetFloat("minDataValue",-1024f);
        m.SetFloat("maxDataValue", 3071f);
        m.SetVectorArray("pPoint", pPoints);
        m.SetVectorArray("pNormal", pNormals);

        if (Input.GetKeyDown(KeyCode.A))
        {
            m.EnableKeyword("DR");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            m.DisableKeyword("DR");
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            m.EnableKeyword("WHOLE");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            m.DisableKeyword("WHOLE");
        }
    }
    IEnumerator load()
    {
        yield return StartCoroutine(fileload.loadFiles(@"G:\liaoximing.nii"));
        volume = fileload.dataVolume;
    }
}
