﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeMaperWindow : EditorWindow {

    private GameObject _pivot;
    private GameObject _target;
    private GameObject _node;

    public float dirtModifier;
    public float waterModifier;

    private GameObject NodeParent;

    //Node n = new Node();

    public List<GameObject> nodes;

    int filas;
    int columnas;

    private float x;
    private float y;
    private float z;

    private float _disX;
    private float _disZ;

    bool pivot;
    bool target;
    bool node;
    bool parent;

    [MenuItem("CustomTools/NodeMaper")] 
    public static void OpenWindow() 
    {
        NodeMaperWindow myWindow = (NodeMaperWindow)GetWindow(typeof(NodeMaperWindow));
        myWindow.wantsMouseMove = true;
        myWindow.Show();
        
        myWindow.nodes = new List<GameObject>();
    }

    private void OnGUI() 
    {
        
        
        _pivot =  (GameObject)EditorGUILayout.ObjectField("Pivot", _pivot, typeof(GameObject), true);
        if(_pivot != null) { pivot = true; }
        _target = (GameObject)EditorGUILayout.ObjectField("Target", _target, typeof(GameObject), true);
        if (_target != null)
        {
            if (_target.gameObject.layer == LayerConstants.floor ||
                _target.gameObject.layer == LayerConstants.water ||
                _target.gameObject.layer == LayerConstants.dirt)
            target = true;
            else
            {
                EditorGUILayout.HelpBox("El objeto no pertenece a una capa compatible", MessageType.Error);
            }
        }
        _node = (GameObject)EditorGUILayout.ObjectField("Node", _node, typeof(GameObject), true);
       
        if(_node != null)
        {
            if (!_node.GetComponent<Node>()) { EditorGUILayout.HelpBox("No contiene el componente Node", MessageType.Error); }
            node = true;
        }

        NodeParent = (GameObject)EditorGUILayout.ObjectField("Contenedor", NodeParent, typeof(GameObject), true);
        if(NodeParent != null) { parent = true; }

        EditorGUILayout.Space();

        filas = EditorGUILayout.IntField("Filas", filas);
        if(filas <= 0) { filas = 1; }

        columnas = EditorGUILayout.IntField("Columnas", columnas);
        if (columnas <= 0) { columnas = 1; }

        EditorGUILayout.Space();

        dirtModifier = EditorGUILayout.FloatField("Dirt Weight", dirtModifier);
        waterModifier = EditorGUILayout.FloatField("Water Weight", waterModifier);

        EditorGUILayout.Space();

        if(pivot && target && node && parent)
        {
            if (GUILayout.Button("Map"))
            {
                takeBounds();
                Maper();
                Debug.Log(nodes.Count);
                foreach (var item in nodes)
                {
                    item.GetComponent<Node>().SearchLinks();
                }
            }
        }


    }



    void takeBounds()
    {
        x = _target.GetComponent<MeshRenderer>().bounds.size.x / 2 - 0.1f;
        y = _target.GetComponent<MeshRenderer>().bounds.size.y;
        z = _target.GetComponent<MeshRenderer>().bounds.size.z / 2 - 0.1f;

        _disX = (x * 2) / columnas;
        _disZ = (z * 2) / filas;

        _pivot.transform.position = _target.transform.position;
        _pivot.transform.position = new Vector3(_pivot.transform.position.x - x, y + 3, _pivot.transform.position.z - z);
    }



    void NodeCreator(Vector3 trans, float multiplyer)
    {
        var node = Instantiate(_node);
        node.transform.position = trans;
        node.GetComponent<Node>().weightModifier = multiplyer;
        nodes.Add(node);
        node.GetComponent<Node>().radious = _disZ / 2;//Lalala
        node.transform.SetParent(NodeParent.transform);
        node.SetActive(false);
    }



    void Maper()
    {

        Vector3 initPos = _pivot.transform.position;

        for (int i = 0; i < filas + 1; i++)
        {
            //CreatNode(transform);
            RaycastHit firstHit;
            if (Physics.Raycast(_pivot.transform.position, -_pivot.transform.up, out firstHit, y + 4f))
            {
                if (firstHit.collider.gameObject.layer == LayerConstants.floor) NodeCreator(firstHit.point, 1);
                else if (firstHit.collider.gameObject.layer == LayerConstants.water) NodeCreator(firstHit.point, waterModifier);
                else if (firstHit.collider.gameObject.layer == LayerConstants.dirt) NodeCreator(firstHit.point, dirtModifier);
            }

            for (int j = 0; j < columnas; j++)
            {
                _pivot.transform.position = new Vector3(_pivot.transform.position.x + _disX, _pivot.transform.position.y, _pivot.transform.position.z);
                //CreatNode(transform);
                //ThrowRayCast(transform);
                RaycastHit hit;
                if (Physics.Raycast(_pivot.transform.position, -_pivot.transform.up, out hit, 11f))
                {
                    //Debug.Log("tiro el ray");
                    if (hit.collider.gameObject.layer == LayerConstants.floor) NodeCreator(hit.point, 1);
                    else if (hit.collider.gameObject.layer == LayerConstants.water) NodeCreator(hit.point, waterModifier);
                    else if (hit.collider.gameObject.layer == LayerConstants.dirt) NodeCreator(hit.point, dirtModifier);
                }

            }

            _pivot.transform.position = new Vector3(initPos.x, initPos.y, initPos.z + _disZ);
            initPos = _pivot.transform.position;
        }


        foreach (var item in nodes)
        {
            if (item.activeSelf == false)
            {
                item.SetActive(true);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        var a = _target.GetComponent<MeshRenderer>().bounds.size;
        Gizmos.DrawWireCube(_target.transform.position, a);

        if (nodes != null)
        {
            foreach (var item in nodes)
            {
                Gizmos.DrawWireCube(item.transform.position, new Vector3(0.1f, 0.1f, 0.1f));
                //Gizmos.DrawLine(item.transform.position, item.transform.position - item.transform.up * 5);
                //Gizmos.DrawIcon(item.transform.position, item.name+ "1");

            }
        }

        //Gizmos.DrawLine(transform.position, transform.position - this.transform.up * 5);


    }

}