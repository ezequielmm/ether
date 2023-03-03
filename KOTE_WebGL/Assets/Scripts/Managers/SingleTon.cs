using UnityEngine;

public abstract class SingleTon<T> : MonoBehaviour, ISingleton<T> where T : Component
{
    #region Fields

    /// <summary>
    /// The instance.
    /// </summary>
    private static T instance;

    //TODO this needs to be set dynamically when a gameobject is created for the instance
    private bool createdGO = true;

    #endregion

    #region Properties

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
                GameObject obj = new GameObject();
                obj.name = typeof(T).Name;
                instance = obj.AddComponent<T>();

                DontDestroyOnLoad(instance.gameObject);
            }

            return instance;
        }
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
        if (createdGO)
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

    #endregion

    #region Methods

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
        else if (createdGO)
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(instance);
        }
    }

    #endregion
}