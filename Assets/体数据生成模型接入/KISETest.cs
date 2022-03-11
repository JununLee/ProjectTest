using MRMLScene.Bridge.Services;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Thrift.Protocol;
using Thrift.Transport;
using UnityEngine;


public class KISETest : MonoBehaviour {

    private void Start()
    {
        //System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("SlicerApp-real");
        //foreach (var item in ps)
        //{
        //    item.Kill();
        //}
        //System.Diagnostics.Process process = new System.Diagnostics.Process();
        //process.StartInfo.FileName = @"E:\KIS Frame\Slicer\Slice-4.7\StartKIS.bat";
        //process.StartInfo.UseShellExecute = false;
        //process.StartInfo.CreateNoWindow = true;
        //process.Start();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Client();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReceiveStrDcm();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            DcmToNii();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SpineDivPlane();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            BrainsRegistration();
        }
    }


    void Client()
    {
        TFramedTransport transport = null;
        try
        {
            int timeout = 10;    // 设置超时10秒
                                 // TStreamTransport socket = new TSocket("127.0.0.1", 30624); // 不设置超时，直到完成
                                 // TStreamTransport socket = new TSocket("127.0.0.1", 30624, timeout);
            TStreamTransport socket = new TSocket("127.0.0.1", 30624); //
            transport = new TFramedTransport(socket);
            //协议要和服务端一致 
            TProtocol protocol = new TBinaryProtocol(transport);
            KISExportService.Client testClient = new KISExportService.Client(protocol);
            transport.Open();

            KISResultStr retStr = testClient.Ready();
            if (KISResult.SUCCESS == retStr.Result)
                Debug.Log( "远程Rpc调用接口Ready成功，返回版本号:"+ retStr.Value);
            else
                Debug.LogError("远程Rpc调用接口Ready失败，错误码："+ retStr.Result.ToString());
        }
        catch (Thrift.TException abc)
        {
            Debug.LogError(abc.StackTrace);
            Debug.LogError( "调用RPC接口Ready异常"+ abc.StackTrace);
        }
        finally
        {
            if (null != transport)
            {
                transport.Close();
            }
        }
    }
    void ReceiveStrDcm()
    {
        TFramedTransport transport = null;
        try
        {
            TStreamTransport socket = new TSocket("127.0.0.1", 30624);
            transport = new TFramedTransport(socket);
            //协议要和服务端一致 
            TProtocol protocol = new TBinaryProtocol(transport);
            KISExportService.Client testClient = new KISExportService.Client(protocol);
            transport.Open();

            string filePath = "G:/Data/dcm/4 No series description.nii";
            int W = 450, L = 550;
            double x = 0.168092, y = 107.316, z = 1142.26;
            double xROIBox = 45.0, yROIBox = 50.0, zROIBox = 108.0;
            KISResultStr retStr = testClient.SpineDicom2Polydata(filePath, W, L, x, y, z, xROIBox, yROIBox, zROIBox);
            //retStr.Value
            if (KISResult.SUCCESS != retStr.Result)
            {
                Debug.LogError("DICOMToPolydata 接口执行失败!");
            }
            string filePathWrite = "G://Data/dcm/aaa.obj";
            WriterPolydataToFile(retStr.Value, filePathWrite);
        }
        catch (Thrift.TException abc)
        {
            Debug.LogError(abc.StackTrace);
            Debug.LogError("调用RPC接口Ready异常" + abc.StackTrace);
        }
        finally
        {
            if (null != transport)
            {
                transport.Close();
            }
        }
    }
    void DcmToNii()
    {
        TFramedTransport transport = null;
        try
        {
            TStreamTransport socket = new TSocket("127.0.0.1", 30624);
            transport = new TFramedTransport(socket);
            //协议要和服务端一致 
            TProtocol protocol = new TBinaryProtocol(transport);
            KISExportService.Client testClient = new KISExportService.Client(protocol);
            transport.Open();

            string dicomDir = @"G:\Spine123\SpineDcm";
            string niftiDir = "G://Data/Dcm";
            KISResultListStr ret = testClient.SpineDicom2Nifti(dicomDir, niftiDir);
            if (ret.Value.Count> 0)
            {
                string fileList = string.Empty;
                fileList = string.Join("\r\n", ret.Value.ToArray());
                Debug.Log("DICOMToNifti 接口返回文件列表!  " + fileList);
                File.Move(fileList, "G://Data/Dcm/test1.nii");
            }
            if (KISResult.SUCCESS != ret.Result)
            {
                Debug.LogError("DICOMToNifti 接口执行失败!");
            }
        }
        catch (Thrift.TException abc)
        {
            Debug.LogError(abc.StackTrace);
            Debug.LogError("调用RPC接口Ready异常" + abc.StackTrace);
        }
        finally
        {
            if (null != transport)
            {
                transport.Close();
            }
        }
    }
    void SpineDivPlane()
    {
        TFramedTransport transport = null;
        try
        {
            TStreamTransport socket = new TSocket("127.0.0.1", 30624);
            transport = new TFramedTransport(socket);
            //协议要和服务端一致 
            TProtocol protocol = new TBinaryProtocol(transport);
            KISExportService.Client testClient = new KISExportService.Client(protocol);
            transport.Open();

            string dicomPath = "G:/Data/dcm/test1.nii";
            int W = 450, L = 550;
            double x = 0.168092, y = -208.5, z = -125.1;
            double xROIBox = 60.0, yROIBox = 70.0, zROIBox = 120.0;
            KISResultStr retStr = testClient.SpineDivPlane(dicomPath, W, L, x, y, z, xROIBox, yROIBox, zROIBox);
            //retStr.Value
            if (KISResult.SUCCESS != retStr.Result)
            {
                Debug.LogError("DICOMToPolydata 接口执行失败!");
            }
            string filePath = "G://Data/Dcm/C-Sharp_SpineDivPlane.csv";
            WriterPolydataToFile(retStr.Value, filePath);
        }
        catch (Thrift.TException abc)
        {
            Debug.LogError(abc.StackTrace);
            Debug.LogError("调用RPC接口Ready异常" + abc.StackTrace);
        }
        finally
        {
            if (null != transport)
            {
                transport.Close();
            }
        }
    }
    void BrainsRegistration()
    {
        TFramedTransport transport = null;
        try
        {
            TStreamTransport socket = new TSocket("127.0.0.1", 30624);
            transport = new TFramedTransport(socket);
            //协议要和服务端一致 
            TProtocol protocol = new TBinaryProtocol(transport);
            KISExportService.Client testClient = new KISExportService.Client(protocol);
            transport.Open();

            string fileFixedImage = "G:/Data/Dcm/CTMRI/CT_4 No series description.nii";
            string fileMoveImage = "G:/Data/Dcm/CTMRI/MT_701 No series description.nii";
            KISResultMatrix retMatrix = testClient.BrainsRegistration(fileFixedImage, fileMoveImage);
            //retStr.Value
                    if (KISResult.SUCCESS != retMatrix.Result)
            {
                Debug.LogError("DICOMToPolydata 接口执行失败!");
            }
            else
            {
                foreach (double item in retMatrix.Value)
                {
                    Debug.Log(item);
                }
            }
        }
        catch (Thrift.TException abc)
        {
            Debug.LogError(abc.StackTrace);
            Debug.LogError("调用RPC接口Ready异常" + abc.StackTrace);
        }
        finally
        {
            if (null != transport)
            {
                transport.Close();
            }
        }

    }
    private void WriterPolydataToFile(string data, string filePath)
    {
        //string path = "D:/Colleagues/Test/1/cellsAA/C-Sharp_TestFile.obj";
        FileStream fs = new FileStream(filePath, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        //开始写入
        //sw.Write("Hello World!!!!");
        sw.Write(data);
        //清空缓冲区
        sw.Flush();
        //关闭流
        sw.Close();
        fs.Close();
    }
}
