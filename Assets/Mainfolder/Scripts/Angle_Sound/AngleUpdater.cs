using UnityEngine;
using UnityEngine.UI;

//맞게 계산되는지 확인해야함.
public class AngleUpdater : MonoBehaviour
{
    #region output
    public Text OutputText;
    private static string OUTPUT_TEXT = "Angle: {0}\nRotation\nX: {1}   Y: {2}   Z: {3}   ";
    #endregion

    #region ray
    private float angle;
    private Vector3 planeNormal;
    private Vector3 rayDirection;
    #endregion

    #region rotation
    private Vector3 rotation;
    #endregion

    private void Start()
    {
        // OutputText가 null인 경우에만 GameObject에서 찾아옴
        if (OutputText == null)
        {
            Debug.Log("OutputText is not assigned.");
        }
    }

    // RaycastHit 정보를 받아 각도를 업데이트하는 메서드
    public void AngleUpdate(RaycastHit hit, Vector3 originPos)
    {
        // originPos와 hit.point 사이의 방향 벡터 계산
        rayDirection = hit.point - originPos;

        // XZ 평면의 법선 벡터는 (0, 1, 0)입니다.
        planeNormal = Vector3.up;

        // XZ 평면과의 각도 계산
        float angle = Vector3.Angle(planeNormal, rayDirection);

        // 방향 벡터가 -Y 방향이면 각도를 음수로 변경
        if (rayDirection.y < 0)
        {
            angle = -angle;
        }

        rotation = transform.rotation.eulerAngles;
        OutputText.text = string.Format(OUTPUT_TEXT, angle, rotation.x, rotation.y, rotation.z);
    }

    public void AngleUpdate(Ray ray)
    {
        // xz 평면의 법선 벡터는 (0, 1, 0)입니다.
        planeNormal = Vector3.up;

        // 레이의 방향 벡터
        rayDirection = ray.direction.normalized;

        // xz 평면과 레이의 각도 계산
        float angle = Vector3.Angle(planeNormal, rayDirection) - 90;
        
        rotation = transform.rotation.eulerAngles;

        OutputText.text = string.Format(OUTPUT_TEXT, angle, rotation.x, rotation.y, rotation.z);
    }

    public void NoAngle()
    {
        OutputText.text = string.Format(OUTPUT_TEXT, "No Value", "_", "_", "_");
    }    
}
