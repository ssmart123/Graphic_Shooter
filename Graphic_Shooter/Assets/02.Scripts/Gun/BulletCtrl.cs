using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SSM;

public class BulletCtrl : MonoBehaviour
{
    [HideInInspector] public int m_BulletDmg;
    // 총알 발사 속도
    private float speed = 2000.0f;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * speed);

        Destroy(this.gameObject, 4.0f);
    }

}
