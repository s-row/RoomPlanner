using UnityEngine;

// 가구의 원본 크기를 저장하는 스크립트
// 크기 조절 시 기준값으로 사용하기 위해 가구 오브젝트에 부착
public class FurnitureInfo : MonoBehaviour
{
    public Vector3 originalScale; // 가구의 초기 크기 저장

    void Start()
    {
        // 게임 시작 시 현재 가구의 localScale 값을 원본 크기로 저장
        originalScale = transform.localScale;
    }
}