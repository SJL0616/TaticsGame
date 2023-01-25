using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
 �ۼ��� : �̻���
 ����: FCanvas = ȭ�鿡�� �����̴� UI �Ŵ��� Ŭ����
       ��� 
           1. ��� ��ɽ� ���̴� UI ����
 ������ ������: 2023.01.04
 */
public class FCanvasCtrl : MonoBehaviour
{
    public Transform targetTr;    // UI Ÿ�� Transform
    public Vector3 offset;        // ��ġ ������ ���� Vector��
    private Camera cam;           // ī�޶�
    private Transform aimImg;     // aimImg Transform
    private Transform aimBoxImg;  // aimBoxImg Transform
    private Text aimBoxText;      // aimBoxImg ���� Text
    private GameObject aimBoxButton; // aimBoxImg ���� ��ư
    public bool isAiming;
 
    // Start is called before the first frame update
    void Start()
    {
        isAiming = false;
        cam = Camera.main;
        aimImg = transform.Find("AimImg");
        aimBoxImg = transform.Find("TextBox");
        aimBoxButton = aimBoxImg.transform.Find("Button").gameObject;
        aimBoxText = aimBoxImg.transform.Find("Text").GetComponent<Text>();

        aimImg.gameObject.SetActive(false);
        aimBoxImg.gameObject.SetActive(false);
    }

    // aimBoxText ����
    public void BoxTextSet(string info)
    {
        aimBoxText.text = info;
        if (info.Contains("����"))
        {
            aimBoxText.color = Color.green;
        }else if (info.Contains("������"))
        {
            aimBoxText.color = Color.red;
        }
        else
        {
            aimBoxText.color = Color.white;
        }
    }

    // aimBoxButton On/Off ����
    public void BoxButtonSetActive(bool isCansee)
    {
        if (!isCansee)
        {
            aimBoxButton.SetActive(false);
        }
        else
        {
            aimBoxButton.SetActive(true);
        }
    }

    // AimImg ����
    public void AimingImgSet(bool _isAiming, Transform _targetTr)
    {
        targetTr = _targetTr;
        if (_isAiming)
        {
            aimImg.gameObject.SetActive(true);
            aimBoxImg.gameObject.SetActive(true);
        }
        else
        {
            if (aimBoxImg.gameObject.activeSelf)
            {
                aimBoxImg.gameObject.SetActive(false);
            }
            if (aimImg.gameObject.activeSelf)
            {
                aimImg.gameObject.SetActive(false);
            }
        }
        isAiming = _isAiming;
    }

    // Update is called once per frame
    void Update()
    {
        // ��ݸ���� ��  Ÿ��(��) ��ġ�� ���󰡵��� UI ����
        if (isAiming)
        {
            Vector3 pos = cam.WorldToScreenPoint(targetTr.position);
            aimImg.transform.position = pos;
            aimBoxImg.transform.position = pos + offset;
        }

    }
}
