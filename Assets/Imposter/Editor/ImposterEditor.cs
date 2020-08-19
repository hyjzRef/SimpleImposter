using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ImposterRecorder))]
public class ImposterEditor : Editor
{
    private ImposterRecorder recorder;

    void OnEnable()
    {
        recorder = (ImposterRecorder) target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.LabelField("Texture Setting");
        recorder.resolution = EditorGUILayout.IntField("Resolution", recorder.resolution);
        EditorGUILayout.BeginHorizontal();
        recorder.saveFolder = EditorGUILayout.TextField("saveFolder", recorder.saveFolder);
        if (GUILayout.Button("Select"))
        {
            recorder.saveFolder = EditorUtility.SaveFilePanel("PathToSave", recorder.saveFolder, "altas", "png");
        }
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Make Atlas Texture"))
        {
            recorder.MakeAtlasTexture();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Camera Setting");
        recorder.widthNumber = EditorGUILayout.IntSlider("Width Number", recorder.widthNumber, 1, 20);
        recorder.heightNumber = EditorGUILayout.IntSlider("Height Number", recorder.heightNumber, 1, 20);
        if (GUILayout.Button("Reset Camera"))
        {
            recorder.ResetCamera();
        }
        
        EditorGUILayout.LabelField("Camera Position");
        Vector3 lastOffset = recorder.lookOffset;
        Vector4 lastValue = new Vector4(recorder.camDistance, recorder.camHeight, recorder.widAngle, recorder.heiAngle);
        recorder.camDistance = EditorGUILayout.FloatField("Cam Distance", recorder.camDistance);
        recorder.camHeight = EditorGUILayout.FloatField("Cam Height", recorder.camHeight);
        recorder.lookOffset = EditorGUILayout.Vector3Field("Look Offset", recorder.lookOffset);
        recorder.widAngle = EditorGUILayout.Slider("Wid Angle", recorder.widAngle, 0, 180);
        recorder.heiAngle = EditorGUILayout.Slider("Hei Angle", recorder.heiAngle, 0, 90);
        if (lastValue != new Vector4(recorder.camDistance, recorder.camHeight, recorder.widAngle, recorder.heiAngle) ||
            lastOffset != recorder.lookOffset)
            recorder.ResetCameraPos();
        
        EditorGUILayout.EndVertical();
    }
}
