using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseChar : MonoBehaviour
{
    // ��� ����Ʈ ����
    private List<Node> m_path = new List<Node>();   // ��ã��� Node �迭
    int idx;                                        
    private PlayerKeyCtrl playerCtrl; 
    private EnemyCtrl enemyCtrl;
    private Transform playerBody;

    private void Awake()
    {
        if(transform.root.name == "Player")
        {
            playerCtrl = GetComponent<PlayerKeyCtrl>();
            enemyCtrl = null;
            playerBody = transform.Find("CharacterBody");
        }
        else
        {
            enemyCtrl = transform.GetComponentInChildren<EnemyCtrl>();
            playerCtrl = null;
            playerBody = transform.Find("CharacterBody");
        }
    }
    private void Start()
    {
        idx = 0;
    }

    // �ɸ��ͺ� �ִϸ��̼� ���� �Լ�
    private void MoveAnimCtrl(bool isMoving)
    {
        if (playerCtrl != null) 
        {
            playerCtrl.MoveToward(isMoving);
        }
        else
        {
            int num = isMoving ? 1 : 0;
            enemyCtrl.ModeSet(num);
        }
    }


    // ���޵� ����Ʈ�� �ɹ� ����Ʈ�� �ʱ�ȭ �ϴ� �Լ�
    public void SetPath(List<Node> path)
    {
        // ���޵� �н�(����Ʈ)�� ���ٸ� ����
        if (path == null)
            return;

        // �ϴ� �ɹ� �н�(����Ʈ)�� Ŭ����
        m_path.Clear();

        // ���޵� �н��� Node�� ��� �н�(����Ʈ)�� �߰�
        foreach (Node p in path)
        {
            m_path.Add(p);
        }
    }

    private void Update()
    {
        // ����, ��� �н�(����Ʈ)�� ����Ÿ�� 0 �ʰ���� ��, ����Ÿ�� ���� �Ѵٸ�.
        if (m_path.Count > 0)
        {
            Vector3 targetPos = new Vector3(m_path[idx].m_turnPoint.transform.position.x, transform.position.y, m_path[idx].m_turnPoint.transform.position.z);
            // ���� ���� ����
            // ù ��° ����Ʈ�� ��ġ����[��, ó������ �ܰ躰�� �̵�] �� �ڽ��� ��ġ�� �� ���ΰ��� �̵� ������ �ȴ�.
            Vector3 dir = targetPos - transform.position;
            // ���� ����ȭ
            dir.Normalize();

            // �������� �̵�
            transform.Translate(dir * 0.2f);
            MoveAnimCtrl(true);
            // Ÿ�� �������� ȸ��
            playerBody.localRotation = Quaternion.Slerp(playerBody.localRotation, Quaternion.LookRotation(dir),  Time.deltaTime* 5.0f);
 
            // �������� ������ �Ÿ��� ������
            float distance = Vector3.Distance(targetPos, transform.position);

            // �������� ���� �ߴٸ� ��ã�� �迭 �ʱ�ȭ, ������
            if (distance < 0.1f)
            {
                idx++;
                if (idx >= m_path.Count)
                {
                    playerBody.localRotation = Quaternion.LookRotation(dir);
                    m_path.Clear();
                    idx = 0;
                    GameObject.Find("StageManager").GetComponent<StageManager>().Reset();
                    MoveAnimCtrl(false);
                }
            }
        }
    }
}
