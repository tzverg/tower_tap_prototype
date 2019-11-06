using UnityEngine;

public class CameraFollowController : MonoBehaviour
{
    private Transform _targetTR;

    public void SetCameraTagetTR(Transform newTargetTR, Vector3 cameraOffset)
    {
        _targetTR = newTargetTR;
        transform.SetParent(_targetTR);
        transform.position = _targetTR.position + cameraOffset;
        transform.LookAt(_targetTR);
    }
}
