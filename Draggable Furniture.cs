using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

// 가구 오브젝트의 드래그 이동을 담당하는 스크립트
// Photon 소유권을 요청한 뒤, 바닥 평면 위에서 가구를 이동시킴
public class DraggableFurniture : MonoBehaviourPun, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 offset;           // 마우스 클릭 지점과 오브젝트 중심 사이의 거리
    private Camera cam;               // 메인 카메라 참조
    private bool isDragging;          // 현재 드래그 중인지 여부
    private Vector3 originalPosition; // 드래그 시작 전 위치 저장

    void Start()
    {
        // 화면 좌표를 월드 좌표로 변환하기 위해 메인 카메라 저장
        cam = Camera.main;
    }

    // 드래그 시작 시 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 다른 사용자가 소유한 가구라도 조작할 수 있도록 소유권 요청
        if (!photonView.IsMine)
        {
            photonView.RequestOwnership();
        }

        // 가구가 다른 오브젝트와 겹쳤을 때 되돌리기 위해 기존 위치 저장
        originalPosition = transform.position;

        // 마우스 위치를 월드 좌표로 변환
        Vector3 worldPoint = cam.ScreenToWorldPoint(
            new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                cam.WorldToScreenPoint(transform.position).z
            )
        );

        // 클릭 위치와 오브젝트 중심 사이의 차이값 저장
        offset = transform.position - worldPoint;
        isDragging = true;
    }

    // 드래그 중 매 프레임 호출
    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 중이 아니거나 소유권이 없으면 이동 처리하지 않음
        if (!isDragging || !photonView.IsMine) return;

        // Floor 태그가 붙은 바닥 오브젝트 찾기
        GameObject floorObject = GameObject.FindGameObjectWithTag("Floor");

        if (floorObject != null)
        {
            Transform floorTransform = floorObject.transform;

            // 바닥의 회전값을 반영한 위쪽 방향 계산
            Vector3 floorUp = floorTransform.up;
            Vector3 floorCenter = floorTransform.position;

            // 바닥 윗면 위치 계산
            Vector3 floorTop = floorCenter + floorUp * (floorTransform.localScale.y / 2f);

            // 바닥 윗면을 기준으로 이동 가능한 평면 생성
            Plane floorPlane = new Plane(floorUp, floorTop);

            // 마우스 위치에서 3D 공간으로 Ray 발사
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Ray와 바닥 평면이 만나는 지점을 계산
            if (floorPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                // 바닥과 겹치지 않도록 약간 위로 띄워서 배치
                Vector3 targetPosition = hitPoint + floorUp * 0.01f;

                transform.position = targetPosition;
            }
        }
    }

    // 드래그 종료 시 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // 현재 위치에서 다른 가구와 겹치는지 검사
        Collider[] overlaps = Physics.OverlapBox(
            transform.position,
            transform.localScale * 0.49f,
            transform.rotation
        );

        foreach (Collider col in overlaps)
        {
            // 다른 Furniture 태그 오브젝트와 겹쳤다면 드래그 전 위치로 되돌림
            if (col.gameObject != this.gameObject && col.gameObject.CompareTag("Furniture"))
            {
                transform.position = originalPosition;
                return;
            }
        }
    }
}