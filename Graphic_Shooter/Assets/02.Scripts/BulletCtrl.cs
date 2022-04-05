using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    // 총알의 파괴력
    public int damage = 20;

    // 총알 발사 속도
    public float speed = 1000.0f;

    // Start is called before the first frame update
    void Start()
    {
        speed = 3000.0f;

        GetComponent<Rigidbody>().AddForce(transform.forward * speed);

        Destroy(this.gameObject, 4.0f);
    }
}
