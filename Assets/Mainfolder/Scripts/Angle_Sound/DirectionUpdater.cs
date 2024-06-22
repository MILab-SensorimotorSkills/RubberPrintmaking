using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionUpdater : MonoBehaviour
{
    // 오브젝트들을 저장할 리스트
    public List<GameObject> objectsToActivate = new List<GameObject>();

    // 시간을 저장할 리스트
    public List<float> activationTimes = new List<float>();

    void Start()
    {
        // 모든 오브젝트를 비활성화
        foreach (var obj in objectsToActivate)
        {
            obj.SetActive(false);
        }
    }

    public void PlayDirection(){
        StartCoroutine(ActivateObjectsInSequence());
    }

    IEnumerator ActivateObjectsInSequence()
    {
        if (objectsToActivate.Count != activationTimes.Count)
        {
            Debug.LogError("The objectsToActivate and activationTimes lists must have the same length.");
            yield break;
        }

        for (int i = 0; i < objectsToActivate.Count; i++)
        {
            objectsToActivate[i].SetActive(true);
            yield return new WaitForSeconds(activationTimes[i]);
            objectsToActivate[i].SetActive(false);
        }
    }
}
