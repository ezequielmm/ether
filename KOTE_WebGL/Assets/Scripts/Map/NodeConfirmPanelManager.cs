using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NodeConfirmPanelManager : MonoBehaviour
{
    [Tooltip("The container for the node selection panel")]
    public GameObject nodeConfirmContainer;
    [Tooltip("The title text for the confirmation panel")]
    public TMP_Text titleText;
    [Tooltip("The description text for the confirmation panel")]
    public TMP_Text descriptionText;

    // store the node data so we can pass it on if the user confirms
    private int nodeId;
    private void Start()
    {
        GameManager.Instance.EVENT_MAP_REQUEST_NODE_CONFIRMATION.AddListener(ShowNodeSelectionRequest);
    }

    private void ShowNodeSelectionRequest(NodeData nodeData)
    {
        titleText.text = Utils.CapitalizeEveryWordOfEnum(nodeData.type);
        descriptionText.text = " Do you want to select " + Utils.CapitalizeEveryWordOfEnum(nodeData.subType) + "?";
        nodeId = nodeData.id;
        nodeConfirmContainer.SetActive(true);
    }

    public void ConfirmSelection()
    {
        nodeConfirmContainer.SetActive(false);
        GameManager.Instance.EVENT_MAP_ACTIVATE_PORTAL.Invoke(nodeId);
    }

    public void GoBack()
    {
        nodeConfirmContainer.SetActive(false);
    }
}
