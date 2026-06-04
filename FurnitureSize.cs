using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

// 가구의 가로/세로 크기 조절을 담당하는 스크립트
// Photon을 사용해 크기 변경 내용을 다른 사용자에게 동기화함
public class FurnitureSize : MonoBehaviourPun
{
    [SerializeField] private InputField widthInput;   // 가구의 가로 크기 입력 필드
    [SerializeField] private InputField heightInput;  // 가구의 세로 크기 입력 필드

    private Transform target; // 현재 크기를 조절할 대상 가구

    // 외부에서 크기 조절 대상 가구를 설정하는 함수
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        // 대상 가구가 있으면 현재 크기 값을 UI 입력창에 표시
        if (target != null)
        {
            Vector3 scale = target.localScale;

            // Unity scale 값을 cm 단위처럼 보이도록 100을 곱해서 표시
            if (widthInput != null) widthInput.text = (scale.x * 100f).ToString();
            if (heightInput != null) heightInput.text = (scale.z * 100f).ToString();
        }
    }

    // 가로 크기 입력값이 변경되었을 때 호출
    public void OnWidthChanged(string value)
    {
        // 본인 소유 객체가 아니거나 대상이 없거나 입력값이 비어 있으면 실행하지 않음
        if (!photonView.IsMine || target == null || string.IsNullOrEmpty(value)) return;

        // 입력된 문자열을 숫자로 변환할 수 있을 때만 크기 변경
        if (float.TryParse(value, out float cm))
        {
            Vector3 scale = target.localScale;

            // 입력값을 100으로 나누어 Unity scale 값으로 변환
            scale.x = cm / 100f;

            // 현재 사용자의 화면에 크기 적용
            target.localScale = scale;

            // 다른 접속자들에게 변경된 크기 동기화
            photonView.RPC("SyncScale", RpcTarget.Others, scale);
        }
    }

    // 세로 크기 입력값이 변경되었을 때 호출
    public void OnHeightChanged(string value)
    {
        // 본인 소유 객체가 아니거나 대상이 없거나 입력값이 비어 있으면 실행하지 않음
        if (!photonView.IsMine || target == null || string.IsNullOrEmpty(value)) return;

        // 입력된 문자열을 숫자로 변환할 수 있을 때만 크기 변경
        if (float.TryParse(value, out float cm))
        {
            Vector3 scale = target.localScale;

            // 3D 공간에서 세로 방향을 z축 기준으로 조절
            scale.z = cm / 100f;

            // 현재 사용자의 화면에 크기 적용
            target.localScale = scale;

            // 다른 접속자들에게 변경된 크기 동기화
            photonView.RPC("SyncScale", RpcTarget.Others, scale);
        }
    }

    // Photon RPC로 다른 사용자에게 가구 크기 변경값을 전달받아 적용
    [PunRPC]
    private void SyncScale(Vector3 newScale)
    {
        if (target != null)
        {
            // 전달받은 크기 값으로 대상 가구 크기 변경
            target.localScale = newScale;

            // 동기화된 크기 값을 UI 입력창에도 반영
            if (widthInput != null) widthInput.text = (newScale.x * 100f).ToString();
            if (heightInput != null) heightInput.text = (newScale.z * 100f).ToString();
        }
    }
}