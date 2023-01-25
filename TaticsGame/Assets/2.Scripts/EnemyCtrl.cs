using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Rand = UnityEngine.Random;
/*
 작성자 : 이상준
 내용: 적 컨트롤 클래스
       기능 
           1. 턴 실행, 초기화
           2. 패트롤, 공격, 죽음 기능
 마지막 수정일: 2023.01.06
 */
[RequireComponent(typeof(AudioSource))]
public class EnemyCtrl : MonoBehaviour
{
    private Animator anim;
    private StageManager stageManager;
    private TileController tileController;
    private TileController tileCtrl;
    private Node startNode;
    public Node[] roamingCheckPoints;           // 패트롤 포인트 저장용 배열
    private int roamidx;                        // 위 배열에 넣을 index 변수
    private Node roamingTarget;                 // 패트롤 타겟
    [SerializeField] private Vector3 patrolPos1;
    [SerializeField] private Vector3 patrolPos2;
    [HideInInspector]
    public bool isDie;
    public enum MODE_STATE { IDLE = 1, MOVE, SURPRISE, TRACE, ATTACK, HIT, EAT, SLEEP, DIE };
    public enum MODE_KIND { ENEMY1 = 1, ENEMY2, ENEMYBOSS };

    [Header("STATE")]
    [Space(20)]
    public MODE_STATE enemyMode = MODE_STATE.IDLE;
    [Header("SETTING")]
    public MODE_KIND enemyKind = MODE_KIND.ENEMY1;
   
    public LayerMask playerMask;                 // 적 콜라이더 저장용 변수
    public float viewRadius;                     // 공격 범위
    private Node playerNode;                     // 플레이어 노드
    public GameObject _player; 
    public GameObject player { 
        set {
            if(_player == null && value != null)
            {
                PlaySound(2);
            }
            _player = value; 
        }
        get { return _player; } 
    }
    private Transform playerBody;                // 플레이어 몸 TR
    public AudioClip[] clips; 
    private AudioSource audioSource;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        isDie = false;
        stageManager = GameObject.FindObjectOfType<StageManager>();
        tileController = GameObject.Find("Tile").GetComponent<TileController>();
        roamingCheckPoints = new Node[2];
        tileCtrl = GameObject.Find("Tile").GetComponent<TileController>();
        playerBody = gameObject.transform;
        playerNode = null;
        player = null;
        audioSource = GetComponent<AudioSource>();
    }
    
  
    void Start()
    {
        Debug.Log("Start");
        StartCoroutine(this.SetPatrolNode(2.0f));
    }

    // 패트롤 포인트 위치값으로 노드를 찾아서 배열에 추가.
    IEnumerator SetPatrolNode(float delay)
    {
        yield return new WaitForSeconds(delay);
        Node oneNode = tileController.FindNode(patrolPos1);
        Debug.Log(oneNode.m_row);
        roamingCheckPoints[0] = tileController.FindNode(patrolPos1);
        roamingCheckPoints[1] = tileController.FindNode(patrolPos2);
    }

    // 패트롤 모드 함수
    public IEnumerator StartPatrolMode()
    {
       if (isDie) {yield break;}
       yield return null;
        //자신의 위치에 있는 노드를 가져와 시작 노드로 설정.
       Vector3 myPos = new Vector3(transform.root.position.x, 0, transform.root.position.z);
       startNode = tileController.FindNode(myPos);

       DetectPlayer(player);
       if (IsAtackReach()) yield break;
       if(player != null)
        {
            // 플레이어 노드 주위의 노드중에 자신에게서 가장 가까운 노드를 찾아서 플레이어 노드로 설정.
            Node[] neighbours = tileCtrl.Neighbours(playerNode);
            int[] cost = new int[neighbours.Length];
            for(int i = 0; i<neighbours.Length; i++)
            {
                cost[i] = tileCtrl.GetDistance(startNode, neighbours[i]);
            }

            for(int j = 0; j<cost.Length-1; j++)
            {
                for(int x = j+1; x < cost.Length; x++)
                {
                    if (cost[j] > cost[x])
                    {
                        int temp = cost[j];
                        cost[j] = cost[x];
                        cost[x] = temp;
                    }
                }
            }
            int idxNum = cost[0];
            for (int i = 0; i < neighbours.Length; i++)
            {
                int oneCost = tileCtrl.GetDistance(startNode, neighbours[i]);
                if(oneCost == idxNum)
                {
                    playerNode = neighbours[i];
                }
            }
            // 길찾기 코루틴 실행
            StartCoroutine(tileController.FindPathCoroutine(transform.root.GetComponent<BaseChar>(), startNode, playerNode, true));
        }
        else
        {
            Debug.Log("Patrolling");
            int nextIdx = Random.Range(0, 2);
            if (roamidx != nextIdx)
            {
                roamidx = nextIdx;
                roamingTarget = roamingCheckPoints[roamidx];
                StartCoroutine(tileController.FindPathCoroutine(transform.root.GetComponent<BaseChar>(), startNode, roamingTarget, true));
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
                stageManager.Reset();
            }
        }
    }

    // 범위내의 플레이어 오브젝트를 찾는 함수
    public void DetectPlayer(GameObject _player = null)
    {
        if(player == null)
        { // 플레이어를 찾은 상태가 아니라면 범위내의 플레이어 오브젝트를 찾아서 가져옴
            Collider[] targetArray = Physics.OverlapSphere(transform.position, viewRadius, playerMask);
            if (targetArray.Length != 0)
            {
                GameObject playerObj = targetArray[0].gameObject;
                player = playerObj;
                Vector3 playerPos = new Vector3(playerObj.transform.position.x, 0, playerObj.transform.position.z);
                playerNode = tileCtrl.FindNode(playerPos);
            }
        }
        else
        { // 이미 플레이어를 찾은 상태라면 플레이어의 노드를 가져옴
            Debug.Log("Player Trace");
            GameObject playerObj = player;
            Vector3 playerPos = new Vector3(playerObj.transform.position.x, 0, playerObj.transform.position.z);
            playerNode = tileCtrl.FindNode(playerPos);
            Debug.Log("Node Row :" + playerNode.Row + " , Col : " + playerNode.Col);
        }
    }

    // 공격 범위내에 플레이어가 있다면 공격 후 턴 종료함수 실행
    public bool IsAtackReach()
    {
        bool isAttackable = false;
        if (player == null) return isAttackable;

        float dis = tileCtrl.GetDistance(startNode, playerNode);
        if ((startNode.m_row == playerNode.m_row && startNode.m_col == playerNode.m_col) || dis <= 20.0f)
        {
            Debug.Log("Attack Player");
            StartCoroutine(this.AttackPlayer());
            isAttackable = true;
        }
        return isAttackable;
    }

    // 근접 공격 함수
    IEnumerator AttackPlayer()
    {
        //공격 애니메이션 , 데미지 부여, 플레이어 방향으로 몸 회전 로직
        yield return null;
        anim.SetTrigger("Attack");
        PlaySound(0);
        Vector3 targetPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        Vector3 dir = targetPos - transform.position;
        dir.Normalize();
        float startTime = Time.time;
        while (Time.time <= startTime + 2.0f)
        {
            yield return null;
            playerBody.localRotation = Quaternion.Slerp(playerBody.localRotation, Quaternion.LookRotation(dir), Time.deltaTime * 5.0f);
        }
        yield return null;
        object[] _paramas = new object[3];
        _paramas[0] = player.transform.position;
        _paramas[1] = 30;
        _paramas[2] = this.gameObject;

        player.SendMessage("OnCollision", _paramas, SendMessageOptions.DontRequireReceiver);
        yield return new WaitForSeconds(2.5f);
        stageManager.Reset();
    }
    
    //사운드 클립 배열에서 사운드 재생 함수
    void PlaySound(int num)
    {
        AudioClip oneClip = clips[num];
        if(oneClip != null && !audioSource.isPlaying)
        {
            audioSource.clip = oneClip;
            audioSource.Play();
        }
    }

    // 애니메이션 실행에 쓰이는 모드 ENUM변수 변경 함수
    public void ModeSet(int num)
    {

        switch (num)
        {
            case 0:
                enemyMode = MODE_STATE.IDLE;
                break;
            case 1:
                enemyMode = MODE_STATE.MOVE;
                break;
            case 2:
                enemyMode = MODE_STATE.ATTACK;
                break;
            case 4:
                enemyMode = MODE_STATE.DIE;
                break;
        }
        ModeAction();
    }

    public void ModeAction()
    {
            switch (enemyMode)
            {
                case MODE_STATE.IDLE:
                anim.SetFloat("Move", 0);
                    break;
                case MODE_STATE.TRACE:
                    anim.SetTrigger("Walk");
                    break;
                case MODE_STATE.ATTACK:
                    anim.SetTrigger("Attack");
                    break;
                case MODE_STATE.MOVE:
                anim.SetFloat("Move", 1);
                break;
                case MODE_STATE.DIE:
                anim.SetTrigger("Die");
                break;
        }
    }

    public void isDead()
    {
        StartCoroutine(this.Die());
    }

    IEnumerator Die()
    {
        isDie = true;
        PlaySound(1);
        anim.SetTrigger("Die");
        enemyMode = MODE_STATE.DIE;
        this.gameObject.transform.Find("EnemyBody").tag = "Untagged";
        foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        yield return null;
    }

}
