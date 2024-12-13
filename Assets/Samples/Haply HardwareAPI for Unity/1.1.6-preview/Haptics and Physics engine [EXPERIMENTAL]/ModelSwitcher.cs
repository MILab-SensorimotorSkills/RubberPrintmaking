// using UnityEngine;
// using Unity.MLAgents.Policies;
// using Unity.Barracuda;

// public class ModelSwitcher : MonoBehaviour
// {
//     [SerializeField] private BehaviorParameters behaviorParameters;
//     [SerializeField] private NNModel goodPerformanceModel;
//     [SerializeField] private NNModel badPerformanceModel;

//     [SerializeField] private AdvancedPhysicsHapticEffector hapticEffector;
//     [SerializeField] private float distanceThreshold = 0.3f;

//     private void Update()
//     {
//         if (hapticEffector == null || behaviorParameters == null)
//         {
//             Debug.LogWarning("Haptic Effector or Behavior Parameters is not assigned.");
//             return;
//         }

//         // hapticEffector.distance_2d 값을 기반으로 모델 전환
//         if (hapticEffector.distance_2d < distanceThreshold)
//         {
//             if (behaviorParameters.Model != goodPerformanceModel)
//             {
//                 behaviorParameters.Model = goodPerformanceModel;
//                 Debug.Log("Switched to Good Performance Model.");
//             }
//         }
//         else
//         {
//             if (behaviorParameters.Model != badPerformanceModel)
//             {
//                 behaviorParameters.Model = badPerformanceModel;
//                 Debug.Log("Switched to Bad Performance Model.");
//             }
//         }
//     }
// }

using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

public class ModelSwitcher : MonoBehaviour
{
    [SerializeField] private BehaviorParameters behaviorParameters;
    [SerializeField] private NNModel HardModel;
    [SerializeField] private NNModel EasyModel;

    [SerializeField] private AdvancedPhysicsHapticEffector hapticEffector;
    [SerializeField] private float distanceThreshold = 0.3f;
    [SerializeField] private int thresholdFrames = 10000; // 조건이 충족되어야 하는 프레임 수

    private int frameCount = 0; // 조건 충족 프레임 카운터
    private bool isThresholdMet = false;

    private void Update()
    {
        if (hapticEffector == null || behaviorParameters == null)
        {
            Debug.LogWarning("Haptic Effector or Behavior Parameters is not assigned.");
            return;
        }

        // 현재 distance_2d 조건이 threshold를 충족하는지 확인
        bool currentThresholdMet = hapticEffector.distance_2d < distanceThreshold;

        if (currentThresholdMet)
        {
            // 조건이 충족된 프레임 수 증가
            frameCount++;

            // 조건 충족 프레임이 thresholdFrames를 초과하면 모델 전환
            if (frameCount >= thresholdFrames)
            {
                if (behaviorParameters.Model != HardModel)
                {
                    behaviorParameters.Model = HardModel;
                    Debug.Log("Switched to Hard Model");
                }
            }
        }
        else
        {
            // 조건이 충족되지 않으면 프레임 카운터 초기화
            frameCount = 0;

            // Bad Performance Model로 전환
            if (behaviorParameters.Model != EasyModel)
            {
                behaviorParameters.Model = EasyModel;
                Debug.Log("Switched to Easy Model.");
            }
        }
    }
}
