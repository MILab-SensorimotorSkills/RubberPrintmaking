using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiggingTest;

public class VirtualKnife : MonoBehaviour
{
    public GameObject virtualObject;
    private AdvancedPhysicsHapticEffector advancedHapticEffector;

    private Vector3 localPosition;
    private Shovel knifeShovel;

    // Start is called before the first frame update
    void Start()
    {
        advancedHapticEffector = GetComponent<AdvancedPhysicsHapticEffector>();
        knifeShovel = GetComponent<Shovel>();

        if (virtualObject != null)
        {
            localPosition = virtualObject.transform.localPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (virtualObject != null)
        {
            float mainForce = advancedHapticEffector.MainForce;
            //Debug.Log($"MainForce: {mainForce}");

            float yPosition = localPosition.y; // 기본 y축 위치
        List<Vector3> colliderWorldPositions = advancedHapticEffector.GetColliderWorldPositions();
        foreach (Vector3 position in colliderWorldPositions)
        {
            Debug.Log($"Collider World Position: {position}");
        }
            if (mainForce > 0.24f)
            {
                yPosition = Mathf.Lerp(localPosition.y, -1.25f, Mathf.Clamp(mainForce / 20.0f, 0, 1));
            }

            virtualObject.transform.localPosition = new Vector3(
                localPosition.x,
                yPosition,
                localPosition.z
            );
            
        }
    }
}
