using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 작성자 : 이상준
 내용: 전체 뷰 카메라 컨트롤 클래스
       기능 
           1. 기본적으로 플레이어 오브젝트를 따라다님
           2. 플레이어 사이의 배경 오브젝트 투명하게 설정
           3. 오른쪽 마우스로 카메라 좌우 회전
 마지막 수정일: 2023.01.06
 */
public class CameraCtrl : MonoBehaviour
{
    GameObject player;
    public List<GameObject> obstacles;      // 장애물 오브젝트 저장용 배열
    public List<GameObject> hitObjs;
    public LayerMask playerMask;            // 플레이어 레이어
    private MeshRenderer obstacleRenderer;  // 장애물 오브젝트의 MeshRenderer 
    private GameObject cameraArm;           // 카메라 암 오브젝트
    private Material originalMat;           // 장애물 오브젝트 처리용 변수 (원래 가지고 있는 Material)
    public Material transparentMat;         // 장애물 오브젝트 처리용 변수 (교환용 Material)     

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
        {  //마우스 오른쪽 버튼으로 카메라 회전
            Vector3 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            Vector3 camAngle = cameraArm.transform.rotation.eulerAngles;

            cameraArm.transform.rotation = Quaternion.Euler(camAngle.x, camAngle.y + mouseDelta.x, camAngle.z);
        }
        ObjTransperantCtrl();
    }


    //맵 오브젝트 투명도 조절함수
    void ObjTransperantCtrl()
    {
        //카메라와 플레이어 사이에 맵 오브젝트가 있으면 투명하게 처리
        float Distance = Vector3.Distance(transform.position, player.transform.position);
        Vector3 Direction = (player.transform.position - transform.position).normalized;

        RaycastHit[] hits;
        hits = (Physics.RaycastAll(transform.position, Direction, Distance));
        hitObjs.Clear();
        foreach(RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.name != "Player" && hit.collider.gameObject.name != "Floor Tile" && !hit.collider.gameObject.name.Contains("Node"))
            {
                // 2.맞았으면 Renderer를 얻어온다.
                obstacleRenderer = hit.collider.gameObject.GetComponent<MeshRenderer>();
                if (obstacleRenderer != null)
                {

                    originalMat = obstacleRenderer.material;
                    Material secondMat = originalMat;
                    secondMat.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");

                    Color matColor = secondMat.color;
                    matColor.a = 0.05f;
                    secondMat.color = matColor;

                    obstacleRenderer.material = secondMat;  // 투명 Material로 교환
                    if(!CheckHitObj(hit.collider.gameObject)) hitObjs.Add(hit.collider.gameObject);  // 현재 RayCast에 맞은 오브젝트를 배열에 추가.
                    if (!CheckObstacle(hit.collider.gameObject)) obstacles.Add(hit.collider.gameObject); // obstacles 배열에도 오브젝트 추가.
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

    // 현재 Ray에 맞은 오브젝트가 아니라면 불투명 Material로 바꿈
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
