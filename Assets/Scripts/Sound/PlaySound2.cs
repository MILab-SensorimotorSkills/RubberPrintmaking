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
    private float clipLength = 0;
    private float currentPitch = 0;

    private Vector3 currentPosS;
    private Vector3 lastPosS;
    private float distanceS;

    private Vector3 currentPosE;
    private Vector3 lastPosE;
    private float distanceE;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

    }

    void OnTriggerEnter(Collider other){
        Debug.Log("충돌 시작");

        lastPosS = GetComponent<Transform>().position;
        audioSource.PlayOneShot(arrAudio[0], 1.0f);

    }


    void OnTriggerStay(Collider other){
        Debug.Log("충돌 진행");
        currentPosS = GetComponent<Transform>().position;

        if(!isPlaying){
            isPlaying = true;

            distanceS = Vector3.Distance(lastPosS, currentPosS);
            distanceS = Mathf.InverseLerp(0.01f, 2f, distanceS);

            currentPitch = Mathf.Round(distanceS * 10f)/10f;
            audioSource.pitch = currentPitch;

            Debug.Log(currentPitch);

            audioSource.clip = arrAudio[1];
            audioSource.Play();

            lastPosS = currentPosS;

            StartCoroutine(WaitForAudioClipEnd());

        }

    }

    void OnTriggerExit(Collider other){
        Debug.Log("충돌 끝");
        
        audioSource.PlayOneShot(arrAudio[2], 1.0f);
    }

    IEnumerator WaitForAudioClipEnd(){
        yield return new WaitForSeconds(0.3f);
        isPlaying = false;
    }

}
