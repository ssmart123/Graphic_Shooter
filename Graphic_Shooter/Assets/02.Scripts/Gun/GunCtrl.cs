using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SSM
{
    [RequireComponent(typeof(AudioSource))]
    public class GunCtrl : GunInit
    {
        private AudioSource audioSource = null;

        public float FireRateP
        {
            get { return m_fireRate; }
        }


        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Start is called before the first frame update
        void Start()
        {
            muzzleFlash.SetActive(false);
        }



        public void Fire()
        {
            //Bullet 프리팹을 동적으로 생성
            GameObject a_Bullet = Instantiate(bulletPrefab, firePos.position, firePos.rotation);
            a_Bullet.GetComponent<BulletCtrl>().m_BulletDmg = damageP;

            //사운드 발생 함수
            audioSource.PlayOneShot(fireSEClip, 0.2f);

            //잠시 기다리는 루틴을 위해 코루틴 함수로 호출
            StartCoroutine(this.ShowMuzzleFlash());

        }

        //MuzzleFlash 활성/비활성화를 짧은 시간 동안 반복
        IEnumerator ShowMuzzleFlash()
        {
            // 총구 화염의 크기를 랜덤으로 바꾸기
            float scale = Random.Range(1.0f, 2.0f);
            muzzleFlash.transform.localScale = Vector3.one * scale;

            // 총구화염을 랜덤한 각도로 회전시키기
            Quaternion rot = Quaternion.Euler(0, 0, Random.Range(0, 360));
            muzzleFlash.transform.localRotation = rot;

            //활성화 해서 보이게 함
            muzzleFlash.SetActive(true);
            //불규칙적인 시간 동안 Delay한 다음 MeshRenderer를 비활성화
            yield return new WaitForSeconds(Random.Range(0.01f, 0.03f));

            //비활성화해서 보이지 않게 함
            muzzleFlash.SetActive(false);

        }

       
    }
}