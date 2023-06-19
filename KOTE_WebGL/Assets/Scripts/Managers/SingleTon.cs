using UnityEngine;

public abstract class SingleTon<T> : MonoBehaviour, ISingleton<T> where T : Component
{
    /// <summary>
    /// The instance.
    /// </summary>
    protected static T instance;

    //TODO this needs to be set dynamically when a gameobject is created for the instance
    private bool createdGameobject = true;


    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                var findInstanceInScene = FindObjectOfType<T>();
                if (findInstanceInScene)
                {
                    instance = findInstanceInScene;
                    DontDestroyOnLoad(instance.gameObject);
                }
                else
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            
            return instance;
        }
    }

    public virtual void Init()
    {
        
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            DestroyInstance();
        }
    }

    public void DestroyInstance()
    {
        if (createdGameobject)
        {
            Destroy(instance.gameObject);
        }
        else
        {
            Destroy(instance);
        }
        instance = null;
    }

    /// <summary>
    /// checks if an instance of this object has been created
    /// </summary>
    /// <returns></returns>
    protected static bool DoesInstanceExist()
    {
        return instance != null;
    }

    /// <summary>
    /// Use this for initialization.
    /// </summary>
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(instance);
        }
        else if (createdGameobject)
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(instance);
        }
    }
}