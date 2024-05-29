using UnityEngine;

public class RotationFixer : MonoBehaviour
{
    private Quaternion targetRotation = Quaternion.Euler(0, 270, 0);

    void Awake()
    {
        // 오브젝트의 모습을 0, 270, 0으로 보이게 설정합니다.
        transform.rotation = targetRotation;
    }

    void LateUpdate()
    {
        // 매 프레임마다 오브젝트의 rotation을 0, 0, 0으로 유지합니다.
        transform.localRotation = Quaternion.identity;

        // targetRotation만큼 반대 방향으로 회전하여 원래 모습 유지
        transform.Rotate(0, 270, 0, Space.Self);
    }
}
