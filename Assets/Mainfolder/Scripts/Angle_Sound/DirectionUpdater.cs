// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class DirectionUpdater : MonoBehaviour
// {
//     // 오브젝트들을 저장할 리스트
//     public List<GameObject> objectsToActivate = new List<GameObject>();

//     // 시간을 저장할 리스트
//     public List<float> activationTimes = new List<float>();

//     void Start()
//     {
//         // 모든 오브젝트를 비활성화
//         foreach (var obj in objectsToActivate)
//         {
//             obj.SetActive(false);
//         }
//     }

//     public void PlayDirection(){
//         StartCoroutine(ActivateObjectsInSequence());
//     }

//     IEnumerator ActivateObjectsInSequence()
//     {
//         if (objectsToActivate.Count != activationTimes.Count)
//         {
//             Debug.LogError("The objectsToActivate and activationTimes lists must have the same length.");
//             yield break;
//         }

//         for (int i = 0; i < objectsToActivate.Count; i++)
//         {
//             objectsToActivate[i].SetActive(true);
//             yield return new WaitForSeconds(activationTimes[i]);
//             objectsToActivate[i].SetActive(false);
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionUpdater : MonoBehaviour
{
    public List<GameObject> objectsToActivate = new List<GameObject>();
    public List<float> activationTimes = new List<float>();

    private bool isPaused = false; // 일시정지 상태를 나타내는 변수
    private Coroutine currentCoroutine; // 현재 실행 중인 코루틴을 저장

    void Start()
    {
        foreach (var obj in objectsToActivate)
        {
            obj.SetActive(false);
        }
    }

    public void PlayDirection()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine); // 기존 코루틴이 있으면 정지
        }
        currentCoroutine = StartCoroutine(ActivateObjectsInSequence());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
            {
                // 재생
                isPaused = false;
                if (currentCoroutine == null)
                {
                    currentCoroutine = StartCoroutine(ActivateObjectsInSequence());
                }
            }
            else
            {
                // 일시정지
                isPaused = true;
            }
        }
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
            while (isPaused) // 일시정지 상태에서는 대기
            {
                yield return null;
            }

            objectsToActivate[i].SetActive(true);
            float elapsedTime = 0;

            while (elapsedTime < activationTimes[i])
            {
                if (isPaused)
                {
                    yield return null;
                }
                else
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            objectsToActivate[i].SetActive(false);
        }

        currentCoroutine = null; // 모든 작업이 끝나면 코루틴을 초기화
    }
}
