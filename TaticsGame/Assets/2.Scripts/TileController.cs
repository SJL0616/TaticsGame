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
 ������ ������: 2023.01.04
 */
//IComparer<in T> �������̽��� ����Ͽ� Compare �Լ� ����
public class NodeComparer : IComparer<Node>
{
    // �� ��带 ���Ͽ� �Һ��� ����
    public int Compare(Node x, Node y)
    {
        // �Һ�Ǵ� �� ����� y �� �� ũ�ٸ� ���� -1
        if (x.FCost < y.FCost)
            return -1;
        // �� �ݴ��� ���� 1
        else if (x.FCost > y.FCost)
            return 1;
        // ���� �Ѵ� ���ٸ�
        else if (x.FCost == y.FCost)
        {
            // ���� ��ġ���� ������ ������ ����� y �� �� ũ�ٸ� ���� -1
            if (x.HCost < y.HCost)
                return -1;
            // �׷����ʴٸ� ���� 1
            else if (x.HCost > y.HCost)
                return 1;
        }

        // �ش� ���ǿ� �������� �ʴٸ� ���� 0
        return 0;
    }
}

public class TileController : MonoBehaviour
{
    public GameObject nodePrefab;��// ��� ������
    private int col;               // ��
    private int row;               // ��
    private bool m_isEnemy;        // ��� ����� ���� ����� ������ �÷��̾����� Ȯ�ο� bool�� ����
    private int repeatStack;       // ��ã�� ��� �Լ��� ��� Ƚ�� Ȯ�ο� int�� ����


    public int nodeRow1;           // Ÿ�� ��� ��
    public int nodeCol1;           // Ÿ�� ��� ��
    private Node[,] m_nodeArr;     // Ÿ�� ��� �迭
    private int m_nodeRow;         // Ÿ�� ��� �迭�� ��
    private int m_nodeCol;         // Ÿ�� ��� �迭�� ��
    // �̿� ��带 ���۷��� �� �� �ִ� ����Ʈ ����
    private List<Node> m_neighbours = new List<Node>();
    private List<Node> m_currNeighbours = new List<Node>();
    // ��ֹ� ��� ������ ����
    public int[] obstacleList = { 10, 11, 12, 13, 14, 15, 16, 17, 34, 39, 40, 41, 44, 64, 65, 73, 74, 75, 78, 81, 82, 90, 91, 92, 93, 124, 125, 126, 129, 130, 131, 133, 134, 135, 147, 151, 158, 159, 160, 164, 168, 181, 185, 187 };


    //Ÿ�� ��� ���� �Լ�
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

    //��� ���� �� �迭�� �����ϴ� �Լ�
    void GenerateTile()
    {
        // ���޵� ��� ��
        m_nodeRow = nodeRow1;
        m_nodeCol = nodeCol1;

        // ���� � ����
        int count = 0;

        // ���޵� �࿭ ũ�⿡ ���õ� �������迭�� �����.(���� ���/���簢�� ���)
        m_nodeArr = new Node[nodeRow1, nodeCol1];

        // ���͸� ���...
        //float center = (float)(nodeCount * 150) / 2;

        // ��� ��,���� ���鼭 ��带 ������. 
        for (int row = 0; row < nodeRow1; ++row)
        {
            for (int col = 0; col < nodeCol1; ++col)
            {
                // �θ� �����Ͽ� ��带 ����
                GameObject oneNode = Instantiate(nodePrefab,
                     Vector3.zero,
                     Quaternion.identity,
                     GameObject.Find("Tile").transform) as GameObject;
                Node node = oneNode.GetComponent<Node>();

                // ���(Node) ������ �迭�� ���鼭 ������ ��带 �ϳ��� ���� 
                m_nodeArr[row, col] = node;

                // ����� ������ ����
                
                node.name = "Node : " + count;
                node.m_nodeIdx = count;
                count++;

                // ������ ��忡 ��/�� ����
                node.SetNode(row, col);

                // ������ ����� ��ġ�� ���� ��Ų��. �� �� ������...
                node.transform.localPosition = new Vector3(
                         col * 2.4f,
                         0.1f,
                         -row * 2.4f
                     );
            }
        }
    }

    // ��ֹ� ���� �Լ�
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

    //��ġ ���� �Ű������� �޾� �ش� ��ġ�� ��� ��ȯ �Լ�
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

    //�̵����� ������� üũ�ϴ� �Լ�
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

    //��� ����� ���� �ʱ�ȭ�ϴ� �Լ�
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

    // ��, ���� �����Ͽ� ������� ��� �׸����� ������ �Ѵ��� Ȯ���ϴ� �Լ�. ��, ������2�� ������ true�� üũ
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

    // �÷��̾ �� �� �ִ� ����� ���, ���� ����� ���� �ٲٴ� �Լ�
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

        // ���� ���
        if (CheckNode(node.Row - 1, node.Col - 1))
        {
            m_nodeArr[node.Row - 1, node.Col - 1].SetWalkableNode(true);
        }

        // ���
        if (CheckNode(node.Row - 1, node.Col))
        {
            m_nodeArr[node.Row - 1, node.Col].SetWalkableNode(true);
            //�ֻ��
            if (CheckNode(node.Row - 2, node.Col))
            {
                m_nodeArr[node.Row - 2, node.Col].SetWalkableNode(true);
            }
        }
           
        // ���� ���
        if (CheckNode(node.Row - 1, node.Col + 1))
        {
            m_nodeArr[node.Row - 1, node.Col + 1].SetWalkableNode(true);
        }

        // ���� 
        if (CheckNode(node.Row, node.Col - 1))
        {
            m_nodeArr[node.Row, node.Col - 1].SetWalkableNode(true);
            // ������ 
            if (CheckNode(node.Row, node.Col - 2))
            {
                m_nodeArr[node.Row, node.Col - 2].SetWalkableNode(true);
            }
        }

        // ����
        if (CheckNode(node.Row, node.Col + 1))
        {
            m_nodeArr[node.Row, node.Col + 1].SetWalkableNode(true);
            // �ֿ���
            if (CheckNode(node.Row, node.Col + 2))
            {
                m_nodeArr[node.Row, node.Col + 2].SetWalkableNode(true);

            }
        }

        // ���� �ϴ�
        if (CheckNode(node.Row + 1, node.Col - 1))
        {
            m_nodeArr[node.Row + 1, node.Col - 1].SetWalkableNode(true);
        }

        // �ϴ�
        if (CheckNode(node.Row + 1, node.Col))
        {
            m_nodeArr[node.Row + 1, node.Col].SetWalkableNode(true);
            // ���ϴ�
            if (CheckNode(node.Row + 2, node.Col))
            {
                m_nodeArr[node.Row + 2, node.Col].SetWalkableNode(true);
            }
        }

        // ���� �ϴ�
        if (CheckNode(node.Row + 1, node.Col + 1))
        {
            m_nodeArr[node.Row + 1, node.Col + 1].SetWalkableNode(true);
        }
    }

    //���� ��带 �迭�� �߰��Ͽ� ��ȯ(��ã�� �Լ���)
    public Node[] Neighbours(Node node)
    {
        m_neighbours.Clear();
        // ���
        if (CheckNode(node.Row - 1, node.Col))
        {
            m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col]);
        }

        // ���� 
        if (CheckNode(node.Row, node.Col - 1))
        {
            m_neighbours.Add(m_nodeArr[node.Row, node.Col - 1]);
        }

        // ����
        if (CheckNode(node.Row, node.Col + 1))
        {
            m_neighbours.Add(m_nodeArr[node.Row, node.Col + 1]);
        }

        // �ϴ�
        if (CheckNode(node.Row + 1, node.Col))
        {
            m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col]);
        }

        ////���� ��ȯ���� ����Ͽ� �̿���带 ������ ���
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

    // ���� �÷���
    private bool m_execute = false;

    // Ŭ���� ��� ����Ʈ
    private List<Node> m_closedList = new List<Node>();
    // ���� ��� ����Ʈ
    private List<Node> m_openList = new List<Node>();
    private List<Node> m_pathList = new List<Node>();

    // ����� �Һ� ����� �����ϱ� ���� ��ü ����
    private NodeComparer m_nodeComparer = new NodeComparer();

    // ���� ���
    private Node m_currNode;
    // ���� ���
    private Node m_startNode;
    // Ÿ�� ���
    private Node m_targetNode;
    // ���� ���
    private Node m_prevNode;

    // �� ����� �Ÿ��� ����(�ٽ� �˰��� 1)
    public int GetDistance(Node a, Node b)
    {
        // ���밪�� Ȱ���Ͽ� �� ����� �� �Ÿ�
        int x = Mathf.Abs(a.Col - b.Col);
        // ���밪�� Ȱ���Ͽ� �� ����� �� �Ÿ�
        int y = Mathf.Abs(a.Row - b.Row);

        // �����Ͻ� ����(�ּҰ��� ���밪�� Ȱ��) ������ ��غ��� �밢�� -14 �� �Ҹ� �� ��� 10 �Ҹ�
        return 14 * Mathf.Min(x, y) + 10 * Mathf.Abs(x - y);
    }

    // ��ã�� �Լ� �ʱ�ȭ�� �Լ�
    public void Ready(Node playerNode, Node targetNode)
    {
        // ������ true
        m_execute = true;

        // ��ã�� �Լ� �ݺ� üũ�� ���� �ʱ�ȭ
        repeatStack = 0;

        // ����,Ŭ���� ��� ����Ʈ �ʱ�ȭ(Ŭ����)
        m_openList.Clear();
        m_closedList.Clear();
        m_pathList.Clear();

        //// �÷��̾ ��ġ�ϰ� �ִ� ���� 
        //// Ÿ���� ��ġ�ϰ� �ִ� ��带 ã�´�.
        m_startNode = playerNode;
        m_targetNode = targetNode;

        // ���� ��带 ��ŸƮ ����
        m_currNode = m_startNode;
        // �������� ����� �ʱ�����(���� ��ġ���� ������ġ������ �Һ�� ���) 0���� �ʱ�ȭ�Ѵ�.
        m_startNode.SetGCost(0);
        // �������� ����� �ʱ�����(���� ��ġ���� ������������ ���) ���� ���� Ÿ�� ��� ������ �Ÿ��� ���� (�˰��� Ȱ��)
        m_startNode.SetHCost(GetDistance(m_startNode, m_targetNode));

    }

    // ����Լ��� �ڷ�ƾ�� ����Ͽ� �񵿱�� ó���غ� �Լ�.
    public IEnumerator FindPathCoroutine(BaseChar Player,Node playerNode, Node targetNode, bool isEnemy)
    {
        yield return null;
        m_isEnemy = isEnemy;
        // ���������� ���� ����� ����� �� �ֵ��� �Ѵ�.
        if (!m_execute )
        {
            Ready(playerNode, targetNode);
            StartCoroutine(IEStep(Player));
        }
    }


    public IEnumerator IEStep(BaseChar player)
    {
        // ���� ������ �̿���带 ã�´�.
        
        Node[] neighbours = Neighbours(m_currNode);
        // 1. ���� ����� �̿���带 ã��, gCost���� hCost���� ����Ѵ�.
        // 2. ���� ��� ����Ʈ�� ��ϵǾ� ���� �ʰų�,
        //    �̿� ����� gCost���� ���� gCost������ ũ�ٸ� ����ó���Ѵ�.
        m_openList.Clear();
        // ���� �̿���� ����Ʈ Ŭ����
        m_currNeighbours.Clear();
        
        m_currNeighbours.AddRange(neighbours);  // �̿� ������ ����Ʈ�� �����ϰ�...
       
        // ���� �̿� ��带 ���������� �˻�
        for (int i = 0; i < neighbours.Length; ++i)
        {
            // 1. ����� �Ϸ�� �ε��� �ٽ� ��ȸ�� �� �ֵ��� �Ѵ�.
            // 2. �̵��� �� ���� ����� �ٽ� ��ȸ�� �� �ֵ��� �Ѵ�.

            // Ŭ���� ��� ����Ʈ��(������ �� ���� ��) �̿���尡 ���ԵǾ� �ִٸ� continue Ű���忡 ���ؼ� skip
            if (m_closedList.Contains(neighbours[i]))
                continue;

            // ��(��ֹ�)�ϰ�� ���³�� ����Ʈ���� ���� ó��
            if (neighbours[i].NType == NodeType.Wall)
                continue;

            // ���� ����� ���� ��ġ���� ������ġ������ �Һ�� ��� + �̿� ���� ���� ������ �Һ� ������� �ʱ�ȭ
            int gCost = m_currNode.GCost + GetDistance(neighbours[i], m_currNode);

            // ���� ��� ����Ʈ��(�������� �ִ� ��) �̿���尡 ���ԵǾ� ���� �ʰų� ���� ��ġ���� ������ġ������ �Һ�� ����� �̿� ��庸�� �������...
            if (m_openList.Contains(neighbours[i]) == false ||
                gCost < neighbours[i].GCost)

            {
                // �̿� ��忡�� ��ǥ�������� �Ÿ����� ����.
                int hCost = GetDistance(neighbours[i], m_targetNode);
                // �̿� ��忡 ���� �Һ��� �ʱ�ȭ
                neighbours[i].SetGCost(gCost);
                neighbours[i].SetHCost(hCost);
                // �̿� ��忡 �θ� ���� ���� �ʱ�ȭ
                neighbours[i].SetParent(m_currNode);

                // ���� ���� ��� ����Ʈ�� �̿� ��尡 ���Ե��� �ʾҴٸ� �߰�
                if (!m_openList.Contains(neighbours[i]))
                {
                    m_openList.Add(neighbours[i]);
                }
                    
            }
        }

        // ���� ����� ���� ó���� �������Ƿ�, Ŭ���� ����Ʈ�� �߰��Ѵ�.
        m_closedList.Add(m_currNode);

        // ���� ��尡 ���� ��� ����Ʈ�� ����� �ִٸ� ���� �Ѵ�.
        if (m_openList.Contains(m_currNode))
            m_openList.Remove(m_currNode);

        // ���� ��� ����Ʈ���� �ּ� ����� ���� ��带 ã��
        // ���� ���� �����Ѵ�.
        if (m_openList.Count > 0)
        {
            // ����
            m_openList.Sort(m_nodeComparer);
            // ���� ��尡 NULL�� �ƴҶ�...
            if (m_currNode != null)
            {
                // ���� ��忡 ���� ��带 �����ϰ�....
                m_prevNode = m_currNode;
            }
            // ���� ��忡 ���� ��� ����Ʈ�� ���� ù ��° ��带 ����
            m_currNode = m_openList[0];
           // Debug.Log(m_openList[0].m_row + " , " + m_openList[0].m_col);
            m_pathList.Add(m_openList[0]);
        }

        yield return null;

        // ��带 ã�����Ƿ� 
        // ���� ó���Ѵ�.
        if (m_currNode == m_targetNode ||(m_isEnemy && repeatStack ==3))
        {
            // ���̽�Ÿ �˰����� �ٽ� ����� �� �ֵ��� 
            // �÷��� ���� �����Ѵ�.
            m_execute = false;

            // �÷��̾� ��ã��
            player.SetPath(m_pathList);
        }
        // ��带 ��ã�����Ƿ� �ٽ� �ڷ�ƾ
        else
        {
            repeatStack++;
            StartCoroutine(IEStep(player));
        }
            
    }


}
