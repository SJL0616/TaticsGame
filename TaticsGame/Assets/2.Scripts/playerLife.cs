using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 �ۼ��� : �̻���
 ����: �÷��̾� ������ ��Ʈ�� Ŭ����
       ��� 
           1. ������ �Դ� ����
           2. �ٴڿ� ���ڱ� ����
 ������ ������: 2023.01.06
 */
public class playerLife : MonoBehaviour
{
    private int life = 100;        // ��ü ������ ����Ʈ : 100
    private Transform myTr;        // �ڽ��� Transform ����
    public GameObject bloodEffet;  // ���� ȿ���� GameObj
    public Transform bloodDecal;   // ���� ȿ���� TR
    public EnemyCtrl enemyCtrl;    // �� ��Ʈ�ѷ� ��ũ��Ʈ
    public SpriteRenderer lifeBar; // UI LifeBar ��Ʈ�ѿ� ����
    public PlayerKeyCtrl playerCtrl; //�÷��̾� ��Ʈ�� ����

    // Start is called before the first frame update
    void Start()
    {
        myTr = GetComponent<Transform>();
        playerCtrl = GetComponent<PlayerKeyCtrl>();
    }

    // ������ �Դ� �Լ�
    void OnCollision(object[] _params)
    {
        Debug.Log(string.Format("info {0} : {1}", _params[0], _params[1]));
        StartCoroutine(this.CreateBlood(transform.position));
        life -= (int)_params[1];
        LifeBarSet(life);
        if (life <= 0)
        {
            playerCtrl.isDead();
        }
    }
    // UI �������� ���� �Լ�
    public void LifeBarSet(int input)
    {
        lifeBar.material.SetFloat("_Progress", input / 100f);
    }

    // �ڽ��� �� �Ʒ��� ���ڱ� ���� �Լ�
    IEnumerator CreateBlood(Vector3 pos)
    {
        GameObject enemyBlood = Instantiate(bloodEffet, pos, Quaternion.identity) as GameObject;
        Destroy(enemyBlood, 1.5f);

        Quaternion decalRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
        float scale = Random.Range(1.0f, 2.5f);

        Transform enemyBlood2 = Instantiate(bloodDecal, myTr.position, decalRot) as Transform;
        enemyBlood2.localScale = Vector3.one * scale;
        Destroy(enemyBlood2.gameObject, 1.5f);
        yield return null;

    }
}
