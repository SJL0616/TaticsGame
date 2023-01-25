using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
/*
 �ۼ��� : �̻���
 ����: Cinemachine Virtual Camera ��Ʈ�� Ŭ����
       ��� 
           1. Virtual Camera ��ȯ
           2. ī�޶� Ȯ��, ���
 ������ ������: 2023.01.06
 */
public class VcamCtrl : MonoBehaviour
{
    public CinemachineVirtualCamera vcamOverWorld;
    public CinemachineVirtualCamera vcamAim;
    public float speed;

    // ī�޶� ��ȯ �Լ�
    public void SwitchVCam(int num, Transform target = null)
    {
        switch (num)
        {
            case 1: // ��ü �� ī�޶�
                vcamOverWorld.Priority = 1;
                vcamAim.Priority = 0;
                break;
            case 2: // ��� ���� ī�޶�
                vcamOverWorld.Priority = 0;
                vcamAim.Priority = 1;
                vcamAim.LookAt = target;
                break;
        }
    }
    
    private void Update()
    { 
        // �÷��̾� ���� �� ���콺�ٷ� Ȯ��, ���
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
