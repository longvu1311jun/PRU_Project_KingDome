using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.Rotate(0, 0.6f, 0);
    }
}
