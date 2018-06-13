using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AsseetPivot))]
public class AssetsPainterEditor : Editor {

    private AsseetPivot _target;
    public AssetsPainter assetPaiterWindow;

    public int _quantity;
    private int _count;
    private bool _auto;
    private bool _delete;
    private float _radious;
    private GameObject _aux;



    private void OnEnable()
    {
        _target = (AsseetPivot)target;
        _count = 0;
        _auto = false;
        _delete = false;
        assetPaiterWindow = new AssetsPainter();
    }



    private void OnSceneGUI()
    {
        //_target.StartCoroutine(_target.UpDateOnGui());
        Handles.BeginGUI();
        _radious = 1;
        var c = Camera.current.WorldToScreenPoint(_target.transform.position);
        var p = new Rect(c.x - 50, Screen.height - c.y - 50, 100, 50);
        if (_quantity <= 0) { _quantity = 1; }
        DrawHandles();
        //if(asset)
        //DrawHandles(r);
        if (!_auto && !_delete)
            if (GUI.Button(p, "Create"))
            {

                _target.CreateAsset(_quantity, _radious);
                _count++;
            }


        Handles.EndGUI();

    }

    void DrawHandles()
    {
        //_target.asset = null;
        //_target.StartCoroutine(_target.UpDateOnGUI());
        //GUI.Box(new Rect(20,20,100,100), GUIContent.none);
        GUILayout.BeginArea(new Rect(20, 20, 120, 200));
        //GUILayout.Toggle()
        //GUI.BeginGroup(new Rect(10, 0, 100, 500));
        //GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Cantidad");
        _quantity = EditorGUILayout.IntField(_quantity);
        if (_quantity > 1)
        {
            EditorGUILayout.LabelField("Radio");
            _radious = EditorGUILayout.FloatField(_radious);
        }
            
        if (_radious > 2)
            _radious = 2;
        //GUILayout.EndHorizontal();
        //GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset:");
        _aux = (GameObject)EditorGUILayout.ObjectField(_target.asset, typeof(GameObject), true);
        _target.asset = _aux;
        //GUILayout.FlexibleSpace();
        //GUILayout.EndHorizontal();

        if (!_auto && !_delete)
            if (GUILayout.Button("Act. Auto Mode"))
            {
                _auto = true;
                _target.StartCoroutine(_target.Automatic());
                _count += _target.counter;
            }
        if (_auto && !_delete)
            if (GUILayout.Button("Stp. Auto Mode"))
            {
                _auto = false;
                _target.StopAllCoroutines();
                _target.counter = 0;
            }
        if (!_auto && !_delete)
            if (GUILayout.Button("Act. Delete Mode"))
            {
                _delete = true;
                _target.StartCoroutine(_target.DeleteMode());
            }

        if (!_auto && _delete)
            if (GUILayout.Button("Stp. Delete Mode"))
            {
                _delete = false;
                _target.StopAllCoroutines();
            }
   
        
        GUILayout.EndArea();
        //GUI.EndGroup();

        var porRect = EditorWindow.GetWindow<SceneView>().camera.pixelRect;
        GUILayout.BeginArea(new Rect(-porRect.width + porRect.width + 20, porRect.height - 70, 100, 75));
        EditorGUILayout.LabelField("Cantidad: " + _count.ToString());
        if (GUILayout.Button("Reset Position"))
        {
            _target.transform.position = Vector3.zero;
        }
        if(GUILayout.Button("Options"))
        {
            assetPaiterWindow.Show();
        }
        //GUILayout.Label("Cantidad:" + _count.ToString());
        GUILayout.EndArea();
    }

}
