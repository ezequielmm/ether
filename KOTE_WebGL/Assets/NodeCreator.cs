using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NodeCreator : MonoBehaviour
{
    public GameObject nodePrefab;
    float h_offset = 350f;
    float v_offset = 100;

    List<GameObject> nodes = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_MAP_NODES_UPDATE.AddListener(OnMapNodesDataUpdated);
    }

    // Update is called once per frame
    void OnMapNodesDataUpdated(string data)
    {
        ExpeditionMapData expeditionMapData = JsonUtility.FromJson<ExpeditionMapData>(data);

        for (int i = 0; i < expeditionMapData.rows.Length; i++)
        {
            // Debug.Log("expeditionMapData.rows[i].nodes.Length:" + expeditionMapData.rows[i].nodes.Length);
            for (int j = 0; j < expeditionMapData.rows[i].nodes.Length; j++)
            {
                h_offset = UnityEngine.Random.Range(325f,360f);
                v_offset = UnityEngine.Random.Range(100f,120f);
                Debug.Log("node id:" + expeditionMapData.rows[i].nodes[j].id);
                GameObject newNode = Instantiate(nodePrefab, this.transform);
                nodes.Add(newNode);
                newNode.GetComponentInChildren<TextMeshProUGUI>().SetText(expeditionMapData.rows[i].nodes[j].id.ToString());
                newNode.transform.localPosition = new Vector3(h_offset * (i+1), (v_offset * -j) + ((expeditionMapData.rows[i].nodes.Length - 1) * v_offset) / 2, 0);
               
                newNode.GetComponent<NodeData>().id = expeditionMapData.rows[i].nodes[j].id;
                newNode.GetComponent<NodeData>().type = expeditionMapData.rows[i].nodes[j].type;

                //populate exit node ids
                if (expeditionMapData.rows[i].nodes[j].exit_nodes != null)
                {
                    for (int k = 0; k < expeditionMapData.rows[i].nodes[j].exit_nodes.Length; k++)
                    {
                        newNode.GetComponent<NodeData>().exitNodesIds.Add(expeditionMapData.rows[i].nodes[j].exit_nodes[k].exit_nodeid);
                    }
                }
                else
                {
                    Destroy(newNode.GetComponent<LineRenderer>());
                }
                
            }

        }
        //lines

        foreach (GameObject go in nodes)
        {
            Debug.Log("Searching :"+ go.GetComponent<NodeData>().id);

            foreach (int exit_id in go.GetComponent<NodeData>().exitNodesIds )
            {
                GameObject targetOb = nodes.Find(x => x.GetComponent<NodeData>().id == exit_id );


                if (targetOb)
                {
                    go.GetComponent<NodeData>().UpdateLine(targetOb);
                }
                else
                {
                    Destroy(go.GetComponent<LineRenderer>());
                }
            }
           /* GameObject targetOb = nodes.Find(x => x.GetComponent<NodeData>().id ==  );

            if (targetOb) {
                go.GetComponent<NodeData>().UpdateLine(targetOb);            
            }*/
        }
      /*  for (int i = 0; i < expeditionMapData.rows.Length; i++)
        {
            // Debug.Log("expeditionMapData.rows[i].nodes.Length:" + expeditionMapData.rows[i].nodes.Length);
            for (int j = 0; j < expeditionMapData.rows[i].nodes.Length; j++)
            {
                for ( int k = 0; k < expeditionMapData.rows[i].nodes[j].exit_nodes.Length; k++)
                {
                    GameObject go = nodes.Find(x => x.GetComponent<NodeData>().id == expeditionMapData.rows[i].nodes[j].exit_nodes[k].exit_nodeid);

                    if (go) { expeditionMapData.rows[i].nodes[j]. }
                }
            }
        }*/
    }
}
