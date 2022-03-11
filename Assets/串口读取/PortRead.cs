using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO.Ports;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;

public class PortRead : MonoBehaviour {
    
    private SerialPort sp;
    private Thread recvThread;//线程

    // Use this for initialization   
    void Start()
    {
        sp = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
        //串口初始化  
        if (!sp.IsOpen)
        {
            sp.Open();
        }
        recvThread = new Thread(ReceiveData); //该线程用于接收串口数据  
        recvThread.Start();
    }
    void Update()
    {
        //...
    }

    private void ReceiveData()
    {
        try
        {
            string s = "";
            //以行的模式读取串口数据
            while ((s = sp.ReadLine()) != null)
            {
                Debug.Log(s); //打印读取到的每一行数据
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    void OnApplicationQuit()
    {
        sp.Close();//关闭串口
    }
    
}
