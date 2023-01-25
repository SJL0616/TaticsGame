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
 마지막 수정일: 2023.01.04
 */
//IComparer<in T> 인터페이스를 상속하여 Compare 함수 정의
public class NodeComparer : IComparer<Node>
{
    // 두 노드를 비교하여 소비값을 셋팅
    public int Compare(Node x, Node y)
    {
        // 소비되는 총 비용이 y 가 더 크다면 리턴 -1
        if (x.FCost < y.FCost)
            return -1;
        // 그 반대라면 리턴 1
        else if (x.FCost > y.FCost)
            return 1;
        // 만약 둘다 같다면
        else if (x.FCost == y.FCost)
        {
            // 현재 위치에서 목적지 까지의 비용이 y 가 더 크다면 리턴 -1
            if (x.HCost < y.HCost)
                return -1;
            // 그렇지않다면 리턴 1
            else if (x.HCost > y.HCost)
                return 1;
        }

        // 해당 조건에 만족하지 않다면 리턴 0
        return 0;
    }
}

public class TileController : MonoBehaviour
{
    public GameObject nodePrefab;　// 노드 프리펍
    private int col;               // 열
    private int row;               // 행
    private bool m_isEnemy;        // 노드 기능을 쓰는 대상이 적인지 플레이어인지 확인용 bool형 변수
    private int repeatStack;       // 길찾기 재귀 함수의 재귀 횟수 확인용 int형 변수


    public int nodeRow1;           // 타일 노드 열
    public int nodeCol1;           // 타일 노드 행
    private Node[,] m_nodeArr;     // 타일 노드 배열
    private int m_nodeRow;         // 타일 노드 배열의 열
    private int m_nodeCol;         // 타일 노드 배열의 행
    // 이웃 노드를 레퍼런스 할 수 있는 리스트 선언
    private List<Node> m_neighbours = new List<Node>();
    private List<Node> m_currNeighbours = new List<Node>();
    // 장애물 노드 설정용 변수
    public int[] obstacleList = { 10, 11, 12, 13, 14, 15, 16, 17, 34, 39, 40, 41, 44, 64, 65, 73, 74, 75, 78, 81, 82, 90, 91, 92, 93, 124, 125, 126, 129, 130, 131, 133, 134, 135, 147, 151, 158, 159, 160, 164, 168, 181, 185, 187 };


    //타일 노드 생성 함수
    public bool TileSet()
    {
        bool isGameStarted = false;
        GenerateTile();
        SetObstacle();
        if(m_nodeArr.Length != 0)
        {
            isGameStarted = true;
        }
        return isGameStarted;
    }

    //노드 생성 후 배열에 저장하는 함수
    void GenerateTile()
    {
        // 전달된 노드 수
        m_nodeRow = nodeRow1;
        m_nodeCol = nodeCol1;

        // 로직 운영 변수
        int count = 0;

        // 전달된 행열 크기에 관련된 다차원배열을 만든다.(정방 행렬/정사각형 행렬)
        m_nodeArr = new Node[nodeRow1, nodeCol1];

        // 센터를 잡고...
        //float center = (float)(nodeCount * 150) / 2;

        // 모든 행,열을 돌면서 노드를 생성함. 
        for (int row = 0; row < nodeRow1; ++row)
        {
            for (int col = 0; col < nodeCol1; ++col)
            {
                // 부모를 지정하여 노드를 생성
                GameObject oneNode = Instantiate(nodePrefab,
                     Vector3.zero,
                     Quaternion.identity,
                     GameObject.Find("Tile").transform) as GameObject;
                Node node = oneNode.GetComponent<Node>();

                // 노드(Node) 다차원 배열을 돌면서 생성된 노드를 하나씩 연결 
                m_nodeArr[row, col] = node;

                // 노드의 네임을 지정
                
                node.name = "Node : " + count;
                node.m_nodeIdx = count;
                count++;

                // 생성된 노드에 행/열 셋팅
                node.SetNode(row, col);

                // 생성된 노드의 위치를 정렬 시킨다. 열 행 순으로...
                node.transform.localPosition = new Vector3(
                         col * 2.4f,
                         0.1f,
                         -row * 2.4f
                     );
            }
        }
    }

    // 장애물 설정 함수
    void SetObstacle()
    {
        int idx = 0;
        int currNum = obstacleList[idx];
        for (int row = 0; row < nodeRow1; ++row)
        {
            for (int col = 0; col < nodeCol1; ++col)
            {
                if(m_nodeArr[row,col].m_nodeIdx == currNum)
                {
                    m_nodeArr[row, col].SetNodeType(NodeType.Wall);
                    idx++;
                    if(idx == obstacleList.Length)
                    {
                        break;
                    }
                    else
                    {
                        currNum = obstacleList[idx];
                    }
                }
            }
        }
    }

    //위치 값을 매개변수로 받아 해당 위치의 노드 반환 함수
    public Node FindNode(Vector3 pos)
    {
        Node node = null;
        for (int row = 0; row < nodeRow1; ++row)
        {
            for (int col = 0; col < nodeCol1; ++col)
            {
                Vector3 nodePos = m_nodeArr[row, col].transform.position;
                if(Vector3.Distance(nodePos, pos) < 2.4f)
                {
                    node = m_nodeArr[row, col];
                    break;
                }
            }
        }
        return node;
    }

    //이동가능 노드인지 체크하는 함수
    public bool isPathColored()
    {
        bool isColored = false;
        for (int row = 0; row < nodeRow1; ++row)
        {
            for (int col = 0; col < nodeCol1; ++col)
            {
                Color nodeColor = m_nodeArr[row, col].GetColor();
                if (nodeColor == new Color(0, 255, 100, 0.2f))
                {
                    isColored = true;
                    return isColored;
                }
            }
        }
        return isColored;
    }

    //모든 노드의 색을 초기화하는 함수
    public void ResetNode()
    {
        for (int row = 0; row < nodeRow1; ++row)
        {
            for (int col = 0; col < nodeCol1; ++col)
            {
                m_nodeArr[row, col].Reset();
            }
        }
    }

    // 행, 열을 전달하여 만들어진 노드 그리드의 범위를 넘는지 확인하는 함수. 즉, 범위안2에 있으면 true로 체크
    private bool CheckNode(int row, int col)
    {
        if (row < 0 || row >= m_nodeRow)
            return false;

        if (col < 0 || col >= m_nodeCol)
            return false;
        if (m_nodeArr[row, col].m_nodeType == NodeType.Wall)
        {
            return false;
        }

        return true;
    }

    // 플레이어가 갈 수 있는 노드일 경우, 주위 노드의 색을 바꾸는 함수
    public void ShowWalkablePath(Node node)
    {
        for (int row = 0; row < nodeRow1; ++row)
        {
            for (int col = 0; col < nodeCol1; ++col)
            {
                m_nodeArr[row, col].SetWalkableNode(false);
            }
        }
        //node.SetWalkableNode(true);

        // 좌측 상단
        if (CheckNode(node.Row - 1, node.Col - 1))
        {
            m_nodeArr[node.Row - 1, node.Col - 1].SetWalkableNode(true);
        }

        // 상단
        if (CheckNode(node.Row - 1, node.Col))
        {
            m_nodeArr[node.Row - 1, node.Col].SetWalkableNode(true);
            //최상단
            if (CheckNode(node.Row - 2, node.Col))
            {
                m_nodeArr[node.Row - 2, node.Col].SetWalkableNode(true);
            }
        }
           
        // 우측 상단
        if (CheckNode(node.Row - 1, node.Col + 1))
        {
            m_nodeArr[node.Row - 1, node.Col + 1].SetWalkableNode(true);
        }

        // 좌측 
        if (CheckNode(node.Row, node.Col - 1))
        {
            m_nodeArr[node.Row, node.Col - 1].SetWalkableNode(true);
            // 최좌측 
            if (CheckNode(node.Row, node.Col - 2))
            {
                m_nodeArr[node.Row, node.Col - 2].SetWalkableNode(true);
            }
        }

        // 우측
        if (CheckNode(node.Row, node.Col + 1))
        {
            m_nodeArr[node.Row, node.Col + 1].SetWalkableNode(true);
            // 최우측
            if (CheckNode(node.Row, node.Col + 2))
            {
                m_nodeArr[node.Row, node.Col + 2].SetWalkableNode(true);

            }
        }

        // 좌측 하단
        if (CheckNode(node.Row + 1, node.Col - 1))
        {
            m_nodeArr[node.Row + 1, node.Col - 1].SetWalkableNode(true);
        }

        // 하단
        if (CheckNode(node.Row + 1, node.Col))
        {
            m_nodeArr[node.Row + 1, node.Col].SetWalkableNode(true);
            // 최하단
            if (CheckNode(node.Row + 2, node.Col))
            {
                m_nodeArr[node.Row + 2, node.Col].SetWalkableNode(true);
            }
        }

        // 우측 하단
        if (CheckNode(node.Row + 1, node.Col + 1))
        {
            m_nodeArr[node.Row + 1, node.Col + 1].SetWalkableNode(true);
        }
    }

    //주위 노드를 배열에 추가하여 반환(길찾기 함수용)
    public Node[] Neighbours(Node node)
    {
        m_neighbours.Clear();
        // 상단
        if (CheckNode(node.Row - 1, node.Col))
        {
            m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col]);
        }

        // 좌측 
        if (CheckNode(node.Row, node.Col - 1))
        {
            m_neighbours.Add(m_nodeArr[node.Row, node.Col - 1]);
        }

        // 우측
        if (CheckNode(node.Row, node.Col + 1))
        {
            m_neighbours.Add(m_nodeArr[node.Row, node.Col + 1]);
        }

        // 하단
        if (CheckNode(node.Row + 1, node.Col))
        {
            m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col]);
        }

        ////이중 순환문을 사용하여 이웃노드를 얻어오는 방법
        //for (int row = -1; row <= 1; ++row)
        //{
        //    for (int col = -1; col <= 1; ++col)
        //    {
        //        if (row == 0 && col == 0)
        //            continue;

        //        if (CheckNode(node.Row + row, node.Col + col))
        //        {
        //            m_neighbours.Add(m_nodeArr[node.Row + row, node.Col + col]);
        //        }
        //    }
        //}
        return m_neighbours.ToArray();
    }

    // 실행 플래그
    private bool m_execute = false;

    // 클로즈 노드 리스트
    private List<Node> m_closedList = new List<Node>();
    // 오픈 노드 리스트
    private List<Node> m_openList = new List<Node>();
    private List<Node> m_pathList = new List<Node>();

    // 노드의 소비 비용을 셋팅하기 위한 객체 생성
    private NodeComparer m_nodeComparer = new NodeComparer();

    // 현재 노드
    private Node m_currNode;
    // 시작 노드
    private Node m_startNode;
    // 타겟 노드
    private Node m_targetNode;
    // 이전 노드
    private Node m_prevNode;

    // 두 노드의 거리를 측정(핵심 알고리즘 1)
    public int GetDistance(Node a, Node b)
    {
        // 절대값을 활용하여 두 노드의 열 거리
        int x = Mathf.Abs(a.Col - b.Col);
        // 절대값을 활용하여 두 노드의 행 거리
        int y = Mathf.Abs(a.Row - b.Row);

        // 비지니스 로직(최소값과 절대값을 활용) 실제로 운영해보면 대각선 -14 값 소모 좌 우로 10 소모
        return 14 * Mathf.Min(x, y) + 10 * Mathf.Abs(x - y);
    }

    // 길찾기 함수 초기화용 함수
    public void Ready(Node playerNode, Node targetNode)
    {
        // 실행을 true
        m_execute = true;

        // 길찾기 함수 반복 체크용 변수 초기화
        repeatStack = 0;

        // 오픈,클로즈 노드 리스트 초기화(클리어)
        m_openList.Clear();
        m_closedList.Clear();
        m_pathList.Clear();

        //// 플레이어가 위치하고 있는 노드와 
        //// 타겟이 위치하고 있는 노드를 찾는다.
        m_startNode = playerNode;
        m_targetNode = targetNode;

        // 현재 노드를 스타트 노드로
        m_currNode = m_startNode;
        // 시작점의 노드의 초기비용을(현재 위치에서 시작위치까지의 소비된 비용) 0으로 초기화한다.
        m_startNode.SetGCost(0);
        // 시작점의 노드의 초기비용을(현재 위치에서 목적지까지의 비용) 시작 노드와 타겟 노드 사이의 거리로 설정 (알고리즘 활용)
        m_startNode.SetHCost(GetDistance(m_startNode, m_targetNode));

    }

    // 재귀함수와 코루틴을 사용하여 비동기로 처리해본 함수.
    public IEnumerator FindPathCoroutine(BaseChar Player,Node playerNode, Node targetNode, bool isEnemy)
    {
        yield return null;
        m_isEnemy = isEnemy;
        // 실행중이지 않은 경우라면 실행될 수 있도록 한다.
        if (!m_execute )
        {
            Ready(playerNode, targetNode);
            StartCoroutine(IEStep(Player));
        }
    }


    public IEnumerator IEStep(BaseChar player)
    {
        // 현재 시점의 이웃노드를 찾는다.
        
        Node[] neighbours = Neighbours(m_currNode);
        // 1. 기준 노드의 이웃노드를 찾고, gCost값과 hCost값을 계산한다.
        // 2. 오픈 노드 리스트에 등록되어 있지 않거나,
        //    이웃 노드의 gCost값이 현재 gCost값보다 크다면 갱신처리한다.
        m_openList.Clear();
        // 현재 이웃노드 리스트 클리어
        m_currNeighbours.Clear();
        
        m_currNeighbours.AddRange(neighbours);  // 이웃 노드들을 리스트에 전달하고...
       
        // 현재 이웃 노드를 순차적으로 검색
        for (int i = 0; i < neighbours.Length; ++i)
        {
            // 1. 계산이 완료된 로드라면 다시 순회할 수 있도록 한다.
            // 2. 이동할 수 없는 노드라면 다시 순회할 수 있도록 한다.

            // 클로즈 노드 리스트에(지나갈 수 없는 길) 이웃노드가 포함되어 있다면 continue 키워드에 의해서 skip
            if (m_closedList.Contains(neighbours[i]))
                continue;

            // 벽(장애물)일경우 오픈노드 리스트에서 생략 처리
            if (neighbours[i].NType == NodeType.Wall)
                continue;

            // 현재 노드의 현재 위치에서 시작위치까지의 소비된 비용 + 이웃 노드와 현재 노드와의 소비 비용으로 초기화
            int gCost = m_currNode.GCost + GetDistance(neighbours[i], m_currNode);

            // 오픈 노드 리스트에(지나갈수 있는 길) 이웃노드가 포함되어 있지 않거나 현재 위치에서 시작위치까지의 소비된 비용이 이웃 노드보다 낮을경우...
            if (m_openList.Contains(neighbours[i]) == false ||
                gCost < neighbours[i].GCost)

            {
                // 이웃 노드에서 목표점까지의 거리값을 구함.
                int hCost = GetDistance(neighbours[i], m_targetNode);
                // 이웃 노드에 각각 소비값을 초기화
                neighbours[i].SetGCost(gCost);
                neighbours[i].SetHCost(hCost);
                // 이웃 노드에 부모를 현재 노드로 초기화
                neighbours[i].SetParent(m_currNode);

                // 만약 오픈 노드 리스트에 이웃 노드가 포함되지 않았다면 추가
                if (!m_openList.Contains(neighbours[i]))
                {
                    m_openList.Add(neighbours[i]);
                }
                    
            }
        }

        // 기준 노드의 연산 처리가 끝났으므로, 클로즈 리스트에 추가한다.
        m_closedList.Add(m_currNode);

        // 기준 노드가 오픈 노드 리스트에 담겨져 있다면 삭제 한다.
        if (m_openList.Contains(m_currNode))
            m_openList.Remove(m_currNode);

        // 오픈 노드 리스트에서 최소 비용을 갖는 노드를 찾아
        // 현재 노드로 설정한다.
        if (m_openList.Count > 0)
        {
            // 정렬
            m_openList.Sort(m_nodeComparer);
            // 현재 노드가 NULL이 아닐때...
            if (m_currNode != null)
            {
                // 이전 노드에 현재 노드를 연결하고....
                m_prevNode = m_currNode;
            }
            // 현재 노드에 오픈 노드 리스트에 가장 첫 번째 노드를 연결
            m_currNode = m_openList[0];
           // Debug.Log(m_openList[0].m_row + " , " + m_openList[0].m_col);
            m_pathList.Add(m_openList[0]);
        }

        yield return null;

        // 노드를 찾았으므로 
        // 종료 처리한다.
        if (m_currNode == m_targetNode ||(m_isEnemy && repeatStack ==3))
        {
            // 에이스타 알고리즘이 다시 실행될 수 있도록 
            // 플래그 값을 변경한다.
            m_execute = false;

            // 플레이어 길찾기
            player.SetPath(m_pathList);
        }
        // 노드를 못찾았으므로 다시 코루틴
        else
        {
            repeatStack++;
            StartCoroutine(IEStep(player));
        }
            
    }


}
