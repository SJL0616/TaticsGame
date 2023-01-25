using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
 작성자 : 이상준
 내용: 사운드 매니저 클래스
       기능 
           1. 게임 내의 효과음 재생
 마지막 수정일: 2023.01.06
 */
[RequireComponent(typeof(AudioSource))]
public class csSoundManager : MonoBehaviour
{
    public AudioClip[] soundFile;
    //오디오 소스 받아옴
    public AudioSource audio;


    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    private void Start()
    {
    }

    //효과음을 배열에서 찾아서 재생
    public void PlayEffect(Vector3 pos, string name)
    {
        Debug.Log("vector :" + pos + " , " + name);
        AudioClip sfx = null;
        sfx = GetSfx(name);
        GameObject _soundObj = new GameObject("sfx");

        AudioSource _audioSource = _soundObj.AddComponent<AudioSource>();
        _audioSource.clip = sfx;
        _audioSource.volume = 1;
        _audioSource.minDistance = 3.0f;
        _audioSource.spatialBlend = 1;
        _audioSource.maxDistance = 8.0f;

        _audioSource.playOnAwake = true;
        Instantiate(_soundObj, pos, Quaternion.identity);

        Destroy(_soundObj, sfx.length + 0.2f);
        // 사운드 길이 + 0.2f 후 오브젝트 삭제
        // 사운드 길이에 딱 맞추면 잘리는 경우 있으므로 0.2초정도 추가
    }

    AudioClip GetSfx(string name)
    {
        AudioClip sfx = null;
        for (int i = 0; i < soundFile.Length; i++)
        {
            if (soundFile[i].name.Contains(name))
            {
                sfx = soundFile[i];

            }
        }
        return sfx;
    }
}
