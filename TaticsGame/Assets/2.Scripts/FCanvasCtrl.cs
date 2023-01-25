using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
 작성자 : 이상준
 내용: FCanvas = 화면에서 움직이는 UI 매니저 클래스
       기능 
           1. 사격 기능시 보이는 UI 제어
 마지막 수정일: 2023.01.04
 */
public class FCanvasCtrl : MonoBehaviour
{
    public Transform targetTr;    // UI 타겟 Transform
    public Vector3 offset;        // 위치 조정용 임의 Vector값
    private Camera cam;           // 카메라
    private Transform aimImg;     // aimImg Transform
    private Transform aimBoxImg;  // aimBoxImg Transform
    private Text aimBoxText;      // aimBoxImg 안의 Text
    private GameObject aimBoxButton; // aimBoxImg 안의 버튼
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

    // aimBoxText 제어
    public void BoxTextSet(string info)
    {
        aimBoxText.text = info;
        if (info.Contains("명중"))
        {
            aimBoxText.color = Color.green;
        }else if (info.Contains("빗나감"))
        {
            aimBoxText.color = Color.red;
        }
        else
        {
            aimBoxText.color = Color.white;
        }
    }

    // aimBoxButton On/Off 제어
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

    // AimImg 제어
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
        // 사격모드일 때  타겟(적) 위치를 따라가도록 UI 제어
        if (isAiming)
        {
            Vector3 pos = cam.WorldToScreenPoint(targetTr.position);
            aimImg.transform.position = pos;
            aimBoxImg.transform.position = pos + offset;
        }

    }
}
