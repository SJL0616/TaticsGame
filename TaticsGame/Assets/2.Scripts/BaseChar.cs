using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseChar : MonoBehaviour
{
    // 노드 리스트 선언
    private List<Node> m_path = new List<Node>();   // 길찾기용 Node 배열
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

    // 케릭터별 애니메이션 실행 함수
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


    // 전달된 리스트로 맴버 리스트를 초기화 하는 함수
    public void SetPath(List<Node> path)
    {
        // 전달된 패스(리스트)가 없다면 리턴
        if (path == null)
            return;

        // 일단 맴버 패스(리스트)는 클리어
        m_path.Clear();

        // 전달된 패스의 Node를 멤버 패스(리스트)에 추가
        foreach (Node p in path)
        {
            m_path.Add(p);
        }
    }

    private void Update()
    {
        // 만약, 멤버 패스(리스트)의 데이타가 0 초과라면 즉, 데이타가 존재 한다면.
        if (m_path.Count > 0)
        {
            Vector3 targetPos = new Vector3(m_path[idx].m_turnPoint.transform.position.x, transform.position.y, m_path[idx].m_turnPoint.transform.position.z);
            // 방향 벡터 설정
            // 첫 번째 리스트의 위치에서[즉, 처음부터 단계별로 이동] 내 자신의 위치를 뺀 벡턱값이 이동 방향이 된다.
            Vector3 dir = targetPos - transform.position;
            // 벡터 정규화
            dir.Normalize();

            // 방향으로 이동
            transform.Translate(dir * 0.2f);
            MoveAnimCtrl(true);
            // 타겟 방향으로 회전
            playerBody.localRotation = Quaternion.Slerp(playerBody.localRotation, Quaternion.LookRotation(dir),  Time.deltaTime* 5.0f);
 
            // 목적지와 나와의 거리를 가져옴
            float distance = Vector3.Distance(targetPos, transform.position);

            // 목적지에 도착 했다면 길찾기 배열 초기화, 턴종료
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
