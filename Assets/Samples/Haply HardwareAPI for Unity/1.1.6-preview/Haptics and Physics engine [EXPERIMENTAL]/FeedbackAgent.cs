// using Unity.MLAgents;
// using Unity.MLAgents.Actuators;
// using Unity.MLAgents.Sensors;
// using UnityEngine;

// public class FeedbackAgent : Agent
// {
//     [SerializeField] private AdvancedPhysicsHapticEffector hapticEffector;
//     [SerializeField] private SphereCollider Sphere;

//     [SerializeField] private float performanceThresholdMainForceMin = 8.0f;
//     [SerializeField] private float performanceThresholdMainForceMax = 20.0f;

//     [SerializeField] private float performanceThresholdDistance = 0.3f; // 성능 기준 distance_2d 초기값

//     public override void Initialize()
//     {
//         // 초기화 코드
//         // performanceThresholdMainForce = 5.0f;
//         performanceThresholdDistance = 0.3f;
//     }

//     public override void CollectObservations(VectorSensor sensor)
//     {
//         // MainForce와 distance_2d를 관측값으로 추가
//         sensor.AddObservation(hapticEffector.MainForce);
//         sensor.AddObservation(hapticEffector.distance_2d);
//     }

//     public override void OnActionReceived(ActionBuffers actionBuffers)
//     {
//         // 성능 평가
//         bool performanceIsGood =  performanceThresholdMainForceMax >= hapticEffector.MainForce && hapticEffector.MainForce >= performanceThresholdMainForceMin && hapticEffector.distance_2d < performanceThresholdDistance;
//         // bool performanceIsGood =  hapticEffector.distance_2d < performanceThresholdDistance;


//         // 행동 값 설정
//         int action = actionBuffers.DiscreteActions[0];
//         // Debug.Log(performanceIsGood);
//         // Debug.Log(action);
//         bool isCorrectAction = false;

//         if (performanceIsGood)
//         {
//             if (action == 1) // Disturbance 선택
//             {
//                 hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Disturbance;
//                 isCorrectAction = true;
//                 Debug.Log("성능이 좋아 Disturbance로 전환합니다.");
//                 Debug.Log($"PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");

//             }
//             else
//             {
//                 hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Guidance; // 잘못된 행동
//                 // isCorrectAction = false;
//                 Debug.Log("성능이 좋은데, Guidance로 잘못 전환하였습니다.");
//                 Debug.Log($"PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");

//             }
//         }
//         else
//         {
//             if (action == 2) // Guidance 선택
//             {
//                 hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Guidance;
//                 isCorrectAction = true;
//                 Debug.Log("성능이 좋지 않아 Guidance로 전환합니다.");
//                 Debug.Log($"PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");

//             }
//             else
//             {
//                 hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Disturbance; // 잘못된 행동
//                 // isCorrectAction = false;
//                 Debug.Log("성능이 좋지 않은데, Disturbance로 잘못 전환하였습니다.");
//                 Debug.Log($"PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");

//             }
//         }

//         // 보상 및 패널티 부여
//         if (isCorrectAction)
//         {
//             SetReward(10.0f); // 올바른 행동에 대한 보상
//         }
//         else
//         {
//             SetReward(-10.0f); // 잘못된 행동에 대한 패널티
//         }

//         // 에피소드 종료 조건
//         if (hapticEffector.MainForce < 1.0f && hapticEffector.distance_2d > 0.8f)
//         {
//             Debug.Log("에피소드 종료");
//             EndEpisode();
//         }
//     }

//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         // 사용자 입력을 수동으로 처리하지 않음 (사용자가 직접 움직임 확인)
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

    [SerializeField] private float performanceThresholdMainForceMin = 2.0f;
    [SerializeField] private float performanceThresholdMainForceMax = 30.0f;
    [SerializeField] private float performanceThresholdDistance = 0.3f;
    private float fixedDistance2D;
    private float fixedMainforce;

    public override void Initialize()
    {
        performanceThresholdDistance = 0.3f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(fixedMainforce);
        sensor.AddObservation(fixedMainforce);
    }

    public override void OnEpisodeBegin()
    {
        // 성능이 좋은 상태가 더 자주 나오도록 가중치를 적용한 랜덤 값 생성
        float randomValue = Random.value; // 0.0 ~ 1.0 사이의 난수 생성

        if (randomValue < 0.7f) // 70% 확률로 성능이 좋은 상태 생성
        {
            fixedMainforce = Random.Range(performanceThresholdMainForceMin, performanceThresholdMainForceMax); // 성능이 좋은 MainForce 범위
            fixedDistance2D = Random.Range(0.01f, performanceThresholdDistance); // 성능이 좋은 거리 범위
        }
        else // 30% 확률로 성능이 나쁜 상태 생성
        {
            fixedMainforce = Random.Range(0.2391f, performanceThresholdMainForceMin); // 성능이 나쁜 MainForce 범위
            fixedMainforce = Random.Range(performanceThresholdDistance, 1.0f); // 성능이 나쁜 거리 범위
        }

        Debug.Log($"[OnEpisodeBegin] MainForce: {fixedMainforce}, Distance: {fixedDistance2D}");
        
        // 랜덤 값으로 MainForce와 distance_2d 초기화
        // hapticEffector.MainForce = Random.Range(0.2391f, 30.0f);  // 0.2391~30 사이의 랜덤 값
        // hapticEffector.distance_2d = Random.Range(0.01f, 1.0f); // 0.01~2.0 사이의 랜덤 값
        // Debug.Log("Mainforce: " + hapticEffector.MainForce + "distance error: " + hapticEffector.distance_2d);

        // hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Default;
    }


    // private bool EvaluatePerformance()
    // {
        
    //      // 항상 고정된 값을 사용하도록 확인
    //     float currentDistance = hapticEffector.distance_2d;
    //     Debug.Log($"[EvaluatePerformance] MainForce: {hapticEffector.MainForce}, Distance: {currentDistance}");

    //     bool isMainForceInRange = performanceThresholdMainForceMax >= hapticEffector.MainForce &&
    //                             hapticEffector.MainForce >= performanceThresholdMainForceMin;
    //     bool isDistanceInRange = currentDistance < performanceThresholdDistance;

    //     return isMainForceInRange && isDistanceInRange;
    //     // True가 왜 출력이 안되지?
    //     // return performanceThresholdMainForceMax >= hapticEffector.MainForce && hapticEffector.MainForce >= performanceThresholdMainForceMin && hapticEffector.distance_2d < performanceThresholdDistance;
    // }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        fixedMainforce = Random.Range(0.2391f, 30.0f);
        fixedDistance2D = Random.Range(0.01f, 1.0f);

        // 성능 판단 로직
        bool isMainForceInRange = performanceThresholdMainForceMax >= fixedMainforce &&
                                  fixedMainforce >= performanceThresholdMainForceMin;
        bool isDistanceInRange = fixedDistance2D < performanceThresholdDistance;
        bool performanceIsGood = isMainForceInRange && isDistanceInRange;
        
        int action = actionBuffers.DiscreteActions[0];
        bool isCorrectAction = false;

        if (performanceIsGood && action == 0)
        {
            hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Disturbance;
            isCorrectAction = true;
        }
        else if (!performanceIsGood && action == 1)
        {
            hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Guidance;
            isCorrectAction = true;
        }
        // else if (action == 0)
        // {
        //     hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Default;
        // }
        // Debug.Log($"MainForce: {fixedMainforce}, Distance: {fixedDistance2D}, PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");


        // if (isCorrectAction) //disturbance가 왜 학습이 안되지? reward 가중치 주기
        // {
        //     if (action == 0) SetReward(10.0f); // Disturbance 보상 증가
        //     else if (action == 1) SetReward(5.0f); // Guidance 보상 기본 유지
        // }
        // else
        // {
        //     SetReward(-10.0f); // 실패에 대한 패널티 유지
        // }

        SetReward(isCorrectAction ? 1.0f : -1.0f);

        // if (hapticEffector.MainForce < 1.0f && hapticEffector.distance_2d > 0.8f)
        if (fixedMainforce < 0.3f)
        {
            EndEpisode();
        }

        if (Application.isEditor)  // 에디터 환경에서만 출력
        {
            // Debug.Log($"MainForce: {hapticEffector.MainForce}, Distance: {fixedDistance2D}, PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 0; // Disturbance
        }
        else if (Input.GetKey(KeyCode.G))
        {
            discreteActions[0] = 1; // Guidance
        }
        // else
        // {
        //     discreteActions[0] = 0; // Default
        // }
    }
}

// timestamp 10으로 고정
// using Unity.MLAgents;
// using Unity.MLAgents.Actuators;
// using Unity.MLAgents.Sensors;
// using UnityEngine;

// public class FeedbackAgent : Agent
// {
//     [SerializeField] private AdvancedPhysicsHapticEffector hapticEffector;

//     [SerializeField] private float performanceThresholdMainForceMin = 8.0f;
//     [SerializeField] private float performanceThresholdMainForceMax = 30.0f;
//     [SerializeField] private float performanceThresholdDistance = 0.3f;

//     private int frameCounter = 0; // 프레임 카운터
//     private int frameInterval = 10; // 전환 간격 (프레임 단위)

//     public override void Initialize()
//     {
//         performanceThresholdDistance = 0.3f;
//     }

//     public override void CollectObservations(VectorSensor sensor)
//     {
//         // 랜덤 값으로 설정된 MainForce와 distance_2d를 관측
//         sensor.AddObservation(hapticEffector.MainForce);
//         sensor.AddObservation(hapticEffector.distance_2d);
//     }

//     public override void OnEpisodeBegin()
//     {
//         // 랜덤 값으로 MainForce와 distance_2d 초기화
//         hapticEffector.MainForce = Random.Range(0.2391f, 30.0f);
//         hapticEffector.distance_2d = Random.Range(0.01f, 2.0f);

//         hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Default;
//         frameCounter = 0; // 카운터 초기화
//     }

//     private bool EvaluatePerformance()
//     {
//         // 랜덤 값에 기반한 성능 평가
//         return performanceThresholdMainForceMax >= hapticEffector.MainForce &&
//                hapticEffector.MainForce >= performanceThresholdMainForceMin &&
//                hapticEffector.distance_2d < performanceThresholdDistance;
//     }

//     public override void OnActionReceived(ActionBuffers actionBuffers)
//     {
//         frameCounter++; // 프레임 카운터 증가

//         // 일정 프레임 간격으로만 동작
//         if (frameCounter < frameInterval) return;

//         frameCounter = 0; // 카운터 초기화

//         bool performanceIsGood = EvaluatePerformance();
//         int action = actionBuffers.DiscreteActions[0];
//         bool isCorrectAction = false;

//         // 행동에 따라 hapticEffector 설정
//         if (performanceIsGood && action == 1)
//         {
//             hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Disturbance;
//             isCorrectAction = true;
//         }
//         else if (!performanceIsGood && action == 2)
//         {
//             hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Guidance;
//             isCorrectAction = true;
//         }
//         else if (action == 0)
//         {
//             hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Default;
//         }

//         // 보상 부여
//         SetReward(isCorrectAction ? 2.0f : -2.0f);

//         // 종료 조건
//         if (hapticEffector.MainForce < 1.0f && hapticEffector.distance_2d > 0.8f)
//         {
//             EndEpisode();
//         }

//         if (Application.isEditor)
//         {
//             Debug.Log($"PerformanceIsGood: {performanceIsGood}, Action: {action}, CorrectAction: {isCorrectAction}");
//         }
//     }

//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         // 랜덤한 행동 선택 (테스트 목적)
//         var discreteActions = actionsOut.DiscreteActions;
//         discreteActions[0] = Random.Range(0, 3); // 0(Default), 1(Disturbance), 2(Guidance) 중 랜덤
//     }
// }
