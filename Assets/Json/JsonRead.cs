using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonRead : MonoBehaviour {

    private void Start()
    {
        Debug.Log(Application.dataPath + "/Json/CannulaType.json");
        StreamReader sr = new StreamReader(Application.dataPath + "/Json/CannulaType.json");
        string json = sr.ReadToEnd();
        CannulaTypes cannulaTypes = JsonUtility.FromJson<CannulaTypes>(json);
        Debug.Log(cannulaTypes.data[0].cannulaType);
        int a = 0;
    }
    private void Update()
    {
    }
}
[System.Serializable]
public class CannulaTypes
{
    public Cannula[] data;
}
[System.Serializable]
public class Cannula
{
    public string cannulaType;
    public float Length;
    public float[] Color;
}