using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ImposterRecorder : MonoBehaviour
{
    [Header("Texture Setting")] 
    public int resolution = 512;

    public string saveFolder;

    [Header("Camera Setting")]
    public int widthNumber = 1;

    public int heightNumber = 1;

    [Header("Camera Position")]
    public float camDistance = 1;

    public float camHeight = 0;
    
    public Vector3 lookOffset = Vector3.zero;

    public float widAngle = 180;

    public float heiAngle = 90;

    private List<GameObject> camList;

    public void MakeAtlasTexture()
    {
        RenderAltas();
    }

    public void ResetCamera()
    {
        if (widthNumber < 1 || heightNumber < 1)
            return;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if(child.GetComponent<Camera>())
                DestroyImmediate(child.gameObject);
        }

        for (int i = 0; i < widthNumber * heightNumber; i++)
        {
            GameObject obj = new GameObject($"camera_{i}", typeof(Camera));
            Transform camTrans = obj.GetComponent<Transform>();
            Camera cam = obj.GetComponent<Camera>();
            camTrans.SetParent(transform);
            cam.fieldOfView = 45;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        ResetCameraPos();
    }

    public void ResetCameraPos()
    {
        int i = 0;
        float maxWidAngle = 180;
        float maxHeiAngle = 90;
        for (int j = 0; j < transform.childCount; j++)
        {
            Transform camTrans = transform.GetChild(j);
            if(!camTrans.GetComponent<Camera>())
                continue;

            float widAngleUnit = widAngle / (widthNumber - 1);
            float heiAngleUnit = heiAngle / heightNumber; // 不用最上方角度
            float theta = ((maxHeiAngle - heiAngle) + (i / heightNumber + 1) * heiAngleUnit) * Mathf.Deg2Rad;
            float phi = ((maxWidAngle - widAngle) * 0.5f + i % widthNumber * widAngleUnit) * Mathf.Deg2Rad;

            camTrans.localPosition = camDistance * new Vector3(
             Mathf.Sin(theta) * Mathf.Cos(phi),
             Mathf.Cos(theta),
             Mathf.Sin(theta) * Mathf.Sin(phi)
             );
            camTrans.localPosition += Vector3.up * camHeight;
            camTrans.LookAt(transform.position + lookOffset);
            i++;
        }
    }

    public void RenderAltas()
    {
        if (string.IsNullOrEmpty(saveFolder) || resolution <= 0)
            return;
        int w = Mathf.FloorToInt(resolution / widthNumber);
        int h = Mathf.FloorToInt(resolution / heightNumber);

        var target = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
        var texOut = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false, true);
        var lastActive = RenderTexture.active;
        RenderTexture.active = target;

        int i = 0;
        for (int j = 0; j < transform.childCount; j++)
        {
            Transform camTrans = transform.GetChild(j);
            Camera cam = camTrans.GetComponent<Camera>();
            if(!cam)
                continue;
            var lastTargetSet = cam.targetTexture;
            cam.targetTexture = target;
            cam.Render();
            cam.targetTexture = lastTargetSet;
            
            texOut.ReadPixels(new Rect(0, 0, w, h), (i%widthNumber)*w, (heightNumber - i/heightNumber - 1)*h);
            texOut.Apply();
            i++;
        }

        for (i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                //用于测试的红线
                // if (i % w == 0 || j % h == 0)
                // {
                //     texOut.SetPixel(i, j, new Color(1, 0, 0, 1));
                // }
                
                // 边缘多余像素写0alpha
                if (i >= widthNumber * w || j >= heightNumber * h)
                {
                    texOut.SetPixel(i, j, new Color(1, 1, 1, 0));
                }
            }
        }

        RenderTexture.active = lastActive;
        var bytess = texOut.EncodeToPNG();
        File.WriteAllBytes(saveFolder, bytess);
        AssetDatabase.Refresh();
        RenderTexture.ReleaseTemporary(target);
        DestroyImmediate(texOut);
        
        Debug.Log(string.Format("png制作完毕:{0}", saveFolder));
    }
    
}
