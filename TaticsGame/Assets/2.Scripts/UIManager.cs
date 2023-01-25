using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 �ۼ��� : �̻���
 ����: UI �Ŵ��� Ŭ����
       ��� 
           1. ���� �ǳ� UI ��Ʈ��(SetActive(true / false), Text ����)
 ������ ������: 2023.01.06
 */
public class UIManager : MonoBehaviour
{
    private GameObject gamePanel;  // ���� ������ ���̴� UI
    private GameObject turnPanel;  // ������ ������ �����ִ� UI
    private GameObject MagazinePanel; // ���� �ϴ� ���� ź�� ���� �����ִ� UI
    private Text state;            // ���� ��� ���� ���¸� �˷��ִ� UI

    // Start is called before the first frame update
    void Start()
    {
        gamePanel = transform.Find("MainPanel").Find("GamePanel").gameObject;  
        gamePanel.SetActive(false);
        turnPanel = transform.Find("MainPanel").Find("TurnPanel").gameObject;
        MagazinePanel = transform.Find("MainPanel").Find("MagazineState").gameObject;
        state = transform.Find("MainPanel").Find("State").Find("Text").GetComponent<Text>();
    }

    // ���� �ǳ� ���� �Լ�
    public void GamePanelSet(string showText = "")
    {
        if(showText == "") { gamePanel.SetActive(false); return; }
        gamePanel.transform.Find("Img").transform.Find("Text").GetComponent<Text>().text = showText;
        gamePanel.SetActive(true);
    }

    // ������ ������ �����ִ� turnPanel ���� �Լ�
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

    // �����ϴ� ���� ź���� �ִ��� ������ TRUE / FALSE ��ȯ �Լ�
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
        if (!isMagazineLeft) { SetStateText("���� �Ѿ��� �����ϴ�. ������ �Ͻʽÿ�."); }
        return isMagazineLeft;
    }

    // ���� �Լ�
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
