using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 작성자 : 이상준
 내용: 스테이지 매니저 클래스
       기능 
           1. 게임이 시작되면 바닥에 이동에 필요한 노드(Node) 타일을 생성하는 함수 호출.
           2. 플레이어, 적의 턴(turn)을 초기화하고 순차적으로 실행, 관리, 초기화
           3. 플레이어나 적이 모두 죽으면 게임 오버 함수 호출.
 마지막 수정일: 2023.01.06
 */

public class StageManager : MonoBehaviour
{
    private Camera mainCamera;           // 메인 카메라
    private UIManager uiManager;         // UI매니저 클래스 변수
    private TileController tileCtrl;     // 타일 컨트롤러 클래스 변수
    private IEnumerator findPathCor;     // 길찾기 코루틴 변수
    private IEnumerator turnCoroutine;   // 턴 코루틴 변수
    private bool isPlayerTurn;           // 플레이어 턴인지 아닌지 확인용 bool 형 변수
    private bool isGameStarted;             // 
    public LayerMask nodeMask;           // 노드 레이어
    public LayerMask playerMask;         // 플레이어 레이어

    private Node targetNode;             // 타겟 노드
    private Node playerNode;             // 플레이어 노드
    private GameObject player;           // 플레이어 오브젝트
    public Vector3 playerPos;            // 플레이어 위치
    
    public GameObject enemyPrefab;       // 적 프리펍
    private EnemyCtrl[] enemys;          // 적 오브젝트 저장용 배열
    public Vector3 enemyPos;             // 적 위치
    private int leftEnemy;               // 남은 적 수 측정용 int형 변수
    private int leftEnemyTurn;           // 남은 적의 턴 수 측정용 int형 변수
    private int nextIdx;                 // 적의 다음 배열의 index용 int형 변수

    private void Awake()
    {   // 변수 초기화
        isGameStarted = false;
        findPathCor = null;
        turnCoroutine = null;
        isPlayerTurn = true;

        tileCtrl = GameObject.Find("Tile").GetComponent<TileController>(); 
        player = GameObject.Find("Player");
        uiManager = GameObject.FindObjectOfType<UIManager>();
        enemys = GameObject.FindObjectsOfType<EnemyCtrl>();
        if(enemys.Length > 0)
        {
            leftEnemy = enemys.Length;
            nextIdx = 0;
        }
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        isGameStarted = tileCtrl.TileSet(); // 타일 컨트롤러가 바닥에 타일 노드를 생성하면 게임이 시작됨
        yield return new WaitForSeconds(0.2f);
        if (isGameStarted)                  
        {
            mainCamera = Camera.main;
            SetTurn(true);                  // 턴 실행 함수
        }
    }

    // 플레이어 혹은 적 턴(Turn) 컨트롤하는 함수
    public void SetTurn(bool _isPlayerTurn)
    {
        isPlayerTurn = _isPlayerTurn; // bool 형 변수에 따라서 true면 플레이어턴, false면 적턴이 실행됨.
        if(turnCoroutine != null)     // 하나의 턴만 실행되도록 실행중인 코루틴 종료 후 코루틴 변수 초기화
        {
            StopCoroutine(turnCoroutine);
            turnCoroutine = null;
        }

        if (isPlayerTurn)
        {   
            turnCoroutine = uiManager.ShowTurn("PlayerTurn"); // 화면 UI 표시
            StartCoroutine(turnCoroutine);
            uiManager.SetStateText("플레이어 턴");
            player.GetComponent<PlayerKeyCtrl>().ResetTurn(); // 플레이어 턴 초기화 함수 실행
        }
        else
        {
            turnCoroutine = uiManager.ShowTurn("EnemyTurn"); // 화면 UI 표시
            StartCoroutine(turnCoroutine);
            uiManager.SetStateText("적 턴");
            
            foreach (EnemyCtrl enemy in enemys)
            {
                if (!enemy.isDie)
                {
                    nextIdx++;                              // 다음 적 선택 후 턴 메서드 실행.
                    StartCoroutine(enemy.StartPatrolMode());
                    break;
                }
            }
        }
    }

    //턴 리셋 함수
    public void Reset()
    {
        findPathCor = null;                         // 길찾기 코루틴 변수 null 설정
        tileCtrl.ResetNode();                       // 타일 노드의 값 초기화

        if (IsPlayerOrEnemyDead())                  // 플레이어 혹은 적이 모두 죽었으면 게임 종료
        {
            isGameStarted = false;
            uiManager.GamePanelSet("GameOver");
            Time.timeScale = 0;
            return;
        }

        if (!isPlayerTurn && leftEnemyTurn > 1)     // 적의 턴일 때 순차적으로 다음 적의 턴 실행
        {
            leftEnemyTurn--;
            StartCoroutine(enemys[nextIdx].StartPatrolMode());
            return;
        }
        nextIdx = 0;                               // 모든 턴이 끝났을 때 적 배열의 Index, 남은 적 턴 초기화
        leftEnemyTurn = leftEnemy;
        SetTurn(!isPlayerTurn);                    // 턴 재실행
    }

    //플레이어 혹은 적이 죽은 상태인지 체크하는 함수
    bool IsPlayerOrEnemyDead()
    {
        bool isDead = false;
        if (player.GetComponent<PlayerKeyCtrl>().isDie)
        {
            isDead = true;
            return isDead;
        }
        foreach (EnemyCtrl enemy in enemys)
        {
            if (enemy.isDie && enemy.gameObject.CompareTag("Enemy"))
            {
                Debug.Log("Enemy Dead");
                leftEnemy--;
                enemy.gameObject.tag = "Untagged";
                if(leftEnemy <= 0)
                {
                    isDead = true;
                    return isDead;
                }
            }
        }
        return isDead;       //한쪽이라도 죽었다면 true 반환
    }

    // 게임을 재시작하는 함수
    IEnumerator ReStartGame()
    {
        isGameStarted = true;
        uiManager.GamePanelSet("");
        player.GetComponent<PlayerKeyCtrl>().Restart();
        enemys = GameObject.FindObjectsOfType<EnemyCtrl>();
        foreach (EnemyCtrl enemy in enemys)
        {
            if (enemy.isDie)
            {
                Debug.Log("Enemy Dead");
                Destroy(enemy.transform.root.gameObject);
                Instantiate(enemyPrefab, enemyPos, Quaternion.identity);  //적 프리펍을 사용하여 적 오브젝트 재생성
            }
        }
        yield return new WaitForSeconds(2.0f);
        enemys = GameObject.FindObjectsOfType<EnemyCtrl>();
        Debug.Log("enemys Length : " + enemys.Length);
        leftEnemy = enemys.Length;
        leftEnemyTurn = 0;
        
        isPlayerTurn = false;
        Invoke("Reset", 2.0f);
    }

    // Update is called once per frame
    void Update()
    {

        if (isGameStarted)   //게임이 시작된 상태일때만 실행됨
        {
            //캐릭터를 왼쪽마우스 클릭 -> 이동할 노드 선택할 수 있도록 노드 색을 바꾸는 로직
            if (Input.GetMouseButtonUp(0) && isPlayerTurn && !player.GetComponent<PlayerKeyCtrl>().menuSelected)
            {
                // 마우스로 찍은 위치의 좌표 값을 가져온다
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 10000f, playerMask))
                {
                    GameObject playerObj = hit.collider.gameObject;
                    Vector3 playerPos = new Vector3(playerObj.transform.position.x, 0, playerObj.transform.position.z);
                    playerNode = tileCtrl.FindNode(playerPos);
                    Debug.Log("Node Row :" + playerNode.Row + " , Col : " + playerNode.Col);
                    if (!tileCtrl.isPathColored())
                    {
                        uiManager.SetStateText("이동할 위치를 선택해주세요.");
                        player.GetComponent<PlayerKeyCtrl>().menuSelected = true;
                        tileCtrl.ShowWalkablePath(playerNode);
                    }
                }
            }

            // 활성화된 노드를 마우스 오른쪽 버튼으로 클릭시 해당 노드로 이동하는 로직
            if (Input.GetMouseButtonUp(1) && isPlayerTurn)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 10000f, nodeMask))
                {
                    targetNode = hit.collider.gameObject.GetComponent<Node>();
                    if (targetNode.GetColor() == new Color(11 / 255f, 255 / 255f, 255 / 255f, 0.2f) && findPathCor == null)
                    {
                        uiManager.SetStateText("선택한 위치로 이동합니다.");
                        findPathCor = tileCtrl.FindPathCoroutine(player.GetComponent<BaseChar>(), playerNode, targetNode, false);
                        StartCoroutine(findPathCor);
                    }
                }
            }

            // Esc 버튼을 통해 기능 선택 초기화
            if (Input.GetKeyDown(KeyCode.Escape) && isPlayerTurn)
            {
                player.GetComponent<PlayerKeyCtrl>().ResetTurn();
            }
        }
        
        //게임이 끝났을 때 화면 클릭을 통해 게임 재시작
        if (Input.GetMouseButtonDown(0) && Time.timeScale == 0 && !isGameStarted)
        {
            Time.timeScale = 1;
            StartCoroutine(this.ReStartGame());
        }
    }



}
