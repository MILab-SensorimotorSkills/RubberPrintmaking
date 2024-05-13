using UnityEngine;

public class RaycastFromObj : MonoBehaviour
{   
    private static GameObject raycastOrigin;
    public float maxRayDistance = 2f;

    void Start(){
        raycastOrigin = gameObject;
    }

    void Update()
    {
        Vector3 originPos = raycastOrigin.transform.position;
        Vector3 originDir = raycastOrigin.transform.forward;

        RaycastHit hit; // RaycastHit 변수 선언

        Ray ray = new Ray(originPos, originDir);

        if (Physics.Raycast(ray, out hit, maxRayDistance))
        {
            Debug.Log("Hit obj: " + hit.collider.gameObject.name);
            Debug.DrawLine(originPos, hit.point, Color.green); // 충돌 지점까지의 라인을 그립니다.
        }
        else
        {
            Debug.Log("No object hit");
            Vector3 endPoint = originPos + originDir * maxRayDistance;
            Debug.DrawLine(originPos, endPoint, Color.red); // 최대 거리까지의 라인을 그립니다.
        }
    }
}
