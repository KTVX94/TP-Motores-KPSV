using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class Node : MonoBehaviour {

    public List<Node> vecinos;
    private Pivot pivot;
    public float radious;
    public float baseWeight;
    public float weightModifier;
    public float finalWeight;

	// Use this for initialization
	void Start () {
        pivot = FindObjectOfType<Pivot>();
        vecinos = new List<Node>();
        if (baseWeight == 0) baseWeight = 1;
        finalWeight = baseWeight * weightModifier;

        //radious = (pivot.columnas + pivot.filas) / pivot.filas;
        //radious = pivot.disZ / 2;
        //radious = 0.5f;

        

        //ReSearch();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SearchLinks()
    {
        var aux = Physics.OverlapSphere(transform.position, radious);
        for (int i = 0; i < aux.Length; i++)
        {
            if (aux[i].GetComponent<Node>() && aux[i] != this)
            {
                vecinos.Add(aux[i].GetComponent<Node>());
            }
        }
    }

    /*void ReSearch()
    {
        vecinos.Clear();
        var aux = Physics.OverlapSphere(transform.position, radious);
        for (int i = 0; i < aux.Length; i++)
        {
            if(aux[i].GetComponent<Node>() && aux[i] != this)
            {
                vecinos.Add(aux[i].GetComponent<Node>());
            }
        }
    }*/

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(transform.position, new Vector3(0.1f, 0.1f, 0.1f));

        if(vecinos != null)
        {
            //Debug.Log("dsdsd");
            for (int i = 0; i < vecinos.Count; i++)
            {
                Gizmos.DrawLine(transform.position, vecinos[i].transform.position);
                //Gizmos.DrawWireSphere(vecinos[i].transform.position, 1);
            }
        }


        //Gizmos.DrawWireSphere(transform.position, radious);
    }
}
