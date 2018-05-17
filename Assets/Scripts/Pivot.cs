using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pivot : MonoBehaviour {

    public GameObject target;
    public GameObject Node;
    public GameObject _nodes;

    public int filas;
    public int columnas;

    float x;
    float y;
    float z;

    [HideInInspector]
    public float disX;
    [HideInInspector]
    public float disZ;

    bool toDo;

    List<GameObject> Nodes;

	// Use this for initialization
	void Start () {
        Nodes = new List<GameObject>();

        x = target.GetComponent<MeshRenderer>().bounds.size.x / 2 - 0.1f;
        y = target.GetComponent<MeshRenderer>().bounds.size.y;
        z = target.GetComponent<MeshRenderer>().bounds.size.z / 2 - 0.1f;

        disX = (x * 2) / columnas;
        disZ = (z * 2) / filas;
        transform.position = target.transform.position;
        transform.position = new Vector3(transform.position.x - x, y + 3, transform.position.z - z);

        //Debug.Log(disX);
        //Debug.Log(disZ);

        toDo = true;
    }
	
	// Update is called once per frame
	void Update () 
    {
        Maper();
	}

    void Maper() 
    {
        Vector3 initPos = transform.position;

        
        if (toDo)
        {
            for (int i = 0; i < filas + 1; i++)
            {
                //CreatNode(transform);
                RaycastHit firstHit;
                if (Physics.Raycast(transform.position,- this.transform.up, out firstHit, y + 4f))
                {
                    if (firstHit.collider.gameObject.layer == 18)
                    {
                        //Debug.Log(firstHit.point);
                        CreatNode(firstHit.point);
                    }
                }

                for (int j = 0; j < columnas; j++)
                {
                    transform.position = new Vector3(transform.position.x + disX, transform.position.y, transform.position.z);
                    //CreatNode(transform);
                    //ThrowRayCast(transform);
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position,- this.transform.up, out hit, 11f))
                    {
                        //Debug.Log("tiro el ray");
                        if(hit.collider.gameObject.layer == 18)
                        {
                            //Debug.Log(hit.transform.position);
                            CreatNode(hit.point);
                        }
                    }

                }

                transform.position = new Vector3(initPos.x, initPos.y, initPos.z + disZ);
                initPos = transform.position;
            }


            foreach (var item in Nodes)
            {
                if(item.activeSelf == false)
                {
                    item.SetActive(true);
                }
            }

            toDo = false;
        }

        
        
    }


    void CreatNode(Vector3 trans)
    {
        var node = Instantiate(Node);
        node.transform.position = trans;
        Nodes.Add(node);
        node.transform.SetParent(_nodes.transform);
        node.SetActive(false);
    }

    /*void ThrowRayCast(Transform trans)
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.position - this.transform.up * 5, out hit, 5f))
        {
            if(hit.collider.gameObject.layer == 18)
            {
                CreatNode(hit.transform);
            }
        }



    }*/

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        var a = target.GetComponent<MeshRenderer>().bounds.size;
        Gizmos.DrawWireCube(target.transform.position, a);

        if(Nodes != null)
        {
            foreach (var item in Nodes)
            {
                Gizmos.DrawWireCube(item.transform.position, new Vector3(0.1f, 0.1f, 0.1f));
                //Gizmos.DrawLine(item.transform.position, item.transform.position - item.transform.up * 5);
                //Gizmos.DrawIcon(item.transform.position, item.name+ "1");
               
            }
        }

        //Gizmos.DrawLine(transform.position, transform.position - this.transform.up * 5);
        

    }
    

}
