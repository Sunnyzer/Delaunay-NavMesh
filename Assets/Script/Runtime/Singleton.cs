using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T instance;
    public static T Instance => instance;
    private void Awake()
    {
        if(instance)
        {
            Destroy(this);
            return;
        }
        instance = this as T;
    }
}
