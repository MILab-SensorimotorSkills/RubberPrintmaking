using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound1 : MonoBehaviour
{
public Transform targetObj;
    public float maxRayDistance = 10f;
    public AudioSource audioSource;
    public AudioClip audioClip;
    public float audioPlayDuration = 0.1f; // 오디오 재생 시간

    private Vector3? previousHitPoint = null; // 이전 충돌 지점
    private bool isAudioPlaying = false; // 오디오가 재생 중인지 확인하는 변수

    void Update()
    {
        Vector3 originPos = targetObj.position;
        Vector3 originDir = targetObj.right;

        RaycastHit hit;
        Ray ray = new Ray(originPos, originDir);

        if (Physics.Raycast(ray, out hit, maxRayDistance))
        {
            Vector3 currentHitPoint = hit.point;

            // 이전 충돌 지점과 현재 충돌 지점을 비교
            if (previousHitPoint == null || previousHitPoint != currentHitPoint)
            {
                // 오디오 재생
                PlayAudio();

                // 현재 충돌 지점을 이전 충돌 지점으로 업데이트
                previousHitPoint = currentHitPoint;
            }

            // 디버그 라인 그리기
            Debug.DrawLine(originPos, hit.point, Color.green);
        }
        else
        {
            // 디버그 라인 그리기
            Debug.DrawLine(originPos, originPos + originDir * maxRayDistance, Color.red);

            // 충돌 지점이 없을 때 이전 충돌 지점을 null로 설정
            previousHitPoint = null;
        }
    }

    void PlayAudio()
    {
        // 오디오 클립이 설정되지 않았으면 리턴
        if (audioClip == null || audioSource == null)
            return;

        // 오디오가 재생 중이 아닐 때만 재생
        if (!isAudioPlaying)
        {
            audioSource.PlayOneShot(audioClip);
            isAudioPlaying = true;

            // 일정 시간 후에 오디오 멈추기
            Invoke("StopAudio", audioPlayDuration);
        }
    }

    void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        isAudioPlaying = false;
    }
}
