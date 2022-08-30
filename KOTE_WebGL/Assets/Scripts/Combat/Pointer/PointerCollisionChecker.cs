using UnityEngine;

public class PointerCollisionChecker : MonoBehaviour
{
    public PointerManager pointerManager;

    [Range(0f, 8f)]
    public float OutlineWidth = 8;
    public Color OutlineColor = Color.yellow;

    private Shader outlineShader;
    private Shader defaultShader;

    private bool OverEnemy 
    {
        get => pointerManager.overEnemy;
        set => pointerManager.overEnemy = value;
    }

    [SerializeField]
    GameObject lastOver;
    new Renderer renderer;

    private void Awake()
    {
        outlineShader = Shader.Find("Spine/Outline/Skeleton");
        defaultShader = Shader.Find("Spine/Skeleton");
    }

    private void Enter(GameObject obj) 
    {
        //Debug.Log("[Pointer] Enemy Enter");
        lastOver = obj;
        var other = obj.GetComponentInParent<EnemyManager>();

        OverEnemy = true;
        pointerManager.enemyData = other.EnemyData;

        renderer = obj.GetComponent<Renderer>();
        foreach (var material in renderer.materials)
        {
            UpdateShader(material, outlineShader, OutlineWidth, OutlineColor);
        }
    }

    private void Exit(GameObject obj) 
    {
        //Debug.Log("[Pointer] Enemy Exit");
        OverEnemy = false;
        foreach (var material in renderer.materials)
        {
            UpdateShader(material, defaultShader, 0, new Color(0, 0, 0, 0));
        }

        renderer = null;
        lastOver = null;
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
        if (collision != null && collision.gameObject.CompareTag("Enemy")) 
        {
            var other = collision.gameObject.GetComponentInParent<EnemyManager>();
            if (other != null)
                isOver = true;
        }
        if (OverEnemy != isOver)
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
        } else if (isOver && collision.gameObject != lastOver) 
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
