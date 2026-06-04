using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

// 선택된 가구의 UI 조작을 담당하는 스크립트
// 삭제, 회전, 크기 조절 기능을 처리하고 Photon을 통해 다른 사용자에게 동기화함
public class FurnitureUI : MonoBehaviourPunCallbacks
{
    public GameObject targetFurniture; // 현재 선택된 가구 오브젝트

    public Button deleteButton;        // 삭제 버튼
    public Button rotateButton;        // 회전 버튼
    public InputField widthInput;      // 가로 크기 입력 필드
    public InputField heightInput;     // 세로 크기 입력 필드

    private bool isRotating = false;   // 회전 버튼을 누르고 있는지 여부
    private float rotateInterval = 0.2f; // 회전 간격
    private float rotateTimer = 0f;      // 회전 시간 체크용 타이머
    private Vector3 originalScale;       // 가구의 원본 크기 저장

    void Start()
    {
        // 삭제 버튼 클릭 이벤트 등록
        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);

        // 회전 버튼을 누르고 있는 동안 계속 회전할 수 있도록 PointerDown/PointerUp 이벤트 등록
        if (rotateButton != null)
        {
            EventTriggerListener trigger = EventTriggerListener.Get(rotateButton.gameObject);
            if (trigger != null)
            {
                trigger.onPointerDown += StartRotate;
                trigger.onPointerUp += StopRotate;
            }
        }

        // 크기 입력이 끝났을 때 크기 변경 함수 호출
        if (widthInput != null)
            widthInput.onEndEdit.AddListener(delegate { OnScaleChanged(); });

        if (heightInput != null)
            heightInput.onEndEdit.AddListener(delegate { OnScaleChanged(); });
    }

    void Update()
    {
        // 회전 버튼을 누르고 있고 선택된 가구가 있을 때만 회전 처리
        if (isRotating && targetFurniture != null && targetFurniture.transform != null)
        {
            rotateTimer += Time.deltaTime;

            // 일정 시간 간격마다 가구를 회전
            if (rotateTimer >= rotateInterval)
            {
                // 삭제된 오브젝트를 참조하는 상황을 방지하기 위한 예외 처리
                if (targetFurniture == null || targetFurniture.transform == null)
                {
                    isRotating = false;
                    return;
                }

                // y축 기준으로 가구 회전
                targetFurniture.transform.Rotate(Vector3.up, 30f);
                rotateTimer = 0f;

                // 마스터 클라이언트 기준으로 회전값을 다른 사용자에게 동기화
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("SyncRotation", RpcTarget.AllBuffered, targetFurniture.transform.rotation);
                }
            }
        }
    }

    // 삭제 버튼 클릭 시 호출되는 함수
    public void OnDeleteButtonClicked()
    {
        if (targetFurniture != null)
        {
            PhotonView view = targetFurniture.GetComponent<PhotonView>();

            // 본인이 소유한 오브젝트라면 바로 PhotonNetwork.Destroy로 삭제
            if (view != null && view.IsMine)
            {
                PhotonNetwork.Destroy(targetFurniture);
            }
            else
            {
                // 소유권이 없을 경우 소유권을 요청한 뒤 삭제 시도
                view.RequestOwnership();
                PhotonNetwork.Destroy(targetFurniture);
            }

            // 삭제 후 선택 대상 초기화
            targetFurniture = null;
        }
    }

    // 회전 버튼을 눌렀을 때 호출
    public void StartRotate(GameObject go)
    {
        isRotating = true;
        rotateTimer = rotateInterval;
    }

    // 회전 버튼에서 손을 뗐을 때 호출
    public void StopRotate(GameObject go)
    {
        isRotating = false;
    }

    // UI에서 조작할 대상 가구를 설정하는 함수
    public void SetTarget(GameObject furniture)
    {
        targetFurniture = furniture;

        // FurnitureSize 스크립트에도 같은 대상 가구 전달
        GetComponent<FurnitureSize>()?.SetTarget(furniture.transform);

        // FurnitureInfo에 저장된 원본 스케일을 기준값으로 사용
        FurnitureInfo info = furniture.GetComponent<FurnitureInfo>();
        if (info != null)
        {
            originalScale = info.originalScale;
        }
        else
        {
            // FurnitureInfo가 없을 경우 현재 스케일을 원본 스케일로 사용
            originalScale = furniture.transform.localScale;
        }

        Vector3 currentScale = furniture.transform.localScale;

        // 현재 크기가 원본 크기 대비 몇 배인지 계산
        float widthRatio = (originalScale.x != 0f) ? currentScale.x / originalScale.x : 1f;
        float heightRatio = (originalScale.y != 0f) ? currentScale.y / originalScale.y : 1f;

        // 계산된 비율을 입력 필드에 표시
        widthInput.text = widthRatio.ToString("F2");
        heightInput.text = heightRatio.ToString("F2");
    }

    // 가로/세로 입력값이 변경되었을 때 가구 크기를 조절하는 함수
    public void OnScaleChanged()
    {
        // 선택된 가구가 없으면 실행하지 않음
        if (targetFurniture == null) return;

        float width, height;

        // 숫자로 변환할 수 없는 입력값은 무시
        if (!float.TryParse(widthInput.text, out width)) return;
        if (!float.TryParse(heightInput.text, out height)) return;

        // 원본 크기를 기준으로 입력된 비율만큼 새 크기 계산
        Vector3 newScale = new Vector3(
            originalScale.x * width,
            originalScale.y * height,
            originalScale.z * width
        );

        // 현재 사용자 화면에 크기 적용
        targetFurniture.transform.localScale = newScale;

        // PhotonView가 있고 본인 소유 오브젝트일 경우 다른 사용자에게 크기 동기화
        PhotonView view = targetFurniture.GetComponent<PhotonView>();
        if (view != null && view.IsMine)
        {
            photonView.RPC("SyncScale", RpcTarget.OthersBuffered, newScale);
        }
    }

    // 다른 사용자에게 삭제 상태를 동기화하기 위한 RPC 함수
    [PunRPC]
    private void SyncDelete()
    {
        if (targetFurniture != null)
        {
            Destroy(targetFurniture);
            targetFurniture = null;
        }
    }

    // 다른 사용자에게 회전값을 동기화하기 위한 RPC 함수
    [PunRPC]
    private void SyncRotation(Quaternion rotation)
    {
        if (targetFurniture != null)
        {
            targetFurniture.transform.rotation = rotation;
        }
    }

    // 다른 사용자에게 크기값을 동기화하기 위한 RPC 함수
    [PunRPC]
    private void SyncScale(Vector3 newScale)
    {
        if (targetFurniture != null)
        {
            targetFurniture.transform.localScale = newScale;
        }
    }
}