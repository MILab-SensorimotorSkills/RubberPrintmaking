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
    private float distance;
    private float[] volArr = new float[] {0.4f, 0.5f, 0.6f, 0.7f};

    // Start is called before the first frame update
    void Start()
    {
        if(audioSource == null){
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        lastPos = transform.position;
    }

    void Update(){
        if(frameCount<10){
            frameCount++;
        }else{
            frameCount = 0;
            currentPos = transform.position;
            distance = Vector3.Distance(currentPos, lastPos);
            if(collision && distance > 0.001){
                lastPos = currentPos;
                audioSource.volume = volArr[Random.Range(0, volArr.Length)];
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
    }
    
    public void StopSound(){
        collision = false;
    }
}
