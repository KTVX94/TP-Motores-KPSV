using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeMaperWindow : EditorWindow {

    private GameObject _pivot;
    private GameObject _target;
    private GameObject _node;

    private GUIStyle _mainHeaderStyle;

    private List<int> takenLayers; //Para chequear que no se ponga un terreno en una layer ya ocupada

    private TerrInfo[] _arrayOfTerrains; //Para el mapper

    //Para setear los parametros del terreno
    public int lyr;
    public float weight;
    public string terrName;

    private GameObject container; //Contiene los terrenos

    private GameObject NodeParent;

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

        //crea todas las cosas
        if (myWindow.takenLayers == null)
        {
            myWindow.takenLayers = new List<int>();
        }

        myWindow.container = GameObject.Find("Terrain Container");
        if (myWindow.container == null) myWindow.container = new GameObject();
        myWindow.container.transform.position = new Vector3(0, 0, 0);
        myWindow.container.name = "Terrain Container";

        myWindow._mainHeaderStyle = new GUIStyle();
        myWindow._mainHeaderStyle.alignment = TextAnchor.MiddleCenter;
        myWindow._mainHeaderStyle.fontStyle = FontStyle.Bold;

        myWindow.nodes = new List<GameObject>();
        myWindow.terrName = "";
        myWindow.weight = 1;
        myWindow.lyr = 9;
    }

    private void OnGUI() 
    {
        
        
        _pivot =  (GameObject)EditorGUILayout.ObjectField("Pivot", _pivot, typeof(GameObject), true);
        if(_pivot != null) { pivot = true; }
        _target = (GameObject)EditorGUILayout.ObjectField("Target", _target, typeof(GameObject), true);
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

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Terrain Creator", _mainHeaderStyle);
        EditorGUILayout.Space();

        CreateTerrain();

    }

    void CreateTerrain()
    {
        //Setea los parametros del terreno por editor
        terrName = EditorGUILayout.TextField("Name", terrName);
        weight = EditorGUILayout.FloatField("Weight", weight);
        lyr = EditorGUILayout.IntField("Layer", lyr);

        if (GUILayout.Button("Create"))
        {
            for (int i = 0; i < takenLayers.Count; i++)
            {
                if (lyr == takenLayers[i])
                {
                    return;
                }
            }

            TerrInfo toAssign = container.AddComponent<TerrInfo>(); //Crea el terreno
            toAssign.terrweight = weight;
            toAssign.terrlyr = lyr;
            toAssign.terr_Name = terrName;

            terrName = "";
            weight = 1;
            lyr = 9;
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
        _arrayOfTerrains = container.GetComponents<TerrInfo>();

        Vector3 initPos = _pivot.transform.position;

        for (int i = 0; i < filas + 1; i++)
        {
            Debug.Log("Entro en el for de filas, i = " + i);
            RaycastHit firstHit;
            if (Physics.Raycast(_pivot.transform.position, -_pivot.transform.up, out firstHit, y + 4f))
            {
                for (int k = 0; k < _arrayOfTerrains.Length; k++)
                {
                  if (firstHit.collider.gameObject.layer == _arrayOfTerrains[k].terrlyr)
                        if(_arrayOfTerrains[k].terrweight != 0)
                           NodeCreator(firstHit.point, _arrayOfTerrains[k].terrweight);
                }

            }

            for (int j = 0; j < columnas; j++)
            {
                Debug.Log("Entro en el for de columnas. J = " + j);
                _pivot.transform.position = new Vector3(_pivot.transform.position.x + _disX, _pivot.transform.position.y, _pivot.transform.position.z);
                RaycastHit hit;
                if (Physics.Raycast(_pivot.transform.position, -_pivot.transform.up, out hit, 11f))
                {
                    for (int l = 0; l < _arrayOfTerrains.Length; l++)
                    {
                        if (hit.collider.gameObject.layer == _arrayOfTerrains[l].terrlyr)
                            if (_arrayOfTerrains[l].terrweight != 0)
                                NodeCreator(hit.point, _arrayOfTerrains[l].terrweight);
                    }
                }
            }

            }

            _pivot.transform.position = new Vector3(initPos.x, initPos.y, initPos.z + _disZ);
            initPos = _pivot.transform.position;


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
