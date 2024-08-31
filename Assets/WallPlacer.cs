using UnityEngine;

class WallPlacer : MonoBehaviour
{
    [SerializeField] Transform _wallGhost;

    Vector3 _wallGhostPos;
    Camera _cam;

    void Awake() {
        _cam = Camera.main;
        _wallGhostPos = _wallGhost.position;
    }

    bool _didHit;
    RaycastHit _hit;
    [SerializeField] Vector2 _test;

    void Update() {
        if (Input.GetMouseButtonUp(1)) {
            if (_didHit && _hit.point.z > 0f) {
                var x = (int)_hit.point.x;
                var y = (int)_hit.point.z;
                Main.Instance.OnWallPlace(new(x, y), _wallGhost.rotation);
            }
            ResetWallGhost();
        }
        if (!Input.GetMouseButton(1)) {
            _wallGhost.position = _wallGhostPos;
            return;
        }

        var screenPoint = Input.mousePosition;
        var ray = _cam.ScreenPointToRay(screenPoint);
        _didHit = Physics.Raycast(ray, out _hit);
        if (_didHit) {
            _wallGhost.position = _hit.point;
        } else {
            _wallGhost.position = _wallGhostPos;
        }
    }

    void ResetWallGhost() {
        _wallGhost.position = _wallGhostPos;
        var euler = _wallGhost.rotation.eulerAngles;
        euler.y += 90f;
        _wallGhost.rotation = Quaternion.Euler(euler);
    }
}

