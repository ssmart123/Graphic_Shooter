using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInit : MonoBehaviour
{
    public int hp = 100;
    private int CurHp;
    public Image imgHpbar;

    // Start is called before the first frame update
    void Start()
    {
        CurHp = hp;
    }


    private void OnTriggerEnter(Collider other)
    {
        //충돌한 Collider가 몬스터의 PUNCH이면 Player의 HP 차감
        if (other.gameObject.tag == "PUNCH")
        {
            if (CurHp <= 0.0f)
                return;

            CurHp -= 5;

            //Image UI 항목의 fillAmount 속성을 조절해 생명 게이지 값 조절

            imgHpbar.fillAmount = (float)CurHp / (float)hp;

            //Player의 생명이 0이하이면 사망 처리
            if (CurHp <= 0)
            {
                PlayerDie();
            }
        }
    }

    void PlayerDie()
    {
       // Debug.Log("Player Die !!");

        //MONSTER라는 Tag를 자진 모든 게임오브젝트를 찾아옴
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
        foreach (GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        }
        Animator a_Animator= gameObject.GetComponent<Animator>();
        a_Animator.SetLayerWeight(1, 0);
        a_Animator.SetLayerWeight(2, 0);
        a_Animator.SetTrigger("doDie");

        GameMgr.s_GameState = GameState.GameEnd;
    }
}
