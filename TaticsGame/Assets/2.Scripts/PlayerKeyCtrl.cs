using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
 작성자 : 이상준
 내용: 플레이어컨트롤 클래스
       기능 
           1. 턴 실행, 초기화
           2. 사격, 재장전모드, 죽음 기능
 마지막 수정일: 2023.01.06
 */
public class PlayerKeyCtrl : MonoBehaviour
{
    Animator anim;
    public bool isDie;
    public Transform firePos;
    public GameObject muzzleFlash;

    // 추가 변수 
    public LayerMask enemyMask;       //적 콜라이더 저장용 변수
    public float viewRadius;          //사격 범위
    private Transform characterBody;
    private VcamCtrl vcamCtrl;
    private List<Collider> targets;
    private FCanvasCtrl fCanvasCtrl;
    private UIManager uiManager;
    private TileController tileCtrl;
    private csSoundManager csSoundManager;
    private bool isSelected;
    public bool menuSelected
    {
        set { isSelected = value; }
        get { return isSelected; }
    }

    private GameObject currEnemy;
    private float hitRate;
    private IEnumerator reload;
    public Vector3 playerPos;
    private int idx;

    private void Awake()
    {
        anim = transform.Find("CharacterBody").gameObject.GetComponent<Animator>();
        characterBody = transform.Find("CharacterBody");
        vcamCtrl = GameObject.FindObjectOfType<VcamCtrl>();
        targets = new List<Collider>();
        fCanvasCtrl = GameObject.FindObjectOfType<FCanvasCtrl>();
        csSoundManager = GameObject.FindObjectOfType<csSoundManager>();
        tileCtrl = GameObject.Find("Tile").GetComponent<TileController>();
        menuSelected = false;
        uiManager = GameObject.FindObjectOfType<UIManager>();
        muzzleFlash.SetActive(false);
        reload = null;
        idx = 0;
    }

    // 턴 시작 전에 각종 변수, 배열 초기화 함수
    public void ResetTurn()
    {
        if (isDie) { return; }
        menuSelected = false;
        targets.Clear();
        tileCtrl.ResetNode();
        vcamCtrl.SwitchVCam(1);
        fCanvasCtrl.BoxButtonSetActive(true);
        fCanvasCtrl.AimingImgSet(false, null);
        idx = 0;
    }

    // 사격 메뉴
    public void OnAimMode()
    {
        if (menuSelected && targets.Count == 0) return;  //이미 다른 메뉴가 선택된 상태 라면 return
        if (!uiManager.CheckMagazine()) { return; }      //장전된 탄약의 갯수가 0개라면 return
        tileCtrl.ResetNode();                          
        
        //주위의 적 콜라이더를 가져와서 적과 플레이어 사이의 장애물이 없는지 RayCast를 사용해서 검사.
        if(targets.Count == 0)
        {
            idx = 0;
            menuSelected = true;
            Collider[] targetArray = Physics.OverlapSphere(transform.position, viewRadius, enemyMask);
            foreach (Collider col in targetArray) 
            { // 주위의 죽은 상태가 아닌 적 오브젝트를 배열에 추가.
                if (!col.gameObject.GetComponent<EnemyCtrl>().isDie)
                {
                    targets.Add(col);
                }
            }
        }

        if(targets.Count != 0)
        {
            Transform target = targets[idx].transform;
            vcamCtrl.SwitchVCam(2,target);                          // 사격용 카메라 위치로 카메라 이동
            Vector3 dirToTarget = (target.position - characterBody.transform.position).normalized;
            float dstToTarget = Vector3.Distance(characterBody.transform.position, target.transform.position);
            StartCoroutine(LookAtTarget(target));                   // 플레이어 오브젝트를 적 방향으로 회전
            fCanvasCtrl.AimingImgSet(true, target);                 // UI 세팅

            // 적 오브젝트와 플레이어 사이에 벽, 장애물 오브젝트가 있으면 사격 적중률 하락하도록 설정.
            LayerMask mask1 = LayerMask.GetMask("Wall");
            LayerMask mask2 = LayerMask.GetMask("Obstacles");
            Debug.DrawLine(characterBody.transform.position + new Vector3(0, 1.5f, 0), target.transform.position, Color.red, 5.0f);
                
            RaycastHit[] walls = Physics.RaycastAll(characterBody.transform.position + new Vector3(0, 1.5f, 0), dirToTarget, dstToTarget,mask1);
            RaycastHit[] obstacles = Physics.RaycastAll(characterBody.transform.position + new Vector3(0, 1.5f, 0), dirToTarget, dstToTarget, mask2);
            float rate = 100.0f;

            float minusPoint1 = obstacles.Length * 10;    // 장애물일 경우 (n * -10) 하락
            float minusPoint2 = walls.Length > 0 ? 0 : 1; // 벽일 경우는 전체 적중률 0 

            float result = (rate - minusPoint1) * minusPoint2;
            hitRate = result;
            currEnemy = target.gameObject;
            fCanvasCtrl.BoxTextSet("명중률 : " + result.ToString());
            if ((idx + 1) == targets.Count)
            {
                idx = 0;
            }
            else
            {
                idx++;
            }
        }
    }

    // 대상을 바라보는 코루틴 함수
    IEnumerator LookAtTarget(Transform target)
    {
        yield return null;
        Vector3 targetpos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        // 방향 벡터 설정
        // 첫 번째 리스트의 위치에서[즉, 처음부터 단계별로 이동] 내 자신의 위치를 뺀 벡턱값이 이동 방향이 된다.
        Vector3 dir = targetpos - transform.position;
        // 벡터 정규화
        dir.Normalize();
        float startTime = Time.time;
        while (Time.time <= startTime + 2.0f)
        {
            yield return null;
            characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, Quaternion.LookRotation(dir), Time.deltaTime * 5.0f);
        }
    }

    // 발포 함수
    public void OnShootMode()
    {
        if (!uiManager.CheckMagazine(true)) { return; }
        anim.SetTrigger("Fire");
        csSoundManager.PlayEffect(firePos.position, "ThreeShot");
        StartCoroutine(this.GunFlashCtrl());

        float rate = Random.Range(0, 101);  //0 ~ 100사이의 수를 램덤으로 생성 => 사격률 이하일 시 사격 성공
        if (rate <= hitRate)
        {   // 명중이면 적에게 데미지를 줌.
            Debug.DrawRay(firePos.position, firePos.forward * 37.0f, Color.red);
            object[] _paramas = new object[3];
            _paramas[0] = currEnemy.transform.position;
            _paramas[1] = 40;
            _paramas[2] = this.gameObject;

            currEnemy.SendMessage("OnCollision", _paramas, SendMessageOptions.DontRequireReceiver); 
            fCanvasCtrl.BoxTextSet("명중!!");
            fCanvasCtrl.BoxButtonSetActive(false);
        }
        else
        {
            fCanvasCtrl.BoxTextSet("빗나감..");
            fCanvasCtrl.BoxButtonSetActive(false);
        }
        // 턴 리셋, 종료 함수 실행
        Invoke("ResetTurn", 1.5f);       
        StartCoroutine(this.TurnOver(2.0f));
    }

    // 총구 화염 이펙트 함수
    IEnumerator GunFlashCtrl()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        muzzleFlash.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        muzzleFlash.SetActive(false);
    }

    //장전 모드 함수
    public void OnReloadMode()
    {
        if (menuSelected && reload != null) return;

        menuSelected = true;
        if(reload == null)
        {
            reload = this.Reloading();
            StartCoroutine(reload);
        }
        
    }
    //장전 코루틴 함수
    IEnumerator Reloading()
    {
        yield return null;
        anim.SetTrigger("Reload");
        uiManager.ReloadMagazine();  // UI에 있는 장전 함수 실행

        yield return new WaitForSeconds(2.0f);
        if (reload != null) { reload = null; }
        StartCoroutine(this.TurnOver(0f));
    }

    // 죽을 시 실행하되는 함수
    public void isDead()
    {
        StartCoroutine(this.Die());
    }

    IEnumerator Die()
    {
        anim.SetTrigger("Die");
        yield return new WaitForSeconds(2.5f);
        isDie = true;
    }

    //이동시 애니메이션 조절 함수
    public void MoveToward(bool isMoving)
    {
        anim.SetInteger("RunSpeed", isMoving ? 1 : -1);
    }

    //게임 재시작시 초기화 함수
    public void Restart()
    {
        isDie = false;
        transform.position = playerPos;
        GetComponent<playerLife>().LifeBarSet(100);
        anim.SetTrigger("Restart");
    }

    //턴 종류시 실행 함수
    IEnumerator TurnOver(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject.Find("StageManager").GetComponent<StageManager>().Reset();
    }
}
