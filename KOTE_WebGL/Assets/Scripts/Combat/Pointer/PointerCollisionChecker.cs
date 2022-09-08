using UnityEngine;

public class PointerCollisionChecker : MonoBehaviour
{
    public PointerManager pointerManager;

    [Range(0f, 8f)]
    public float OutlineWidth = 8;
    public Color OutlineColor = Color.yellow;

    private Shader outlineShader;
    private Shader defaultShader;

    private bool targetPlayer => pointerManager.targetPlayer;
    private bool targetEnemy => pointerManager.targetEnemy;

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
        renderer = obj.GetComponent<Renderer>();
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
        lastOver = obj;

        targetID = GetID(obj);

        if (targetID == null)
            return;

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

    private string GetID(GameObject other) 
    {
        string id = null;
        if (targetEnemy) 
        {
            var enemy = other.GetComponentInParent<EnemyManager>();
            if (enemy != null) 
            {
                id = enemy.EnemyData.id;
            }
        }
        if (targetPlayer) 
        {
            var player = other.GetComponentInParent<PlayerManager>();
            if (player != null)
            {
                // TODO: Change to string based ID
                id = $"{player.PlayerData.playerId}";
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

    private void OnTriggerStay2D(Collider2D collision)
    {
        bool isOver = false;
        if (collision != null && targetEnemy && collision.gameObject.CompareTag("Enemy"))
        {
            var other = collision.gameObject.GetComponentInParent<EnemyManager>();
            if (other != null)
                isOver = true;
        } 
        else if (collision != null && targetPlayer && collision.gameObject.CompareTag("Player"))
        {
            var other = collision.gameObject.GetComponentInParent<PlayerManager>();
            if (other != null)
                isOver = true;
        }

        if (OverTarget != isOver)
        {
            if (isOver)
            {
                Enter(collision.gameObject);
            }
            else 
            {
                if (lastOver != null)
                {
                    Exit(lastOver);
                }
            }
        } 
        else if (isOver && collision.gameObject != lastOver) 
        {
            if (lastOver != null)
            {
                Exit(lastOver);
            }
            Enter(collision.gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        OnTriggerStay2D(null);
    }
}
