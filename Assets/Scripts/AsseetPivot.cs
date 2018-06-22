using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsseetPivot : MonoBehaviour {
    [HideInInspector]
    public GameObject asset;
    [HideInInspector]
    public int counter;

    GameObject empty;

    [HideInInspector]
    public Vector3 circleHandlePos;
    [HideInInspector]
    public Vector3 circleHandleNormal;
    public Transform sprite;
    /* [HideInInspector]
     public Vector3 circlePos;
     [HideInInspector]
     public Vector3 cirlceNormal;*/

    List<GameObject> first;
    List<GameObject> second;

    public IEnumerator DeleteMode()
    {
        while(true)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -this.transform.up, out hit, 40f))
            {
                //Debug.Log(hit.collider.gameObject.name);
                if(hit.collider.gameObject.layer == 17)
                {
                    DestroyImmediate(hit.collider.gameObject);     
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    public IEnumerator DeleteModeRadious(float radious)
    {
        while (true)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -this.transform.up, out hit, 40f))
            {
                var objs = Physics.OverlapSphere(hit.point, radious);
                
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    public IEnumerator UpDateOnGUI()
    {
        while(true)
        {
            var mousePos = Event.current.mousePosition;
            transform.position += new Vector3(mousePos.x, 0, 0);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator GetCircleHandlePos()
    {
        while(true)
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, -this.transform.up, out hit, 40f))
            {
                if(hit.collider.gameObject.layer == 18)
                {
                    circleHandlePos = hit.point;
                    circleHandleNormal = hit.normal;
                    Debug.DrawLine(this.transform.position, hit.point);
                    //Debug.DrawLine(hit.point, hit.normal.normalized);
                    //sprite.position = hit.point;
                    //sprite.up = hit.normal.normalized;


                }
                else
                {
                    Debug.DrawLine(transform.position, transform.position - transform.up * 10);
                }
               
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    public IEnumerator Automatic()
    {
        while (true)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -this.transform.up, out hit, 40f))
            {
                if (hit.collider.gameObject.layer == 18)
                {
                    var a = Instantiate(asset);
                    a.transform.position = hit.point;
                    a.transform.position += new Vector3(0, a.GetComponent<Collider>().bounds.size.y / 2, 0);
                    a.gameObject.layer = 17;
                    counter++;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

    }

    public void CreateAsset(int quantity, float radious, bool separation)
    {
        if(asset != null)
        {
            if(quantity > 1)
            {
                if(!separation)
                {
                    RaycastHit hitNC;
                    if (Physics.Raycast(transform.position, -this.transform.up, out hitNC, 40f))
                    {
                        if (hitNC.collider.gameObject.layer == 18)
                        {
                            for (int i = 0; i < quantity; i++)
                            {  
                                var a = Instantiate(asset);
                                a.transform.position = hitNC.point;
                                a.transform.position += new Vector3(Random.Range(-radious + 0.5f, radious - 0.5f), a.GetComponent<Collider>().bounds.size.y / 2, Random.Range(-radious + 0.5f, radious- 0.5f));
                                a.name = a.name + i.ToString();
                                first.Add(a);

                                RaycastHit hit2;
                                if(Physics.Raycast(a.transform.position, - a.transform.up, out hit2, 40f))
                                {
                                    if(hit2.collider.gameObject.layer == 18)
                                    {
                                        a.gameObject.layer = 17;
                                        Debug.Log("soy: " + a.name + "y colicione");
                                        second.Add(a);
                                        a.transform.position = new Vector3(hit2.point.x, hit2.point.y + a.GetComponent<Collider>().bounds.size.y / 2, hit2.point.z);
                                        a.transform.up = hit2.normal;
                                    }
                                    
                                }
                            }
                        }
                    }
                }else
                {
                    RaycastHit hit1;
                    if (Physics.Raycast(transform.position, -this.transform.up, out hit1, 40f))
                    {
                        if (hit1.collider.gameObject.layer == 18)
                        {
                            empty = new GameObject();
                            empty.transform.position = hit1.point;
                            var angDist = 360 / quantity;
                            for (int i = 0; i < quantity; i++)
                            {
                                Vector3 rightLimit = Quaternion.AngleAxis(angDist, empty.transform.up) * empty.transform.forward;
                                rightLimit = empty.transform.position + (rightLimit * radious);
                                var a = Instantiate(asset);
                                a.transform.position = rightLimit;
                                a.transform.position += new Vector3(0, a.GetComponent<Collider>().bounds.size.y / 2, 0);
                                //a.gameObject.layer = 17;
                                angDist = angDist + (360 / quantity);
                                a.gameObject.layer = 17;
                            }
                            DestroyImmediate(empty);
                        }
                    }
                }

                
            }else
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, -this.transform.up, out hit, 40f))
                {
                    if (hit.collider.gameObject.layer == 18)
                    {
                        for (int i = 0; i < quantity; i++)
                        {
                            var a = Instantiate(asset);
                            a.transform.position = hit.point;
                            a.transform.position += new Vector3(0, a.GetComponent<Collider>().bounds.size.y / 2, 0);
                            a.gameObject.layer = 17;
                        }

                    }
                }
            }
            
        }
    }

    
}
