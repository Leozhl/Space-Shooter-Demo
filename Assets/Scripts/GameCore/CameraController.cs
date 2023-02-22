using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 mPosOffset;

    // Start is called before the first frame update
    void Start()
    {
        mPosOffset = transform.position - PlayerController.Instance.transform.position;
    }

    private void LateUpdate()
    {
        float horizontalAngle = Vector3.Angle(Vector3.forward, PlayerController.Instance.transform.forward);
        if (Vector3.Cross(Vector3.forward, PlayerController.Instance.transform.forward).y < 0) horizontalAngle = -horizontalAngle;
        Vector3 curOffset = Quaternion.Euler(PlayerController.Instance.UpAngle, horizontalAngle, 0) * mPosOffset;
        transform.position = PlayerController.Instance.transform.position + curOffset;
        transform.rotation = Quaternion.LookRotation(PlayerController.Instance.CameraPosition.position - transform.position);
    }
}
