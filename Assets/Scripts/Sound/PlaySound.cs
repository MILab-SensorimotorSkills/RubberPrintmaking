using UnityEngine;
using System.Collections;


public class PlaySound : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isPlaying = false;
    private float clipLength = 0;

    private Transform objTransform;
    private Vector3 currentPos;
    private Vector3 lastPos;
    private float lastCheck;
    private float objDistance;
    private float objSpeed;
    private float checkInterval = 0.1f;
    
    private bool pitchOver = false;
    
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        clipLength = audioSource.clip.length;
        Debug.Log("오디오 클립 길이는 " + clipLength + "초");

        objTransform = GetComponent<Transform>();
        lastPos = objTransform.position;
        lastCheck = Time.time;
    }

    void Update()
    {
        //오디오 pitch 업데이트. 0이면 나오지 않음. 
        if(Time.time - lastCheck >= checkInterval){

            currentPos = objTransform.position;
            objDistance = Vector3.Distance(lastPos, currentPos);
            
            objSpeed = Mathf.Clamp(objDistance, 0.1f, 6);

            if(objSpeed >= 3)   pitchOver = true;
            else    pitchOver = false;

            //최소, 최대 이동 거리 임의 지정
            objSpeed = Mathf.InverseLerp(0.1f, 3, objSpeed);

            //0.1~3은 pitch 1로, 3~6은 pitch 2까지 도달 가능
            if(pitchOver) objSpeed += 1;

            audioSource.pitch = objSpeed;

            lastPos = currentPos;
            lastCheck = Time.time;
        }

    }

    void OnTriggerEnter(Collider other){
        if(other.CompareTag("DrawableCanvas")){
            if(!isPlaying){
                StartCoroutine(PlayAudioWithSpeed());
            }
        }
    }

    void OnTriggerExit(Collider other){
        if(other.CompareTag("DrawableCanvas")){
            if(isPlaying){
                StopCoroutine(PlayAudioWithSpeed());
                isPlaying = false;
            }
        }
    }

    IEnumerator PlayAudioWithSpeed(){
        isPlaying = true;
        if(audioSource.pitch < 0.5f){
            audioSource.pitch = 0.5f;
        }
        audioSource.time = clipLength / 5 * 2; // 재생 위치 설정
        audioSource.Play();

        // 재생 중인 소리가 끝날 때까지 대기
        yield return new WaitForSeconds(clipLength); 

        isPlaying = false;
    }
}
