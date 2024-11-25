// using Unity.MLAgents;
// using Unity.MLAgents.Actuators;
// using Unity.MLAgents.Sensors;
// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// public class FeedbackAgent : Agent
// {
//     [SerializeField] private AdvancedPhysicsHapticEffector hapticEffector;
//     [SerializeField] private SphereCollider Sphere;

//     public override void Initialize()
//     {
//         // 초기화 코드
//     }

//     public override void CollectObservations(VectorSensor sensor)
//     {
//         // MainForce와 distance_2d를 관측값으로 추가
//         sensor.AddObservation(hapticEffector.MainForce);
//         sensor.AddObservation(hapticEffector.distance_2d);
//     }

//     public override void OnActionReceived(ActionBuffers actionBuffers)
//     {
//         // 행동 값(0: Default, 1: Disturbance, 2: Guidance) 기반으로 ForceFeedbackType 설정
//         int action = actionBuffers.DiscreteActions[0];
//         switch (action)
//         {
//             case 1:
//                 hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Disturbance;
//                 break;
//             case 2:
//                 hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Guidance;
//                 break;
//             default:
//                 hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Default;
//                 break;
//         }

//         // 성과에 따라 보상 부여
//         if (hapticEffector.MainForce > 5 && hapticEffector.distance_2d < 0.3)
//         {
//             SetReward(1.0f); // 좋은 성과에 대한 보상
//         }
//         else
//         {
//             SetReward(-0.1f); // 낮은 성과에 대한 패널티
//         }

//         // 에피소드 종료 조건 (선택 사항)
//         if (hapticEffector.distance_2d > 1.0f) // 특정 조건에 따라 에피소드 종료
//         {
//             Debug.Log("에피소드 종료");
//             EndEpisode();
//         }
//     }

//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         // 디버깅을 위해 사용자의 행동을 수동으로 지정할 수 있는 코드
//         // var discreteActionsOut = actionsOut.DiscreteActions;
//         // discreteActionsOut[0] = Random.Range(0, 3); // 예: 0(Default), 1(Disturbance), 2(Guidance) 중 무작위 선택
//         // Debug.Log("휴리스틱 결과: " + discreteActionsOut);

//         // if (Input.GetKey(KeyCode.A)) discreteActionsOut[0] = 1; // Disturbance
//         // else if (Input.GetKey(KeyCode.D)) discreteActionsOut[0] = 2; // Guidance
//         // else discreteActionsOut[0] = 0; // Default

//     }
// }

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class FeedbackAgent : Agent
{
    [SerializeField] private AdvancedPhysicsHapticEffector hapticEffector;
    [SerializeField] private SphereCollider Sphere;

    [SerializeField] private float performanceThresholdMainForceMin = 8.0f;
    [SerializeField] private float performanceThresholdMainForceMax = 20.0f;

    [SerializeField] private float performanceThresholdDistance = 0.3f; // 성능 기준 distance_2d 초기값

    public override void Initialize()
    {
        // 초기화 코드
        // performanceThresholdMainForce = 5.0f;
        performanceThresholdDistance = 0.3f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // MainForce와 distance_2d를 관측값으로 추가
        sensor.AddObservation(hapticEffector.MainForce);
        sensor.AddObservation(hapticEffector.distance_2d);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 성능 평가
        // bool performanceIsGood =  performanceThresholdMainForceMax >= hapticEffector.MainForce && hapticEffector.MainForce >= performanceThresholdMainForceMin && hapticEffector.distance_2d < performanceThresholdDistance;
        bool performanceIsGood =  hapticEffector.distance_2d < performanceThresholdDistance;


        // 행동 값 설정
        int action = actionBuffers.DiscreteActions[0];
        // Debug.Log(performanceIsGood);
        // Debug.Log(action);
        bool isCorrectAction = false;

        if (performanceIsGood)
        {
            if (action == 1) // Disturbance 선택
            {
                hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Disturbance;
                isCorrectAction = true;
                Debug.Log("성능이 좋아 Disturbance로 전환합니다.");
                Debug.Log($"PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");

            }
            else
            {
                hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Guidance; // 잘못된 행동
                // isCorrectAction = false;
                Debug.Log("성능이 좋은데, Guidance로 잘못 전환하였습니다.");
                Debug.Log($"PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");

            }
        }
        else
        {
            if (action == 2) // Guidance 선택
            {
                hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Guidance;
                isCorrectAction = true;
                Debug.Log("성능이 좋지 않아 Guidance로 전환합니다.");
                Debug.Log($"PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");

            }
            else
            {
                hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Disturbance; // 잘못된 행동
                // isCorrectAction = false;
                Debug.Log("성능이 좋지 않은데, Disturbance로 잘못 전환하였습니다.");
                Debug.Log($"PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");

            }
        }

        // 보상 및 패널티 부여
        if (isCorrectAction)
        {
            SetReward(10.0f); // 올바른 행동에 대한 보상
        }
        else
        {
            SetReward(-10.0f); // 잘못된 행동에 대한 패널티
        }

        // 에피소드 종료 조건
        if (hapticEffector.MainForce < 1.0f && hapticEffector.distance_2d > 0.8f)
        {
            Debug.Log("에피소드 종료");
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 사용자 입력을 수동으로 처리하지 않음 (사용자가 직접 움직임 확인)
    }
}
