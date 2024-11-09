using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBillboard : MonoBehaviour
{
    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = CameraController.instance.transform.rotation;
    }
}
