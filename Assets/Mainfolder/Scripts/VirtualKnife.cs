using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualKnife : MonoBehaviour
{
    public GameObject virtualObject;
    private AdvancedPhysicsHapticEffector advancedHapticEffector;

    private Vector3 localPosition;

    // Start is called before the first frame update
    void Start()
    {
        advancedHapticEffector = GetComponent<AdvancedPhysicsHapticEffector>();
    }

    // Update is called once per frame
    void Update()
    {
        if (virtualObject != null)
        {
            float mainForce = advancedHapticEffector.MainForce;
            Debug.Log($"MainForce: {mainForce}");

            float zLength = 0.001f; // 기본 Z축 길이
            if (mainForce > 0.24f)
            {
 zLength = Mathf.Clamp(mainForce / 20.0f, 0, 1) * 0.034f;
            }

            virtualObject.transform.localPosition = new Vector3(
                localPosition.x,
                localPosition.y,
                zLength
            );
        }
    }
}
