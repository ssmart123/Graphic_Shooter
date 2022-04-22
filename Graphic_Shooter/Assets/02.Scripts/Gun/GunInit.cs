using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSM
{
    public class GunInit : MonoBehaviour
    {
        [Header("총기 제원")]
        [SerializeField] protected string m_gunName;
        [SerializeField] protected float m_range;
        [SerializeField] private int m_damage;
        [SerializeField] protected float m_reloadTime;
        [SerializeField] protected float m_fireRate = 0.3f;  // 총기 딜레이 

        [Header("탄약 관련")]
        [SerializeField] protected int m_bulletReloadCount;
        [SerializeField] protected int m_bulletcurrentCount;

        [Header("필요 컴포넌트 연결")]
        [SerializeField] protected GameObject bulletPrefab;// 총알 프리팹
        [SerializeField] protected Transform firePos;  // 총알이 발사되는 위치
        [SerializeField] protected AudioClip fireSEClip;  // 발사 사운드
        [SerializeField] protected GameObject muzzleFlash;



        public int damageP
        {
            get { return m_damage; }
            set { m_damage = value; }
        }

    }
}