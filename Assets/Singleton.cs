using UnityEngine;

class Singleton<T> : MonoBehaviour
{
    public static T Instance { get; protected set; }

    protected void SingletonInit(T self) {
        Instance = self;
    }
}

