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

        audioSource.clip = arrAudio[0];
    }

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Ground"){

            lastPos = GetComponent<Transform>().position;
        }

    }


    void OnTriggerStay(Collider other){

        if(other.gameObject.tag == "Ground"){
            currentPos = GetComponent<Transform>().position;

            if(!isPlaying){
                isPlaying = true;

                distance = Vector3.Distance(lastPos, currentPos);
                distance = Mathf.InverseLerp(0.0001f, 0.1f, distance);
                

                if(distance != 0){
                    Debug.Log(distance);
                    currentVolume = Mathf.Round(distance * 10f)/10f;
                    currentVolume = Mathf.Clamp(distance, 0.1f, 0.3f);
                    audioSource.volume = currentVolume;
                    //Debug.Log("Volume: " + currentVolume);

                    distance = Mathf.Clamp(distance, 0.95f, 1.0f);
                    currentPitch = Mathf.Round(distance * 100f)/100f;
                    audioSource.pitch = currentPitch;

                    //Debug.Log("Pitch: " + currentPitch);

                    audioSource.clip = arrAudio[0];
                    audioSource.Play();
                }else{
                    audioSource.Stop();
                }
                lastPos = currentPos;

                StartCoroutine(WaitForAudioClipEnd());

            }
        }
    }

    IEnumerator WaitForAudioClipEnd(){
        yield return new WaitForSeconds(0.2f);
        isPlaying = false;
    }
}
