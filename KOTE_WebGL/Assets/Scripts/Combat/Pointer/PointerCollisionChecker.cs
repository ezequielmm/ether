using UnityEngine;
using System.Linq;

public class PointerCollisionChecker : MonoBehaviour
{
    public PointerManager pointerManager;

    [Range(0f, 8f)]
    public float OutlineWidth = 8;
    public Color OutlineColor = Color.yellow;

    private Shader outlineShader;
    private Shader defaultShader;

    private TargetProfile targetProfile => pointerManager.TargetProfile;

    private bool OverTarget
    {
        get => pointerManager.overTarget;
        set => pointerManager.overTarget = value;
    }

    private string targetID
    {
        get => pointerManager.targetID;
        set => pointerManager.targetID = value;
    }

    [SerializeField]
    GameObject lastOver;
    new Renderer renderer;

    private void Awake()
    {
        outlineShader = Shader.Find("Spine/Outline/Skeleton");
        defaultShader = Shader.Find("Spine/Skeleton");
    }

    private void Highlight(GameObject obj)
    {
        renderer = obj.GetComponentInChildren<Renderer>();
        foreach (var material in renderer.materials)
        {
            UpdateShader(material, outlineShader, OutlineWidth, OutlineColor);
        }
    }

    private void RemoveHighlight(GameObject obj)
    {
        foreach (var material in renderer.materials)
        {
            UpdateShader(material, defaultShader, 0, new Color(0, 0, 0, 0));
        }

        renderer = null;
    }

    private void Enter(GameObject obj)
    {
        //Debug.Log("[Pointer] Enemy Enter");
        

        targetID = GetID(obj);

        if (targetID == null)
        {
            return;
        }

        if (!IsAllowedEntity(obj.tag, targetID)) 
        {
            targetID = null;
            return;
        }

        lastOver = obj;
        OverTarget = true;

        // highlight
        Highlight(obj);
    }

    private void Exit(GameObject obj)
    {
        //Debug.Log("[Pointer] Enemy Exit");
        OverTarget = false;
        targetID = null;

        RemoveHighlight(obj);
        lastOver = null;
    }

    private bool IsAllowedEntity(string tag, string id) 
    {
        if (targetProfile.specificEntities.Contains(id)) 
        {
            return true;
        }
        if (tag == "Player" && targetProfile.player)
        {
            return true;
        }
        if(tag == "Enemy" && targetProfile.enemy) 
        {
            return true;
        }
        return false;
    }

    private string GetID(GameObject other) 
    {
        string id = null;
        if (other.tag == "Enemy") 
        {
            var enemy = other.GetComponentInParent<EnemyManager>();
            if (enemy != null) 
            {
                id = enemy.EnemyData.id;
            }
        }
        if (other.tag == "Player") 
        {
            var player = other.GetComponentInParent<PlayerManager>();
            if (player != null)
            {
                // TODO: Change to string based ID
                id = $"{player.PlayerData.id}";
            }
        }
        return id;
    }

    private void UpdateShader(Material material, Shader shader, float width, Color color) 
    {
        material.shader = shader;
        material.SetFloat("_OutlineWidth", width);
        material.SetColor("_OutlineColor", color);
    }

    private void LateUpdate()
    {
        if (renderer != null) 
        {
            foreach (var material in renderer.materials)
            {
                if (material.shader != outlineShader)
                {
                    UpdateShader(material, outlineShader, OutlineWidth, OutlineColor);
                }
            }
        }
    }

    public void EvaluateCursor(GameObject entity, bool isOver)
    {
        if (isOver)
            Enter(entity);
        else
            Exit(entity);
        // if (OverTarget != isOver)
        // {
        //     if (isOver)
        //     {
        //         Enter(entity);
        //     }
        //     else 
        //     {
        //         if (lastOver != null)
        //         {
        //             Exit(lastOver);
        //         }
        //     }
        // } 
        // else if (isOver && entity != lastOver) 
        // {
        //     if (lastOver != null)
        //     {
        //         Exit(lastOver);
        //     }
        //     Enter(entity);
        // }
    }
}
