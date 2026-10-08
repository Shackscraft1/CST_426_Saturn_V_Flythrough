using UnityEngine;

public class ShotNavigation : MonoBehaviour
{
    [SerializeField] SplineFollow spiralCamera;
    [SerializeField] SplinePath spiralPath;
    [SerializeField] SplinePath spiralReturnPath;
    [SerializeField] SplineFollow spiralAim;
    [SerializeField] SplinePath spiralAimPath;
    [SerializeField] SplinePath spiralAimReturnPath;
    [SerializeField] GameObject flybyRoot;
    [SerializeField] SplineFollow flybyCamera;
    [SerializeField] SplinePath flybyPath;
    [SerializeField] SplinePath flybyReturnPath;
    [SerializeField] Transform flybyAim;
    [SerializeField] UnityEngine.UI.Button returnButton;
    [SerializeField] UnityEngine.UI.Button spiralButton;
    [SerializeField] UnityEngine.UI.Button flybyButton;

    enum Shot
    {
        Spiral,
        Flyby
    }

    Shot _currentShot;
    bool _moving;
    bool _returning;

    void Start()
    {
        flybyRoot.SetActive(true);
        flybyCamera.gameObject.SetActive(false);
        BeginSpiral(spiralPath, spiralAimPath, false);
    }

    void Update()
    {
        if (!_moving)
            return;

        SplineFollow camera = _currentShot == Shot.Spiral ? spiralCamera : flybyCamera;
        Vector3 end = camera.path.SamplePoint(camera.path.SegmentCount);
        if (Vector3.Distance(camera.transform.position, end) > 0.001f)
            return;

        _moving = false;
        returnButton.interactable = !_returning;
        spiralButton.interactable = true;
        flybyButton.interactable = true;
    }

    public void ReturnToStart()
    {
        if (_moving || _returning)
            return;

        if (_currentShot == Shot.Spiral)
            BeginSpiral(spiralReturnPath, spiralAimReturnPath, true);
        else
            BeginFlyby(flybyReturnPath, true);
    }

    public void PlaySpiral()
    {
        if (_moving)
            return;

        BeginSpiral(spiralPath, spiralAimPath, false);
    }

    public void PlayFlyby()
    {
        if (_moving)
            return;

        BeginFlyby(flybyPath, false);
    }

    void BeginSpiral(SplinePath cameraPath, SplinePath aimPath, bool returning)
    {
        flybyCamera.gameObject.SetActive(false);
        spiralCamera.gameObject.SetActive(true);
        spiralAim.enabled = true;
        spiralAim.path = aimPath;
        spiralAim.Restart();
        spiralCamera.path = cameraPath;
        spiralCamera.target = spiralAim.transform;
        spiralCamera.Restart();
        _currentShot = Shot.Spiral;
        BeginMovement(returning);
    }

    void BeginFlyby(SplinePath path, bool returning)
    {
        spiralAim.enabled = false;
        spiralCamera.gameObject.SetActive(false);
        flybyCamera.gameObject.SetActive(true);
        flybyCamera.path = path;
        flybyCamera.target = flybyAim;
        flybyCamera.Restart();
        _currentShot = Shot.Flyby;
        BeginMovement(returning);
    }

    void BeginMovement(bool returning)
    {
        _moving = true;
        _returning = returning;
        returnButton.interactable = false;
        spiralButton.interactable = false;
        flybyButton.interactable = false;
    }
}
