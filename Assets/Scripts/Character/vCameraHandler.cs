using Invector.vCamera;
using UnityEngine;

public class vCameraHandler : MonoBehaviour
{
    [SerializeField]
    private vThirdPersonCamera _tpCamera;

    [SerializeField]
    private Camera _camera;

    public vThirdPersonCamera TpCamera => _tpCamera;
    public Camera Camera => _camera;

    public void SetCamera(vThirdPersonCamera tpCamera, Camera camera)
    {
        _tpCamera = tpCamera;
        _camera = camera;
    }

    public void ClearCamera()
    {
        _tpCamera = null;
        _camera = null;
    }
}
