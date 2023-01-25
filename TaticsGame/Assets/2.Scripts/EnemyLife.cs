using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 작성자 : 이상준
 내용: 적 라이프 컨트롤 클래스
       기능 
           1. 데미지 입는 로직
           2. 바닥에 핏자국 생성
 마지막 수정일: 2023.01.06
 */
public class EnemyLife : MonoBehaviour
{
    private int life = 100;        // 전체 라이프 포인트 : 100
    private Transform myTr;        // 자신의 Transform 변수
    public GameObject bloodEffet;  // 블러드 효과용 GameObj
    public Transform bloodDecal;   // 블러드 효과용 TR
    public EnemyCtrl enemyCtrl;    // 적 컨트롤러 스크립트
    public SpriteRenderer lifeBar; // UI LifeBar 컨트롤용 변수

    // Start is called before the first frame update
    void Start()
    {
        myTr = GetComponent<Transform>();
    }

    // 데미지 입는 함수
    void OnCollision(object[] _params)
    {
        StartCoroutine(this.CreateBlood(transform.position));  // 바닥에 핏자국 생성

        enemyCtrl.player = (GameObject)_params[2];
        life -= (int)_params[1];           // LifeBar를 데미지만큼 줄이기
        lifeBar.material.SetFloat("_Progress", life / 100f);
        if (life <= 0)
        {                                  // life 포인트가 0이면 죽는 함수 실행
            enemyCtrl.isDead();
        }
    }

    // 자신의 발 아래에 핏자국 생성 함수
    IEnumerator CreateBlood(Vector3 pos)
    {
        GameObject enemyBlood = Instantiate(bloodEffet, pos, Quaternion.identity) as GameObject;
        Destroy(enemyBlood, 1.5f);

        Quaternion decalRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
        float scale = Random.Range(1.0f, 2.5f);

        Transform enemyBlood2 = Instantiate(bloodDecal, myTr.position, decalRot) as Transform;
        enemyBlood2.localScale = Vector3.one * scale;
        Destroy(enemyBlood2.gameObject, 1.5f);
        yield return null;

    }

}
