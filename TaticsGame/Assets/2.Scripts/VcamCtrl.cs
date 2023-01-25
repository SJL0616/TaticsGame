using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
/*
 작성자 : 이상준
 내용: Cinemachine Virtual Camera 컨트롤 클래스
       기능 
           1. Virtual Camera 전환
           2. 카메라 확대, 축소
 마지막 수정일: 2023.01.06
 */
public class VcamCtrl : MonoBehaviour
{
    public CinemachineVirtualCamera vcamOverWorld;
    public CinemachineVirtualCamera vcamAim;
    public float speed;

    // 카메라 전환 함수
    public void SwitchVCam(int num, Transform target = null)
    {
        switch (num)
        {
            case 1: // 전체 뷰 카메라
                vcamOverWorld.Priority = 1;
                vcamAim.Priority = 0;
                break;
            case 2: // 사격 모드용 카메라
                vcamOverWorld.Priority = 0;
                vcamAim.Priority = 1;
                vcamAim.LookAt = target;
                break;
        }
    }
    
    private void Update()
    { 
        // 플레이어 턴일 시 마우스휠로 확대, 축소
        if(vcamOverWorld.Priority == 1)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel") * speed;

            if (scroll != 0)
            {
                if (vcamOverWorld.m_Lens.FieldOfView <= 20.0f && scroll < 0)
                {
                    vcamOverWorld.m_Lens.FieldOfView = 20.0f;
                }
                else if (vcamOverWorld.m_Lens.FieldOfView >= 60.0f && scroll > 0)
                {
                    vcamOverWorld.m_Lens.FieldOfView = 60.0f;
                }
                else
                {
                    vcamOverWorld.m_Lens.FieldOfView += scroll;
                }
            }
        }
    }
}
