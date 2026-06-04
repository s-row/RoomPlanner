using UnityEngine;
using Photon.Pun;

// 가구 오브젝트 클릭 처리를 담당하는 스크립트
// 클릭한 가구의 Photon 소유권을 요청하고, FurnitureUI에 선택된 가구로 전달함
public class FurnitureClick : MonoBehaviourPun
{
    private FurnitureUI uiScript; // 가구 조작 UI 스크립트 참조

    void Start()
    {
        // 씬에서 FurnitureUI 오브젝트를 찾아 UI 스크립트 연결
        GameObject uiObj = GameObject.Find("FurnitureUI");
        if (uiObj != null)
        {
            uiScript = uiObj.GetComponent<FurnitureUI>();
        }
    }

    void OnMouseDown()
    {
        // 클릭한 가구 오브젝트의 PhotonView 가져오기
        PhotonView view = GetComponent<PhotonView>();

        if (view != null)
        {
            // 현재 사용자가 해당 가구의 소유자가 아니면 소유권 요청
            // 실시간 환경에서 가구를 조작하기 위해 필요한 처리
            if (!view.IsMine)
            {
                view.RequestOwnership();
            }

            // 선택한 가구를 FurnitureUI에 전달하여 이동, 회전, 삭제, 크기 조절 대상으로 설정
            if (uiScript != null)
            {
                uiScript.SetTarget(gameObject);
            }
        }
    }
}