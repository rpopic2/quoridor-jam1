using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static WallOrientation;

class Main : Singleton<Main>
{
    [SerializeField] Cell _cellPrefab;
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] Material _p2mat;
    [SerializeField] TMP_Text _winText;

    Transform _player1;
    Transform _player2;
    Transform _currentPlayer;

    HashSet<Wall> _walls = new();

    void Awake() {
        // TODO Test code!!
        _walls.Add(new(4, 0, H));
        SingletonInit(this);

        var rot = Quaternion.Euler(90f, 0f, 0f);
        int x = 0;
        int y = 0;
        for (int i = 0; i < 9 * 9; ++i) {
            var cell = Instantiate(_cellPrefab, new(x, 0f, y), rot);
            cell.Init(x, y);
            x += 1;
            if (x >= 9) {
                x = 0;
                y += 1;
            }
        }

        _player1 = Instantiate(_playerPrefab, new(4f, 0f, 0f), Quaternion.identity).transform;
        _player2 = Instantiate(_playerPrefab, new(4f, 0f, 8f), Quaternion.identity).transform;
        _player2.GetComponent<MeshRenderer>().material = _p2mat;

        _currentPlayer = _player1;
    }

    public bool WallExists(int x, int y, WallOrientation ori) {
        var exists = _walls.Contains(new Wall(x, y, ori));
        return exists;
    }

    public void OnCellClick(int x, int y) {
        print("click " + x + "," + y);
        var pos = _currentPlayer.position;
        var px = (int)pos.x;
        var py = (int)pos.z;
        print(pos);
        if (x == px && y == py + 1) { // move up
            // if 40h or 30h exists (when curpos is 40)
            if (WallExists(px, py, H) || WallExists(px - 1, py, H)) {
                return;
            }
            _currentPlayer.position = new(x, 0f, y);
        } else if (x == px && y == py - 1) { // move down
            _currentPlayer.position = new(x, 0f, y);
        } else if (x == px + 1 && y == py) { // move right
            _currentPlayer.position = new(x, 0f, y);
        } else if (x == px - 1 && y == py) { // move left
            _currentPlayer.position = new(x, 0f, y);
        } else {
            return;
        }

        if (_player1.BoardPos().y == 8) {
            _winText.text = "player1 wins!";
        } else if (_player2.BoardPos().y == 0) {
            _winText.text = "player2 wins!";
        }

        print("turn elapse");
        if (_currentPlayer == _player1) {
            _currentPlayer = _player2;
        } else if (_currentPlayer == _player2) {
            _currentPlayer = _player1;
        } else {
            throw new("invalid player");
        }
    }

    public void OnWallPlace(RaycastHit hit, Transform prefab, Quaternion rotation) {
        Instantiate(prefab, hit.point, rotation);
    }
}

enum WallOrientation {
    H, V
}

struct Wall {
    int x, y;
    WallOrientation ori;

    public Wall(int x, int y, WallOrientation ori) {
        this.x = x;
        this.y = y;
        this.ori = ori;
    }
}

