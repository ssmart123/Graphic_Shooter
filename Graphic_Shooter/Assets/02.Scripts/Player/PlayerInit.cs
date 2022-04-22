using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SSM
{
    public class PlayerInit : MonoBehaviour
    {
        [Header("플레이어 이동속도 변수")]
        [SerializeField] protected float m_PlayerCrouchSpeed;
        [SerializeField] protected float m_PlayerWalkSpeed;
        [SerializeField] protected float m_PlayerRunSpeed;
        [SerializeField] protected float m_PlayerSprintSpeed;
        [SerializeField] protected float m_PlayerJumpForce;
        [Header("플레이어 HP")]
        [SerializeField] protected int m_PlayerHP = 200;

    }
}