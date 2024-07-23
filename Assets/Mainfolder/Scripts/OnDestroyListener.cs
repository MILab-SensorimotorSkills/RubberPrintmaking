using UnityEngine;
using System;

public class OnDestroyListener : MonoBehaviour
{
    public event Action OnDestroyEvent;

    private void OnDestroy()
    {
        OnDestroyEvent?.Invoke();
    }
}
