////--------------------------------------------------------------------
/// Namespace:          
/// Class:              IntersectionSlicer
/// Description:        Manages sagittal, coronal and transverse slices
///                         through a VolumeComponent. Updates Cameras, 
///                         SliceIndicators, Crosshairs and
///                         VolumeIntersectionRenderers.
/// Author:             LISCINTEC
///                         http://www.liscintec.com
///                         info@liscintec.com
/// Date:               Feb 2017
/// Notes:              -
/// Revision History:   First release
/// 
/// This file is part of the examples of the Volume Viewer Pro Package.
/// Volume Viewer Pro is a Unity Asset Store product.
/// https://www.assetstore.unity3d.com/#!/content/83185
////--------------------------------------------------------------------

using UnityEngine;

public class IntersectionSlicer : MonoBehaviour {

    public Camera mainCamera;
    public VolumeViewer.VolumeIntersectionRenderer sagittalRenderer;
    public VolumeViewer.VolumeIntersectionRenderer coronalRenderer;
    public VolumeViewer.VolumeIntersectionRenderer transverseRenderer;
    
    public Crosshairs sagittalCrosshairs;
    public Crosshairs coronalCrosshairs;
    public Crosshairs transverseCrosshairs;

    public Transform sagittalPlane;
    public Transform coronalPlane;
    public Transform transversePlane;

    public Transform slicer;

    public RectTransform UIView;
    
    public RectTransform sagittalPickerTransform;
    public RectTransform coronalPickerTransform;
    public RectTransform transversePickerTransform;
    
    Camera sagittalCamera;
    Camera coronalCamera;
    Camera transverseCamera;

    Vector3 focusPoint = Vector3.zero;
    Vector3 invertAxes = Vector3.zero;

    void Start()
    {
        sagittalCamera = sagittalRenderer.GetComponent<Camera>();
        coronalCamera = coronalRenderer.GetComponent<Camera>();
        transverseCamera = transverseRenderer.GetComponent<Camera>();
    }

    public void UpdateCameraPosition(Vector2 newPos)
    {
        //Workaround to get rid of the crosshairs in case 3D View is made fullscreen
        Vector2 fixPos = newPos;
        if ((newPos - new Vector2(0, 1f)).magnitude <= Mathf.Epsilon)
        {
            sagittalCrosshairs.Hide();
            coronalCrosshairs.Hide();
            transverseCrosshairs.Hide();
            fixPos = new Vector2(-0.01f, 1.01f);
        }
        else
        {
            sagittalCrosshairs.Show();
            coronalCrosshairs.Show();
            transverseCrosshairs.Show();
        }

        transverseCamera.rect = new Rect(0, 0, fixPos.x, fixPos.y);
        coronalCamera.rect = new Rect(0, fixPos.y, fixPos.x, 1 - fixPos.y);
        sagittalCamera.rect = new Rect(fixPos.x, fixPos.y, 1 - fixPos.x, 1 - fixPos.y);
        mainCamera.rect = new Rect(fixPos.x, 0, 1 - fixPos.x, fixPos.y);

        UIView.anchorMin = new Vector2(newPos.x, 0);
        UIView.anchorMax = new Vector2(1, newPos.y);

        sagittalPickerTransform.anchorMin = newPos;
        sagittalPickerTransform.anchorMax = Vector2.one;
        
        coronalPickerTransform.anchorMin = new Vector2(0, newPos.y);
        coronalPickerTransform.anchorMax = new Vector2(newPos.x, 1);

        transversePickerTransform.anchorMin = Vector2.zero;
        transversePickerTransform.anchorMax = newPos;

        sagittalCrosshairs.UpdateAspectRatio();
        coronalCrosshairs.UpdateAspectRatio();
        transverseCrosshairs.UpdateAspectRatio();

    }

    public void ChangeSliceIndicator(int value)
    {
        switch(value)
        {
            case 2:
                slicer.gameObject.SetActive(true);
                sagittalPlane.gameObject.SetActive(false);
                coronalPlane.gameObject.SetActive(false);
                transversePlane.gameObject.SetActive(false);
                break;
            case 1:
                slicer.gameObject.SetActive(false);
                sagittalPlane.gameObject.SetActive(true);
                coronalPlane.gameObject.SetActive(true);
                transversePlane.gameObject.SetActive(true);
                break;
            default:
                slicer.gameObject.SetActive(false);
                sagittalPlane.gameObject.SetActive(false);
                coronalPlane.gameObject.SetActive(false);
                transversePlane.gameObject.SetActive(false);
                break;
        }
    }

    private Vector2 tidyVec2D(Vector2 pos)
    {
        pos.x = float.IsInfinity(pos.x) || float.IsNaN(pos.x) || Mathf.Abs(pos.x) < Mathf.Epsilon ? 0 : pos.x;
        pos.y = float.IsInfinity(pos.y) || float.IsNaN(pos.y) || Mathf.Abs(pos.y) < Mathf.Epsilon ? 0 : pos.y;
        return pos;
    }

    public void SagittalCrossMoved(Vector2 newPos)
    {
        newPos = tidyVec2D(newPos);
        float sagittalRatio = sagittalCamera.rect.width / sagittalCamera.rect.height;
        sagittalRatio = float.IsInfinity(sagittalRatio)  || float.IsNaN(sagittalRatio) || Mathf.Abs(sagittalRatio) < Mathf.Epsilon ? 1 : sagittalRatio;
        float aspectRatio = (float) Screen.width / Screen.height;
        focusPoint.y = (newPos.x - 0.5f) * aspectRatio * sagittalRatio;
        focusPoint.z = newPos.y - 0.5f;

        coronalRenderer.planeOffset = focusPoint.y;
        transverseRenderer.planeOffset = focusPoint.z;
        
        sagittalCrosshairs.UpdateCrosshairs(newPos);
        coronalCrosshairs.UpdateH(newPos.y);
        transverseCrosshairs.UpdateH(focusPoint.y + 0.5f);

        Vector3 position = coronalPlane.localPosition;
        position.y = focusPoint.y;
        coronalPlane.localPosition = position;

        position = transversePlane.localPosition;
        position.z = focusPoint.z;
        transversePlane.localPosition = position;

        UpdateSlicer();
    }

    public void CoronalCrossMoved(Vector2 newPos)
    {
        newPos = tidyVec2D(newPos);
        float coronalRatio = coronalCamera.rect.width / coronalCamera.rect.height;
        coronalRatio = float.IsInfinity(coronalRatio)  || float.IsNaN(coronalRatio) || Mathf.Abs(coronalRatio) < Mathf.Epsilon ? 1 : coronalRatio;
        float transverseRatio = transverseCamera.rect.width / transverseCamera.rect.height;
        transverseRatio = float.IsInfinity(transverseRatio)  || float.IsNaN(transverseRatio) || Mathf.Abs(transverseRatio) < Mathf.Epsilon ? 1 : transverseRatio;
        float aspectRatio = (float) Screen.width / Screen.height;
        focusPoint.x = (newPos.x - 0.5f) * aspectRatio * coronalRatio;
        focusPoint.z = newPos.y - 0.5f;

        sagittalRenderer.planeOffset = focusPoint.x;
        transverseRenderer.planeOffset = focusPoint.z;

        coronalCrosshairs.UpdateCrosshairs(newPos);
        sagittalCrosshairs.UpdateH(newPos.y);
        transverseCrosshairs.UpdateV((newPos.x - 0.5f) * coronalRatio / transverseRatio + 0.5f);

        Vector3 position = sagittalPlane.localPosition;
        position.x = focusPoint.x;
        sagittalPlane.localPosition = position;

        position = transversePlane.localPosition;
        position.z = focusPoint.z;
        transversePlane.localPosition = position;

        UpdateSlicer();
    }

    public void TransverseCrossMoved(Vector2 newPos)
    {
        newPos = tidyVec2D(newPos);
        float sagittalRatio = sagittalCamera.rect.width / sagittalCamera.rect.height;
        sagittalRatio = float.IsInfinity(sagittalRatio)  || float.IsNaN(sagittalRatio) || Mathf.Abs(sagittalRatio) < Mathf.Epsilon ? 1 : sagittalRatio;
        float coronalRatio = coronalCamera.rect.width / coronalCamera.rect.height;
        coronalRatio = float.IsInfinity(coronalRatio)  || float.IsNaN(coronalRatio) || Mathf.Abs(coronalRatio) < Mathf.Epsilon ? 1 : coronalRatio;
        float transverseRatio = transverseCamera.rect.width / transverseCamera.rect.height;
        transverseRatio = float.IsInfinity(transverseRatio)  || float.IsNaN(transverseRatio) || Mathf.Abs(transverseRatio) < Mathf.Epsilon ? 1 : transverseRatio;
        float aspectRatio = (float) Screen.width / Screen.height;

        focusPoint.x = (newPos.x - 0.5f) * aspectRatio * transverseRatio;
        focusPoint.y = newPos.y - 0.5f;

        sagittalRenderer.planeOffset = focusPoint.x;
        coronalRenderer.planeOffset = focusPoint.y;
        
        transverseCrosshairs.UpdateCrosshairs(newPos);
        sagittalCrosshairs.UpdateV((focusPoint.y / aspectRatio / sagittalRatio) + 0.5f);
        coronalCrosshairs.UpdateV(((newPos.x - 0.5f) * transverseRatio / coronalRatio) + 0.5f);

        Vector3 position = sagittalPlane.localPosition;
        position.x = focusPoint.x;
        sagittalPlane.localPosition = position;

        position = coronalPlane.localPosition;
        position.y = focusPoint.y;
        coronalPlane.localPosition = position;

        UpdateSlicer();
    }

    void UpdateSlicer()
    {
        Vector3 position = (Vector3.one/2.0f + focusPoint) / 2.0f;
        Vector3 scale = Vector3.one/2.0f - focusPoint;
        
        invertAxes.x = focusPoint.x < -0.499 ? 1 : invertAxes.x;
        invertAxes.x = focusPoint.x >  0.499 ? 0 : invertAxes.x;
        invertAxes.y = focusPoint.y < -0.499 ? 1 : invertAxes.y;
        invertAxes.y = focusPoint.y >  0.499 ? 0 : invertAxes.y;
        invertAxes.z = focusPoint.z < -0.499 ? 1 : invertAxes.z;
        invertAxes.z = focusPoint.z >  0.499 ? 0 : invertAxes.z;

        position.x = invertAxes.x > 0 ? (-0.5f + focusPoint.x) / 2.0f : position.x;
        position.y = invertAxes.y > 0 ? (-0.5f + focusPoint.y) / 2.0f : position.y;
        position.z = invertAxes.z > 0 ? (-0.5f + focusPoint.z) / 2.0f : position.z;
        
        scale.x = invertAxes.x > 0 ? 1-scale.x : scale.x;
        scale.y = invertAxes.y > 0 ? 1-scale.y : scale.y;
        scale.z = invertAxes.z > 0 ? 1-scale.z : scale.z;

        slicer.localPosition = position;
        slicer.localScale = scale;
    }


}