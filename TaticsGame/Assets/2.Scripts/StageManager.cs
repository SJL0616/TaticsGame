using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 �ۼ��� : �̻���
 ����: �������� �Ŵ��� Ŭ����
       ��� 
           1. ������ ���۵Ǹ� �ٴڿ� �̵��� �ʿ��� ���(Node) Ÿ���� �����ϴ� �Լ� ȣ��.
           2. �÷��̾�, ���� ��(turn)�� �ʱ�ȭ�ϰ� ���������� ����, ����, �ʱ�ȭ
           3. �÷��̾ ���� ��� ������ ���� ���� �Լ� ȣ��.
 ������ ������: 2023.01.06
 */

public class StageManager : MonoBehaviour
{
    private Camera mainCamera;           // ���� ī�޶�
    private UIManager uiManager;         // UI�Ŵ��� Ŭ���� ����
    private TileController tileCtrl;     // Ÿ�� ��Ʈ�ѷ� Ŭ���� ����
    private IEnumerator findPathCor;     // ��ã�� �ڷ�ƾ ����
    private IEnumerator turnCoroutine;   // �� �ڷ�ƾ ����
    private bool isPlayerTurn;           // �÷��̾� ������ �ƴ��� Ȯ�ο� bool �� ����
    private bool isGameStarted;             // 
    public LayerMask nodeMask;           // ��� ���̾�
    public LayerMask playerMask;         // �÷��̾� ���̾�

    private Node targetNode;             // Ÿ�� ���
    private Node playerNode;             // �÷��̾� ���
    private GameObject player;           // �÷��̾� ������Ʈ
    public Vector3 playerPos;            // �÷��̾� ��ġ
    
    public GameObject enemyPrefab;       // �� ������
    private EnemyCtrl[] enemys;          // �� ������Ʈ ����� �迭
    public Vector3 enemyPos;             // �� ��ġ
    private int leftEnemy;               // ���� �� �� ������ int�� ����
    private int leftEnemyTurn;           // ���� ���� �� �� ������ int�� ����
    private int nextIdx;                 // ���� ���� �迭�� index�� int�� ����

    private void Awake()
    {   // ���� �ʱ�ȭ
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
        isGameStarted = tileCtrl.TileSet(); // Ÿ�� ��Ʈ�ѷ��� �ٴڿ� Ÿ�� ��带 �����ϸ� ������ ���۵�
        yield return new WaitForSeconds(0.2f);
        if (isGameStarted)                  
        {
            mainCamera = Camera.main;
            SetTurn(true);                  // �� ���� �Լ�
        }
    }

    // �÷��̾� Ȥ�� �� ��(Turn) ��Ʈ���ϴ� �Լ�
    public void SetTurn(bool _isPlayerTurn)
    {
        isPlayerTurn = _isPlayerTurn; // bool �� ������ ���� true�� �÷��̾���, false�� ������ �����.
        if(turnCoroutine != null)     // �ϳ��� �ϸ� ����ǵ��� �������� �ڷ�ƾ ���� �� �ڷ�ƾ ���� �ʱ�ȭ
        {
            StopCoroutine(turnCoroutine);
            turnCoroutine = null;
        }

        if (isPlayerTurn)
        {   
            turnCoroutine = uiManager.ShowTurn("PlayerTurn"); // ȭ�� UI ǥ��
            StartCoroutine(turnCoroutine);
            uiManager.SetStateText("�÷��̾� ��");
            player.GetComponent<PlayerKeyCtrl>().ResetTurn(); // �÷��̾� �� �ʱ�ȭ �Լ� ����
        }
        else
        {
            turnCoroutine = uiManager.ShowTurn("EnemyTurn"); // ȭ�� UI ǥ��
            StartCoroutine(turnCoroutine);
            uiManager.SetStateText("�� ��");
            
            foreach (EnemyCtrl enemy in enemys)
            {
                if (!enemy.isDie)
                {
                    nextIdx++;                              // ���� �� ���� �� �� �޼��� ����.
                    StartCoroutine(enemy.StartPatrolMode());
                    break;
                }
            }
        }
    }

    //�� ���� �Լ�
    public void Reset()
    {
        findPathCor = null;                         // ��ã�� �ڷ�ƾ ���� null ����
        tileCtrl.ResetNode();                       // Ÿ�� ����� �� �ʱ�ȭ

        if (IsPlayerOrEnemyDead())                  // �÷��̾� Ȥ�� ���� ��� �׾����� ���� ����
        {
            isGameStarted = false;
            uiManager.GamePanelSet("GameOver");
            Time.timeScale = 0;
            return;
        }

        if (!isPlayerTurn && leftEnemyTurn > 1)     // ���� ���� �� ���������� ���� ���� �� ����
        {
            leftEnemyTurn--;
            StartCoroutine(enemys[nextIdx].StartPatrolMode());
            return;
        }
        nextIdx = 0;                               // ��� ���� ������ �� �� �迭�� Index, ���� �� �� �ʱ�ȭ
        leftEnemyTurn = leftEnemy;
        SetTurn(!isPlayerTurn);                    // �� �����
    }

    //�÷��̾� Ȥ�� ���� ���� �������� üũ�ϴ� �Լ�
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
        return isDead;       //�����̶� �׾��ٸ� true ��ȯ
    }

    // ������ ������ϴ� �Լ�
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
                Instantiate(enemyPrefab, enemyPos, Quaternion.identity);  //�� �������� ����Ͽ� �� ������Ʈ �����
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

        if (isGameStarted)   //������ ���۵� �����϶��� �����
        {
            //ĳ���͸� ���ʸ��콺 Ŭ�� -> �̵��� ��� ������ �� �ֵ��� ��� ���� �ٲٴ� ����
            if (Input.GetMouseButtonUp(0) && isPlayerTurn && !player.GetComponent<PlayerKeyCtrl>().menuSelected)
            {
                // ���콺�� ���� ��ġ�� ��ǥ ���� �����´�
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
                        uiManager.SetStateText("�̵��� ��ġ�� �������ּ���.");
                        player.GetComponent<PlayerKeyCtrl>().menuSelected = true;
                        tileCtrl.ShowWalkablePath(playerNode);
                    }
                }
            }

            // Ȱ��ȭ�� ��带 ���콺 ������ ��ư���� Ŭ���� �ش� ���� �̵��ϴ� ����
            if (Input.GetMouseButtonUp(1) && isPlayerTurn)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 10000f, nodeMask))
                {
                    targetNode = hit.collider.gameObject.GetComponent<Node>();
                    if (targetNode.GetColor() == new Color(11 / 255f, 255 / 255f, 255 / 255f, 0.2f) && findPathCor == null)
                    {
                        uiManager.SetStateText("������ ��ġ�� �̵��մϴ�.");
                        findPathCor = tileCtrl.FindPathCoroutine(player.GetComponent<BaseChar>(), playerNode, targetNode, false);
                        StartCoroutine(findPathCor);
                    }
                }
            }

            // Esc ��ư�� ���� ��� ���� �ʱ�ȭ
            if (Input.GetKeyDown(KeyCode.Escape) && isPlayerTurn)
            {
                player.GetComponent<PlayerKeyCtrl>().ResetTurn();
            }
        }
        
        //������ ������ �� ȭ�� Ŭ���� ���� ���� �����
        if (Input.GetMouseButtonDown(0) && Time.timeScale == 0 && !isGameStarted)
        {
            Time.timeScale = 1;
            StartCoroutine(this.ReStartGame());
        }
    }



}
