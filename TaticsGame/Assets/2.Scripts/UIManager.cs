using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 작성자 : 이상준
 내용: UI 매니저 클래스
       기능 
           1. 각종 판넬 UI 컨트롤(SetActive(true / false), Text 설정)
 마지막 수정일: 2023.01.06
 */
public class UIManager : MonoBehaviour
{
    private GameObject gamePanel;  // 게임 오버시 보이는 UI
    private GameObject turnPanel;  // 누구의 턴인지 보여주는 UI
    private GameObject MagazinePanel; // 우측 하단 남은 탄약 수를 보여주는 UI
    private Text state;            // 우측 상단 현재 상태를 알려주는 UI

    // Start is called before the first frame update
    void Start()
    {
        gamePanel = transform.Find("MainPanel").Find("GamePanel").gameObject;  
        gamePanel.SetActive(false);
        turnPanel = transform.Find("MainPanel").Find("TurnPanel").gameObject;
        MagazinePanel = transform.Find("MainPanel").Find("MagazineState").gameObject;
        state = transform.Find("MainPanel").Find("State").Find("Text").GetComponent<Text>();
    }

    // 게임 판넬 설정 함수
    public void GamePanelSet(string showText = "")
    {
        if(showText == "") { gamePanel.SetActive(false); return; }
        gamePanel.transform.Find("Img").transform.Find("Text").GetComponent<Text>().text = showText;
        gamePanel.SetActive(true);
    }

    // 누구의 턴인지 보여주는 turnPanel 제어 함수
    public IEnumerator ShowTurn(string turnName)
    {
        yield return null;
        GameObject activeObj = null;
        for (int i = 0; i < turnPanel.transform.childCount; i++)
        {
            yield return null;
            if (turnPanel.transform.GetChild(i).name == turnName)
            {
                activeObj = turnPanel.transform.GetChild(i).gameObject;
                activeObj.SetActive(true);
            }
            else
            {
                turnPanel.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        yield return new WaitForSeconds(2.5f);
        activeObj.SetActive(false);
        activeObj = null;

    }

    // 우측하단 남은 탄약이 있는지 없는지 TRUE / FALSE 반환 함수
    public bool CheckMagazine(bool useMagazine = false)
    {
        GameObject leftMagazineImg = MagazinePanel.transform.Find("LeftMagazine").gameObject;
        bool isMagazineLeft = false;
        for (int i = 0; i < leftMagazineImg.transform.childCount; i++)
        {
            Image oneMagazine = leftMagazineImg.transform.GetChild(i).gameObject.GetComponent<Image>();
            if (oneMagazine.color == new Color(1, 1, 1, 1))
            {
                if (useMagazine)
                {
                    oneMagazine.color = new Color(1, 1, 1, 0);
                }
                isMagazineLeft = true;
                break;
            }
        }
        if (!isMagazineLeft) { SetStateText("남은 총알이 없습니다. 재장전 하십시오."); }
        return isMagazineLeft;
    }

    // 장전 함수
    public void ReloadMagazine()
    {
        GameObject leftMagazineImg = MagazinePanel.transform.Find("LeftMagazine").gameObject;
        for (int i = 0; i < leftMagazineImg.transform.childCount; i++)
        {
            Image oneMagazine = leftMagazineImg.transform.GetChild(i).gameObject.GetComponent<Image>();
            if (oneMagazine.color == new Color(1, 1, 1, 0))
            {
                //leftMagazineImg.transform.GetChild(i).gameObject.SetActive(false);
                oneMagazine.color = new Color(1, 1, 1, 1);
            }
        }
    }

    public void SetStateText(string nowState)
    {
        state.text = nowState;
    }

}
