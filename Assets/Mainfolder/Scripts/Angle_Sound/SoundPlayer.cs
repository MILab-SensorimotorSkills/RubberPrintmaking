using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public GameObject virtualCollider;  // virtualCollider 오브젝트를 공개 변수로 선언
    private AudioSource audioSource;
    private float yThreshold = -0.157f;
    private float volumeMinY = -0.217f;
    private float volumeMaxY = -0.157f;
    private float volumeMin = 0.6f;
    private float volumeMax = 1.0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Stop();

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is missing on this GameObject.");
        }

        if (virtualCollider == null)
        {
            Debug.LogError("virtualCollider GameObject reference is missing.");
        }
    }

    void Update()
    {   
        if (virtualCollider == null)
        {
            return;
        }

        float currentY = virtualCollider.transform.position.y;

        if (currentY < yThreshold)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            // Adjust volume based on y position
            if (currentY >= volumeMinY && currentY <= volumeMaxY)
            {
                float t = (currentY - volumeMinY) / (volumeMaxY - volumeMinY);
                audioSource.volume = Mathf.Lerp(volumeMin, volumeMax, t);
            }
            else if (currentY < volumeMinY)
            {
                audioSource.volume = volumeMin;
            }
            else if (currentY > volumeMaxY)
            {
                audioSource.volume = volumeMax;
            }
        }
        else
        {
            audioSource.Stop();
        }
    }
}
