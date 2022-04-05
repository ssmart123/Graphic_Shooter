using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCtrl : MonoBehaviour
{
    public GameObject sparkEffect;


    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 게임오브젝트의 태그값 비교
        if (collision.gameObject.tag == "BULLET")
        {
            GameObject spark = (GameObject)Instantiate(sparkEffect, collision.transform.position, Quaternion.identity);
            Destroy(spark, spark.GetComponent<ParticleSystem>().main.duration + 0.2f);

            // 충돌한 게임오브젝트 삭제
            Destroy(collision.gameObject);
        }
    }
}
