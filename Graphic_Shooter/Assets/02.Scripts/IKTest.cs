using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 본을 인스펙터창에 표시하기 위한 클래스
[System.Serializable]
public class HumanBone
{
    public HumanBodyBones bone;
    public float weight = 1.0f;
}

public class IKTest : MonoBehaviour
{
    [Header("WeaponHandle")]
    public Transform LeftHandle;
    public Transform RightHandle;

    public Transform AimSpot;
    public Vector3 CheastOffset;
    public Vector3 HeadOffset;

    private Transform spine;
    private Transform head;



    private Animator playerAnimator;

    public Transform targetTransform;
    public Transform aimTransform;
    public Transform Headbone;

    public int iterations = 10;   // 반복횟수
    public float weight = 1.0f;

    public HumanBone[] humanBones;  // 본의 갯수
    Transform[] boneTransforms;  // 설정한 본에 해당되는 트렌스폼

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();

        spine = playerAnimator.GetBoneTransform(HumanBodyBones.Spine);


        boneTransforms = new Transform[humanBones.Length]; // boneTransforms 배열의 크기 선언

        for (int i = 0; i < boneTransforms.Length; i++)
        {
            boneTransforms[i] = playerAnimator.GetBoneTransform(humanBones[i].bone);  // 본의 transform 정의
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {

        if (GameMgr.s_GameState == GameState.GameEnd) 
            return;


        // 견착
        Vector3 targetPosition = targetTransform.position;  // 타겟의 위치를 가져온다.


        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < boneTransforms.Length-1; j++)
            {
                Transform bone = boneTransforms[j];
                float boneWeight = humanBones[j].weight * weight;
                AimAtTarget(bone, aimTransform, targetPosition, boneWeight);
            }

            Transform hashs = boneTransforms[4];

            AimAtTarget(hashs, Headbone, targetPosition,weight);
        }

    }

    // 
    private void AimAtTarget(Transform bone,Transform aimPosition, Vector3 targetPosition, float weight)
    {
       // Vector3 aimDirection = aimTransform.forward;    // 에임의 전방벡터
        Vector3 aimDirection = aimPosition.forward;    // 에임의 전방벡터
        Vector3 targetDirection = targetPosition - aimPosition.position; // 타겟 위치과 에임 위치를 빼서 타겟 방향을 구함
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);  // 에임이 바라볼 방향
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
        bone.rotation = blendedRotation * bone.rotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(aimTransform.position, targetTransform.position);
    }

    private void OnAnimatorIK(int layerIndex)
    {

        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        // 팔 IK애니메이션 설정
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);


        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand,LeftHandle.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand,LeftHandle.rotation);

        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand,RightHandle.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand,RightHandle.rotation);

    }
}
