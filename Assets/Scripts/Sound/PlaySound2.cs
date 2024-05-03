using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//0:  파기 시작하는 소리
//1: 파는 소리
//2: 파는 행위를 끝내는 소리
public class PlaySound2 : MonoBehaviour
{
    public AudioClip[] arrAudio = new AudioClip[3];


    private AudioSource audioSource;
    private bool isPlaying = false;
    private float currentPitch = 0;

    private Vector3 currentPos;
    private Vector3 lastPos;
    private float distance;
    private float currentVolume;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = arrAudio[1];
        Debug.Log(audioSource.clip.length);

    }

    void OnTriggerEnter(Collider other){
        // Debug.Log("충돌 시작");

        lastPos = GetComponent<Transform>().position;
        audioSource.volume = 1.0f;
        audioSource.PlayOneShot(arrAudio[0], 1.0f);

    }


    void OnTriggerStay(Collider other){
        // Debug.Log("충돌 진행");
        currentPos = GetComponent<Transform>().position;

        if(!isPlaying){
            isPlaying = true;

            distance = Vector3.Distance(lastPos, currentPos);
            distance = Mathf.InverseLerp(0.01f, 1.0f, distance);
            

            if(distance != 0){
                currentVolume = Mathf.Round(distance * 10f)/10f;
                currentVolume = Mathf.Clamp(distance, 0.1f, 1.0f);
                audioSource.volume = currentVolume;
                Debug.Log("Volume: " + currentVolume);

                distance = Mathf.Clamp(distance, 0.85f, 1.0f);
                currentPitch = Mathf.Round(distance * 100f)/100f;
                audioSource.pitch = currentPitch;

                Debug.Log("Pitch: " + currentPitch);

                audioSource.clip = arrAudio[1];
                audioSource.Play();
            }else{
                audioSource.Stop();
            }
            lastPos = currentPos;

            StartCoroutine(WaitForAudioClipEnd());

        }

    }

    void OnTriggerExit(Collider other){
        // Debug.Log("충돌 끝");
        
        audioSource.volume = 1.0f;
        audioSource.PlayOneShot(arrAudio[2], 1.0f);
    }

    IEnumerator WaitForAudioClipEnd(){
        yield return new WaitForSeconds(0.15f);
        isPlaying = false;
    }

}
