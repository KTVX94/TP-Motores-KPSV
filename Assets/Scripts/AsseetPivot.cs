using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsseetPivot : MonoBehaviour {
    [HideInInspector]
    public GameObject asset;
    [HideInInspector]
    public int counter;

    GameObject empty;

    public IEnumerator DeleteMode()
    {
        while(true)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -this.transform.up, out hit, 40f))
            {
                Debug.Log(hit.collider.gameObject.name);
                if(hit.collider.gameObject.layer == 17)
                {
                    DestroyImmediate(hit.collider.gameObject);     
                }
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
                                a.transform.position += new Vector3(Random.Range(-radious, radious), a.GetComponent<Collider>().bounds.size.y / 2, Random.Range(-radious, radious));
                                a.gameObject.layer = 17;
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
