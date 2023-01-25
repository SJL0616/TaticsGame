using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour
{
    GameObject cols;
    // Start is called before the first frame update
    void Start()
    {
       

    }

    private void OnMouseEnter()
    {
        Debug.Log("MouseEnter");
    }

    private void OnMouseExit()
    {
        Debug.Log("MouseExit");
    }
}
