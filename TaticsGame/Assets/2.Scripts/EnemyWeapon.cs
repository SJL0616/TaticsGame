using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public int power = 5;
    Collider col;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            StartCoroutine(this.ResetColl());
        }
        
    }

    IEnumerator ResetColl()
    {
        col.enabled = false;
        yield return new WaitForSeconds(1.5f);
        col.enabled = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<CapsuleCollider>();
    }

}
