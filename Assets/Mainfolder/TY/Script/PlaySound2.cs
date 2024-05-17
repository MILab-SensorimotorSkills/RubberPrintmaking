using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//05.10 OnTriggerEnter가 Stay처럼 계속해서 호출되는 문제가 있음.
//Stay를 거치지 않고, Start가 연속으로 호출될 경우 무시하는 로직 추가 구현 필요
//해당 문제로 lastPos가 currentPos와 동일시 되거나, 0처리 되는 문제로 오디오가 재생되지 않음.
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
