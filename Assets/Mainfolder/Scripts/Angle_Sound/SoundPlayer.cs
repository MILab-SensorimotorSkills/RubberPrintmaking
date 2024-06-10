using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float positionCheckInterval = 0.2f; // 위치를 체크하는 간격 (초)
    private bool isGrounded = false;
    private float positionThreshold = 0.0005f; // 위치 변화 임계값

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is missing on this GameObject.");
        }

        lastPosition = transform.position;
        StartCoroutine(CheckPosition());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = true;
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = false;
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    IEnumerator CheckPosition()
    {
        while (true)
        {
            yield return new WaitForSeconds(positionCheckInterval);

            // 위치 변화 계산
            float positionDifference = Vector3.Distance(transform.position, lastPosition);

            // 위치 변화가 임계값 이하이면 소리 일시 중지
            if (isGrounded && positionDifference < positionThreshold)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Pause();
                }
            }
            // 위치 변화가 임계값 이상이면 소리 이어서 재생
            else if (isGrounded && positionDifference >= positionThreshold)
            {
                if (audioSource != null && !audioSource.isPlaying)
                {
                    audioSource.UnPause();
                }
            }

            lastPosition = transform.position;
        }
    }
}
