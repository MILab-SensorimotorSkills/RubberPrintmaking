using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

public class ModelSwitcher : MonoBehaviour
{
    [SerializeField] private BehaviorParameters behaviorParameters;
    [SerializeField] private NNModel goodPerformanceModel;
    [SerializeField] private NNModel badPerformanceModel;

    [SerializeField] private AdvancedPhysicsHapticEffector hapticEffector;
    [SerializeField] private float distanceThreshold = 0.3f;

    private void Update()
    {
        if (hapticEffector == null || behaviorParameters == null)
        {
            Debug.LogWarning("Haptic Effector or Behavior Parameters is not assigned.");
            return;
        }

        // hapticEffector.distance_2d 값을 기반으로 모델 전환
        if (hapticEffector.distance_2d < distanceThreshold)
        {
            if (behaviorParameters.Model != goodPerformanceModel)
            {
                behaviorParameters.Model = goodPerformanceModel;
                Debug.Log("Switched to Good Performance Model.");
            }
        }
        else
        {
            if (behaviorParameters.Model != badPerformanceModel)
            {
                behaviorParameters.Model = badPerformanceModel;
                Debug.Log("Switched to Bad Performance Model.");
            }
        }
    }
}
