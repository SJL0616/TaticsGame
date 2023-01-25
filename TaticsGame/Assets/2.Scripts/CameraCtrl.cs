using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 �ۼ��� : �̻���
 ����: ��ü �� ī�޶� ��Ʈ�� Ŭ����
       ��� 
           1. �⺻������ �÷��̾� ������Ʈ�� ����ٴ�
           2. �÷��̾� ������ ��� ������Ʈ �����ϰ� ����
           3. ������ ���콺�� ī�޶� �¿� ȸ��
 ������ ������: 2023.01.06
 */
public class CameraCtrl : MonoBehaviour
{
    GameObject player;
    public List<GameObject> obstacles;      // ��ֹ� ������Ʈ ����� �迭
    public List<GameObject> hitObjs;
    public LayerMask playerMask;            // �÷��̾� ���̾�
    private MeshRenderer obstacleRenderer;  // ��ֹ� ������Ʈ�� MeshRenderer 
    private GameObject cameraArm;           // ī�޶� �� ������Ʈ
    private Material originalMat;           // ��ֹ� ������Ʈ ó���� ���� (���� ������ �ִ� Material)
    public Material transparentMat;         // ��ֹ� ������Ʈ ó���� ���� (��ȯ�� Material)     

    private void Awake()
    {
       if ((player = GameObject.Find("Player"))  != null)
        {
            playerMask = player.layer;
            cameraArm = transform.root.gameObject;
        }
        
    }
    // Start is called before the first frame update
    void Start()
    {
        obstacles = new List<GameObject>();
        hitObjs = new List<GameObject>();
        obstacles.Clear();
        hitObjs.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        cameraArm.transform.position = Vector3.Slerp(cameraArm.transform.position, player.transform.position, 4.0f * Time.deltaTime);

        if (Input.GetMouseButton(1))
        {  //���콺 ������ ��ư���� ī�޶� ȸ��
            Vector3 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            Vector3 camAngle = cameraArm.transform.rotation.eulerAngles;

            cameraArm.transform.rotation = Quaternion.Euler(camAngle.x, camAngle.y + mouseDelta.x, camAngle.z);
        }
        ObjTransperantCtrl();
    }


    //�� ������Ʈ ���� �����Լ�
    void ObjTransperantCtrl()
    {
        //ī�޶�� �÷��̾� ���̿� �� ������Ʈ�� ������ �����ϰ� ó��
        float Distance = Vector3.Distance(transform.position, player.transform.position);
        Vector3 Direction = (player.transform.position - transform.position).normalized;

        RaycastHit[] hits;
        hits = (Physics.RaycastAll(transform.position, Direction, Distance));
        hitObjs.Clear();
        foreach(RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.name != "Player" && hit.collider.gameObject.name != "Floor Tile" && !hit.collider.gameObject.name.Contains("Node"))
            {
                // 2.�¾����� Renderer�� ���´�.
                obstacleRenderer = hit.collider.gameObject.GetComponent<MeshRenderer>();
                if (obstacleRenderer != null)
                {

                    originalMat = obstacleRenderer.material;
                    Material secondMat = originalMat;
                    secondMat.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");

                    Color matColor = secondMat.color;
                    matColor.a = 0.05f;
                    secondMat.color = matColor;

                    obstacleRenderer.material = secondMat;  // ���� Material�� ��ȯ
                    if(!CheckHitObj(hit.collider.gameObject)) hitObjs.Add(hit.collider.gameObject);  // ���� RayCast�� ���� ������Ʈ�� �迭�� �߰�.
                    if (!CheckObstacle(hit.collider.gameObject)) obstacles.Add(hit.collider.gameObject); // obstacles �迭���� ������Ʈ �߰�.
                }
            }
        }
        ClearObstacle();
    }
    
    bool CheckHitObj(GameObject obj)
    {
        for (int i = 0; i < hitObjs.Count; i++)
        {
            if (hitObjs[i] == obj)
            {
                return true;
            }
        }
        return false;
    }
    bool CheckObstacle(GameObject obj)
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            if (obstacles[i] == obj)
            {
                return true;
            }
        }
        return false;
    }

    // ���� Ray�� ���� ������Ʈ�� �ƴ϶�� ������ Material�� �ٲ�
    bool ClearObstacle()
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            if (!CheckHitObj(obstacles[i]))
            {
                obstacleRenderer = obstacles[i].GetComponent<MeshRenderer>();
                
                originalMat = obstacleRenderer.material;

                // originalShader = originalMat.shader;
                originalMat.shader = Shader.Find("Standard");

                Color matColor = originalMat.color;

                matColor.a = 1f;

                originalMat.color = matColor;
                originalMat.SetFloat("_Mode", 1);
                obstacles.RemoveAt(i);

            }
        }
        return false;
    }


}
