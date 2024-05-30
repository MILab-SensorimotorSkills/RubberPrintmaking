using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private bool collision = false;

    private Vector3 lastPos;
    private Vector3 currentPos;
    private int frameCount = 0;
    private bool isPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        if(audioSource == null){
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        lastPos = transform.position;
    }

    void Update(){
        if(frameCount<5){
            frameCount++;
        }else{
            frameCount = 0;
            currentPos = transform.position;
            if(collision && currentPos != lastPos){
                if(!isPlaying){
                    isPlaying = true;
                    audioSource.Play();
                }
            }else{
                isPlaying = false;
                audioSource.Stop();
            }
        }

    }


    public void PlaySound(){
        collision = true;
        Debug.Log("플레이");
    }
    
    public void StopSound(){
        collision = false;
        Debug.Log("스탑");
    }
}
