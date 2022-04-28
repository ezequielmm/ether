using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapSpriteManager : MonoBehaviour
{
    public GameObject mapContainer;

    public GameObject nodePrefab;
    float h_offset = 350f;
    float v_offset = 100;

    List<GameObject> nodes = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.EVENT_MAP_NODES_UPDATE.AddListener(OnMapNodesDataUpdated);
        GameManager.Instance.EVENT_NODE_DATA_UPDATE.AddListener(OnNodeDataUpdated);
        GameManager.Instance.EVENT_MAP_ICON_CLICKED.AddListener(OnMapIconClicked);

    }

    private void OnMapIconClicked()
    {
        //make sure when the map is on panel mode the nodes are not clickable
        foreach (GameObject go in nodes)
        {
            go.GetComponent<NodeData>().disabled = true;
        }

        if (mapContainer.activeSelf)
        {
            mapContainer.SetActive(false);
            GameManager.Instance.EVENT_MAP_PANEL_TOOGLE.Invoke(false);
        }
        else
        {
            mapContainer.SetActive(true);
            GameManager.Instance.EVENT_MAP_PANEL_TOOGLE.Invoke(true);
        }


    }

    private void OnNodeDataUpdated(NodeStateData nodeState, bool initialCall)
    {
       if(initialCall) mapContainer.SetActive(false);
    }

    // Update is called once per frame
    void OnMapNodesDataUpdated(string data)
    {
        Debug.Log("[OnMapNodesDataUpdated] " + data);

        //ExpeditionMapData expeditionMapData = JsonUtility.FromJson<ExpeditionMapData>("{\"data\":" + data + "}");
        ExpeditionMapData expeditionMapData = JsonUtility.FromJson<ExpeditionMapData>(data);

        MapStructure mapStructure = new MapStructure();

        //parse the nodes data
        for (int i = 0; i < expeditionMapData.data.Length; i++)
        {
            NodeDataHelper nodeData = expeditionMapData.data[i];

            //acts
            if (mapStructure.acts.Count < (nodeData.act + 1))
            {
                Act newAct = new Act();
                mapStructure.acts.Add(newAct);

            }
            //steps
            if (mapStructure.acts[nodeData.act].steps.Count < (nodeData.step + 1))
            {
                Step newStep = new Step();
                mapStructure.acts[nodeData.act].steps.Add(newStep);
            }
            //add id
            mapStructure.acts[nodeData.act].steps[nodeData.step].nodesData.Add(nodeData);

        }

        Debug.Log(mapStructure);

        float actOffset = 5f;
        float columnOffset = 3.5f;

        //map holder is on -1 (y center) so top is +3 and bottom is -5

        float mapTopEdge = 3;
        float mapBottomEdge = -5;

        //generate the map
        foreach (Act act in mapStructure.acts)
        {
            //areas
            foreach (Step step in act.steps)
            {
                //columns
                float rows = step.nodesData.Count;
                float rowsMaxSpace = 8 / rows;
                //Debug.Log("rowsMaxSpace:" + rowsMaxSpace);                

                foreach (NodeDataHelper nodeData in step.nodesData)
                {

                    float yy = (rowsMaxSpace * step.nodesData.IndexOf(nodeData)) - ((rows - 1) * rowsMaxSpace) / 2;

                    // Debug.Log("act:"+mapStructure.acts.IndexOf(act)+",step:"+act.steps.IndexOf(step)+",node:"+step.nodesData.IndexOf(nodeData).ToString()+",yy:"+yy+",rowspace:"+rowsMaxSpace);

                    //nodes
                    GameObject newNode = Instantiate(nodePrefab, mapContainer.transform);
                    nodes.Add(newNode);
                    newNode.GetComponentInChildren<TextMeshPro>().SetText(nodeData.id.ToString());
                    newNode.transform.localPosition = new Vector3(columnOffset, yy, GameSettings.MAP_SPRITE_ELEMENTS_Z);
                    newNode.GetComponent<NodeData>().id = nodeData.id;
                    newNode.GetComponent<NodeData>().type = nodeData.type;
                    newNode.GetComponent<NodeData>().exits = nodeData.exits;
                }

                columnOffset += 3;
            }
        }

        foreach (GameObject go in nodes)
        {
            //Debug.Log("Searching :" + go.GetComponent<NodeData>().id);

            foreach (int exit_id in go.GetComponent<NodeData>().exits)
            {
                GameObject targetOb = nodes.Find(x => x.GetComponent<NodeData>().id == exit_id);


                if (targetOb)
                {
                    go.GetComponent<NodeData>().UpdateLine(targetOb);
                }
                else
                {
                    Destroy(go.GetComponent<LineRenderer>());
                }
            }
        }
    }
}
