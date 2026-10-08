using UnityEngine;

public class ShotNavigation : MonoBehaviour
{
    [SerializeField] SplineFollow camera;
    [SerializeField] SplinePath spiralPath;
    [SerializeField] SplinePath spiralReturnPath;
    [SerializeField] SplineFollow spiralAim;
    [SerializeField] SplinePath spiralAimPath;
    [SerializeField] SplinePath spiralAimReturnPath;
    [SerializeField] GameObject flybyRoot;
    [SerializeField] GameObject flybyCamera;
    [SerializeField] SplinePath flybyPath;
    [SerializeField] Transform flybyAim;
    [SerializeField] float flybySpeed = 7f;
    [SerializeField] SplinePath spiralEndToFlybyPath;
    [SerializeField] SplinePath spiralStartToFlybyPath;
    [SerializeField] SplinePath flybyEndToSpiralPath;
    [SerializeField] SplinePath flybyStartToSpiralPath;
    [SerializeField] SplinePath spiralEndToFlybyAimPath;
    [SerializeField] SplinePath spiralStartToFlybyAimPath;
    [SerializeField] SplinePath flybyToSpiralAimPath;
    [SerializeField] UnityEngine.UI.Button returnButton;
    [SerializeField] UnityEngine.UI.Button spiralButton;
    [SerializeField] UnityEngine.UI.Button flybyButton;

    enum Shot
    {
        None,
        Spiral,
        Flyby
    }

    enum Location
    {
        SpiralStart,
        SpiralEnd,
        FlybyStart,
        FlybyEnd
    }

    float _spiralSpeed;
    Location _location;
    Location _destination;
    Shot _nextShot;
    bool _moving;

    void Start()
    {
        _spiralSpeed = camera.speed;
        flybyRoot.SetActive(true);
        flybyCamera.SetActive(false);
        _location = Location.SpiralStart;
        BeginSpiral();
    }

    void Update()
    {
        if (!_moving)
            return;

        Vector3 end = camera.path.SamplePoint(camera.path.SegmentCount);
        if (Vector3.Distance(camera.transform.position, end) > 0.001f)
            return;

        _location = _destination;
        if (_nextShot == Shot.Spiral)
        {
            BeginSpiral();
            return;
        }

        if (_nextShot == Shot.Flyby)
        {
            BeginFlyby();
            return;
        }

        _moving = false;
        returnButton.interactable = _location == Location.SpiralEnd || _location == Location.FlybyEnd;
        spiralButton.interactable = _location != Location.SpiralEnd;
        flybyButton.interactable = _location != Location.FlybyEnd;
    }

    public void ReturnToStart()
    {
        if (_moving)
            return;

        if (_location == Location.SpiralEnd)
            BeginSpiralPath(spiralReturnPath, spiralAimReturnPath, Location.SpiralStart, Shot.None);
        else if (_location == Location.FlybyEnd)
            BeginTransition(flybyEndToSpiralPath, flybyToSpiralAimPath, Location.SpiralStart, Shot.None);
    }

    public void PlaySpiral()
    {
        if (_moving || _location == Location.SpiralEnd)
            return;

        if (_location == Location.SpiralStart)
            BeginSpiral();
        else if (_location == Location.FlybyStart)
            BeginTransition(flybyStartToSpiralPath, flybyToSpiralAimPath, Location.SpiralStart, Shot.Spiral);
        else
            BeginTransition(flybyEndToSpiralPath, flybyToSpiralAimPath, Location.SpiralStart, Shot.Spiral);
    }

    public void PlayFlyby()
    {
        if (_moving || _location == Location.FlybyEnd)
            return;

        if (_location == Location.FlybyStart)
            BeginFlyby();
        else if (_location == Location.SpiralStart)
            BeginTransition(spiralStartToFlybyPath, spiralStartToFlybyAimPath, Location.FlybyStart, Shot.Flyby);
        else
            BeginTransition(spiralEndToFlybyPath, spiralEndToFlybyAimPath, Location.FlybyStart, Shot.Flyby);
    }

    void BeginSpiral()
    {
        BeginSpiralPath(spiralPath, spiralAimPath, Location.SpiralEnd, Shot.None);
    }

    void BeginFlyby()
    {
        BeginFlybyPath(flybyPath, Location.FlybyEnd, Shot.None);
    }

    void BeginSpiralPath(SplinePath cameraPath, SplinePath aimPath, Location destination, Shot nextShot)
    {
        camera.speed = _spiralSpeed;
        spiralAim.enabled = true;
        spiralAim.path = aimPath;
        spiralAim.speed = aimPath.TotalLength * camera.speed / cameraPath.TotalLength;
        spiralAim.Restart();
        camera.path = cameraPath;
        camera.target = spiralAim.transform;
        camera.Restart();
        BeginMovement(destination, nextShot);
    }

    void BeginFlybyPath(SplinePath path, Location destination, Shot nextShot)
    {
        camera.speed = flybySpeed;
        spiralAim.enabled = false;
        camera.path = path;
        camera.target = flybyAim;
        camera.Restart();
        BeginMovement(destination, nextShot);
    }

    void BeginTransition(SplinePath cameraPath, SplinePath aimPath, Location destination, Shot nextShot)
    {
        camera.speed = nextShot == Shot.Spiral ? _spiralSpeed : flybySpeed;
        spiralAim.enabled = true;
        spiralAim.path = aimPath;
        spiralAim.speed = aimPath.TotalLength * camera.speed / cameraPath.TotalLength;
        spiralAim.Restart();
        camera.path = cameraPath;
        camera.target = spiralAim.transform;
        camera.Restart();
        BeginMovement(destination, nextShot);
    }

    void BeginMovement(Location destination, Shot nextShot)
    {
        _destination = destination;
        _nextShot = nextShot;
        _moving = true;
        returnButton.interactable = false;
        spiralButton.interactable = false;
        flybyButton.interactable = false;
    }
}
