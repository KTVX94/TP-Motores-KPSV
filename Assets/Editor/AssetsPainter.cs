using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetsPainter : EditorWindow {

    public AssetsPainterEditor ape;


    private GameObject _myPivot;
    public AssetsPainterEditor myEd;

    //[MenuItem("CustomTools/AssetsPainter")]
    public static void OpenWindow(AssetsPainterEditor ed)
    {
        AssetsPainter myWindow = (AssetsPainter)GetWindow(typeof(AssetsPainter));
        myWindow.wantsMouseMove = true;
        myWindow.myEd = ed;
        myWindow.Show();
        
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Asset Painter Options", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
       
    }
}
