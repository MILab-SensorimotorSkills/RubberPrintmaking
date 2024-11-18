using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class FeedbackAgent : Agent
{
    public AdvancedPhysicsHapticEffector hapticEffector;

    public override void Initialize()
    {
        // 초기화 코드: 필요에 따라 커스터마이징
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // MainForce와 distance_2d를 관측값으로 추가
        sensor.AddObservation(hapticEffector.MainForce);
        sensor.AddObservation(hapticEffector.distance_2d);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 행동 값(0: Default, 1: Disturbance, 2: Guidance) 기반으로 ForceFeedbackType 설정
        int action = actionBuffers.DiscreteActions[0];
        switch (action)
        {
            case 1:
                hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Disturbance;
                break;
            case 2:
                hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Guidance;
                break;
            default:
                hapticEffector.forceFeedbackType = AdvancedPhysicsHapticEffector.ForceFeedbackType.Default;
                break;
        }

        // 성과에 따라 보상 부여
        if (hapticEffector.MainForce < 5 && hapticEffector.distance_2d < 0.3)
        {
            SetReward(1.0f); // 좋은 성과에 대한 보상
        }
        else
        {
            SetReward(-0.1f); // 낮은 성과에 대한 패널티
        }

        // 에피소드 종료 조건 (선택 사항)
        if (hapticEffector.distance_2d > 5.0f) // 특정 조건에 따라 에피소드 종료
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 디버깅을 위해 사용자의 행동을 수동으로 지정할 수 있는 코드
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Random.Range(0, 3); // 예: 0(Default), 1(Disturbance), 2(Guidance) 중 무작위 선택
    }
}

