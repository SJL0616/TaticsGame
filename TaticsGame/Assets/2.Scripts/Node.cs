using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ��忡 ���� �Ӽ� �� : "����", "��(��ֹ�)".
public enum NodeType
{
    None,
    Wall,
}

public class Node : MonoBehaviour
{
    // �⺻�� ����� Ÿ���� "����"����...
    public NodeType m_nodeType = NodeType.None;

    // �Һ�Ǵ� �� ���
    private int m_fCost = 0;

    // ���� ��ġ���� ������������ ���(�̵��� ���� �پ� ��)
    private int m_hCost = 0;

    // ���� ��ġ���� ������ġ������ �Һ�� ���(�̵��� ���� �þ� ��)
    private int m_gCost = 0;

    // ��,��
    public int m_row = 0;
    public int m_col = 0;

    // �θ��� ��带 ����ϰ� �ֱ� ���� ��.
    private Node m_parent;

    // �浹ü
    private Collider2D m_collider;
    public GameObject m_turnPoint;
    public int m_nodeIdx;

    // ����� ǥ���ϱ� ����
    private TextMesh m_fText;
    private TextMesh m_hText;
    private TextMesh m_gText;

    // �̹��� ���۷���
    private Image m_image;
    private MeshRenderer m_ms;

    // �Ӽ� : ��
    public int Row
    {
        get { return m_row; }
    }

    // �Ӽ� : ��
    public int Col
    {
        get { return m_col; }
    }

    // �Ӽ� : �θ� ��带 ����
    public Node Parent
    {
        get { return m_parent; }
    }

    // �Ӽ� : ���޵� ������ transform ����
    public Vector3 POS
    {
        set
        {
            transform.position = value;
        }
        get { return transform.position; }
    }

    // �Ӽ� : ����� Ÿ���� ����
    public NodeType NType
    {
        get { return m_nodeType; }
    }

    // ���޵� ��� Ÿ������ ���� ��� Ÿ�� ���� �� ���� ����
    public void SetNodeType(NodeType nodeType)
    {
        // ��� Ÿ���� ��(��ֹ�)�϶�...
        if (nodeType == NodeType.Wall)
        {
            // û�ϻ� ����
            SetColor(Color.cyan);
        }
        // ���޵� ��� Ÿ�� ����
        m_nodeType = nodeType;

    }

    // �����ϸ� ��� Ÿ���� None����, �� �ؽ�Ʈ�� ����, �θ�� ����, ������ �⺻ ȭ��Ʈ
    public void Reset()
    {
        //m_nodeType = NodeType.None;
        m_fText.text = "F : ";
        m_hText.text = "H : ";
        m_gText.text = "G : ";
        //m_parent = null;
        m_ms.material.color = Color.white;
        m_ms.enabled = false;
    }

    private void OnMouseEnter()
    {
        if (m_ms.enabled)
        {
            m_ms.material.color = new Color(11 / 255f, 255 / 255f, 255 / 255f, 0.2f);
        }
    }

    private void OnMouseExit()
    {
        if (m_ms.enabled)
        {
            m_ms.material.color = new Color(0, 255 / 255f, 100 / 255f, 0.2f);
        }
    }

    // �� ���۷��� ���� 
    public void Awake()
    {
        m_collider = GetComponent<Collider2D>();
        m_ms = GetComponent<MeshRenderer>();
        
        m_ms.enabled = false;
        m_fText = transform.Find("F").GetComponent<TextMesh>();
        m_hText = transform.Find("H").GetComponent<TextMesh>();
        m_gText = transform.Find("G").GetComponent<TextMesh>();
        m_turnPoint = transform.Find("TurnPoint").gameObject;

    }

    public void SetWalkableNode(bool isWalkable)
    {
       
        if (isWalkable)
        {
            m_ms.enabled = true;
            m_ms.material.color = new Color(0, 255/255f, 100/255f, 0.2f);
        }
        else
        {
            m_ms.enabled = false;
            m_ms.material.color = new Color(255/255f,0,0,0.2f);
        }
    }

    // ���޵� ���콺 �����ǿ� �ٿ�� �ڽ��� ���ԵǾ� �ִ��� Ȯ���ϴ� �Լ� (���⼱ �Ⱦ� X)
    public bool Contains(Vector3 position)
    {
        return m_collider.bounds.Contains(position);
    }

    // ����� ������ �����ϴ� �Լ�
    public void SetColor(Color color)
    {
        // ����� Ÿ���� ��(��ֹ�) �̸� �׳� û�ϻ� ����
        if (m_nodeType == NodeType.Wall)
            return;

        // �̹��� ���۷����� NULL �� �ƴ϶�� ���޵� �������� ����
        if (m_image != null)
            m_image.color = color;

    }
    // ����� ������ �����ϴ� �Լ�
    public Color GetColor()
    {
        // ����� Ÿ���� ��(��ֹ�) �̸� �׳� û�ϻ� ����
        if (m_nodeType == NodeType.Wall)
            return Color.black;

        // �̹��� ���۷����� NULL �� �ƴ϶�� ���޵� �������� ����
        //if (m_image != null)
        //    m_image.color = color;
        return m_ms.material.color;
    }

    // ���޵� �θ� ���� ���� ����� �θ� ����
    public void SetParent(Node parent)
    {
        m_parent = parent;
    }

    // ����� ��� ���� ����
    public void SetNode(int row, int col)
    {
        m_row = row;
        m_col = col;
    }

    // �Ӽ� : �Һ�Ǵ� �� ����� ��ȯ
    public int FCost
    {
        get { return m_hCost + m_gCost; }
    }

    // �Ӽ� : ���� ��ġ���� ������������ ����� ��ȯ
    public int HCost
    {
        get { return m_hCost; }
    }

    // �Ӽ� : ���� ��ġ���� ������ġ������ �Һ�� ����� ��ȯ
    public int GCost
    {
        get { return m_gCost; }
    }

    // ���޵� ������ ���� ��ġ���� ������������ ��� �� �Һ�Ǵ� �� ����� �ش� ������ ���� �� TEXT ����
    public void SetHCost(int cost)
    {
        m_hText.text = "H : " + cost;
        m_hCost = cost;

        m_fText.text = "F : " + (m_hCost + m_gCost);
    }

    // ���޵� ������ ���� ��ġ���� ������ġ������ �Һ�� ����� �ش� ������ ���� �� TEXT ����
    public void SetGCost(int cost)
    {
        m_gText.text = "G : " + cost;
        m_gCost = cost;
    }
}
