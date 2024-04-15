using UnityEngine;
using System.Collections;


public class PlaySound : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isPlaying = false;
    private float clipLength = 0;
    private float minPitch = 0.5f;
    private float maxPitch = 2.0f;


    private Vector3 currentMouse;
    private Vector3 lastMouse;
    private float lastCheck;
    private float mouseDistance;
    private float mouseSpeed;
    private float checkInterval = 0.1f;
    
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        clipLength = audioSource.clip.length;
        Debug.Log("오디오 클립 길이는 " + clipLength + "초");

        lastMouse = Input.mousePosition;
        lastCheck = Time.time;
    }

    void Update()
    {
        if(!isPlaying && Input.GetMouseButton(0)){
            StartCoroutine(PlayAudioWithSpeed());
        }

        if(Time.time - lastCheck >= checkInterval){
            mouseDistance = Vector3.Distance(lastMouse, currentMouse);
            mouseSpeed = mouseDistance/checkInterval;
            mouseSpeed = Mathf.InverseLerp(minPitch, maxPitch, mouseSpeed);

            audioSource.pitch = mouseSpeed;

            lastMouse = currentMouse;
            lastCheck = Time.time;
        }

    }

    IEnumerator PlayAudioWithSpeed(){
        isPlaying = true;

        audioSource.time = clipLength / 5 * 2; // 재생 위치 설정
        audioSource.Play();

        // 재생 중인 소리가 끝날 때까지 대기
        yield return new WaitForSeconds(clipLength); 

        isPlaying = false;
    }
}
