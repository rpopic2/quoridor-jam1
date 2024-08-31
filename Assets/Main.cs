using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static WallOrientation;
using static Direction;

class Player {
    const int WALL_MAX = 10;
    public Transform Transform;
    public GameObject MyTurn;
    public int WinY;

    public Queue<GameObject> WallInstances = new();

    public Vector3 position {
        get => Transform.position;
        set => Transform.position = value;
    }

    public Vector2Int BoardPos() {
        return new Vector2Int((int)Transform.position.x, (int)Transform.position.z);
    }

    public static Player New(Vector2Int pos, GameObject prefab, Material mat = null) {
        var player = new Player();
        player.Transform = GameObject.Instantiate(prefab, new(pos.x, 0f, pos.y), Quaternion.identity).transform;
        player.WinY = pos.y switch {
            0 => 8,
            8 => 0,
            _ => throw new("invalid start pos"),
        };
        if (mat != null)
            player.Transform.GetComponent<MeshRenderer>().material = mat;
        player.MyTurn = player.Transform.GetChild(0).gameObject;
        player.MyTurn.SetActive(false);

        var wallX = pos.y switch {
            0 => 10f,
            8 => -2f,
            _ => throw new(),
        };
        var wallPos = new Vector3(wallX, 0f, -0.5f);
        for (int i = 0; i < WALL_MAX; ++i) {
            var wall = GameObject.Instantiate(Main.Instance._wallPrefab, wallPos, Quaternion.identity);
            player.WallInstances.Enqueue(wall.gameObject);
            wallPos.z += 1f;
        }

        return player;
    }
}

class Main : Singleton<Main>
{
    [SerializeField] Cell _cellPrefab;
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] Material _p2mat;
    [SerializeField] TMP_Text _winText;
    [SerializeField] public Transform _wallPrefab;

    Player _player1;
    Player _player2;
    Player _currentPlayer;
    Player _opponentPlayer;

    int _player1WallCount;

    HashSet<Wall> _walls = new();

    void Awake() {
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

        _player1 = Player.New(new(4, 0), _playerPrefab);
        _player2 = Player.New(new(4, 8), _playerPrefab, _p2mat);

        _currentPlayer = _player1;
        _opponentPlayer = _player2;
        _currentPlayer.MyTurn.SetActive(true);
    }

    public bool WallExists(int x, int y, WallOrientation ori) {
        var exists = _walls.Contains(new Wall(x, y, ori));
        return exists;
    }

    bool CanMoveTo(Vector2Int pos, Direction dir) {
        int px = pos.x;
        int py = pos.y;
        if (dir is Up) { // move up
            if (WallExists(px, py, H) || WallExists(px - 1, py, H))
                return false;
        } else if (dir is Down) { // move down
            if (WallExists(px, py - 1, H) || WallExists(px - 1, py - 1, H))
                return false;
        } else if (dir is Right) { // move right
            if (WallExists(px, py - 1, V) || WallExists(px, py, V))
                return false;
        } else if (dir is Left) { // move left
            if (WallExists(px - 1, py - 1, V) || WallExists(px - 1, py, V))
                return false;
        } else {
            return false;
        }
        return true;
    }

    public void OnCellClick(int x, int y) {
        print("click " + x + "," + y);
        var pos = _currentPlayer.position;
        var px = (int)pos.x;
        var py = (int)pos.z;
        var p = new Vector2Int(px, py);

        var oppPos = _opponentPlayer.BoardPos();

        if (x == px && y == py + 2) { // jump over opp up
            if (oppPos != new Vector2(px, py + 1))
                return;
            if (!CanMoveTo(new(px, py), Up))
                return;
            if (!CanMoveTo(new(px, py + 1), Up))
                return;
        } else if (x == px && y == py - 2) { // jump over opp down
            if (oppPos != new Vector2(px, py - 1))
                return;
            if (!CanMoveTo(new(px, py), Down))
                return;
            if (!CanMoveTo(new(px, py - 1), Down))
                return;
        } else if (x == px + 2 && y == py) { // jump over opp right
            if (oppPos != new Vector2(px + 1, py))
                return;
            if (!CanMoveTo(new(px, py), Right))
                return;
            if (!CanMoveTo(new(px + 1, py), Right))
                return;
        } else if (x == px - 2 && y == py) { // jump over opp left
            if (oppPos != new Vector2(px - 1, py))
                return;
            if (!CanMoveTo(new(px, py), Left))
                return;
            if (!CanMoveTo(new(px - 1, py), Left))
                return;
        }

        else if (x == px && y == py + 1) { // move up
            if (WallExists(px, py, H) || WallExists(px - 1, py, H))
                return;
        } else if (x == px && y == py - 1) { // move down
            if (WallExists(px, py - 1, H) || WallExists(px - 1, py - 1, H))
                return;
        } else if (x == px + 1 && y == py) { // move right
            if (WallExists(px, py - 1, V) || WallExists(px, py, V))
                return;
        } else if (x == px - 1 && y == py) { // move left
            if (WallExists(px - 1, py - 1, V) || WallExists(px - 1, py, V))
                return;
        }

        else if (x == px - 1 && y == py + 1) { // move top left
            if (oppPos == p.Go(Up)) {
                if (CanMoveTo(p.Go(Up), Up))
                    return;
                if (!CanMoveTo(p, Up))
                    return;
                if (!CanMoveTo(p.Go(Up), Left))
                    return;
            } else if (oppPos == p.Go(Left)) {
                if (CanMoveTo(p.Go(Left), Left))
                    return;
                if (!CanMoveTo(p, Left))
                    return;
                if (!CanMoveTo(p.Go(Left), Up))
                    return;
            } else {
                return;
            }
        } else if (x == px + 1 && y == py + 1) { // move top right
            if (oppPos == p.Go(Up)) {
                if (CanMoveTo(p.Go(Up), Up))
                    return;
                if (!CanMoveTo(p, Up))
                    return;
                if (!CanMoveTo(p.Go(Up), Right))
                    return;
            } else if (oppPos == p.Go(Right)) {
                if (CanMoveTo(p.Go(Right), Right))
                    return;
                if (!CanMoveTo(p, Right))
                    return;
                if (!CanMoveTo(p.Go(Right), Up))
                    return;
            } else {
                return;
            }
        } else if (x == px - 1 && y == py - 1) { // move bottom left
            if (oppPos == p.Go(Down)) {
                if (CanMoveTo(p.Go(Down), Down))
                    return;
                if (!CanMoveTo(p, Down))
                    return;
                if (!CanMoveTo(p.Go(Down), Left))
                    return;
            } else if (oppPos == p.Go(Left)) {
                if (CanMoveTo(p.Go(Left), Left))
                    return;
                if (!CanMoveTo(p, Left))
                    return;
                if (!CanMoveTo(p.Go(Left), Down))
                    return;
            } else {
                return;
            }
        } else if (x == px + 1 && y == py - 1) { // move bottom right
            if (oppPos == p.Go(Down)) {
                if (CanMoveTo(p.Go(Down), Down))
                    return;
                if (!CanMoveTo(p, Down))
                    return;
                if (!CanMoveTo(p.Go(Down), Right))
                    return;
            } else if (oppPos == p.Go(Right)) {
                if (CanMoveTo(p.Go(Right), Right))
                    return;
                if (!CanMoveTo(p, Right))
                    return;
                if (!CanMoveTo(p.Go(Right), Down))
                    return;
            } else {
                return;
            }
        }

        else {
            return;
        }
        _currentPlayer.position = new(x, 0f, y);

        if (_player1.BoardPos().y == 8) {
            _winText.text = "player1 wins!";
        } else if (_player2.BoardPos().y == 0) {
            _winText.text = "player2 wins!";
        }

        ElapseTurn();
    }

    public void OnWallPlace(Vector2Int pos, Quaternion rotation) {
        if (_currentPlayer.WallInstances.Count <= 0) {
            print("no walls remaining!");
            return;
        }

        var euler = rotation.eulerAngles;
        var ori = ((int)euler.y % 180) switch {
            0 => H,
            90 => V,
            _ => throw new()
        };
        var other_rot = ori switch {
            H => V,
            V => H,
            _ => throw new()
        };
        var x = pos.x;
        var y = pos.y;

        if (_walls.Contains(new(x, y, other_rot)))
            return;
        if (_walls.Contains(new(x, y, ori)))
            return;
        if (ori is H) {
            if (_walls.Contains(new(x - 1, y, ori)))
                return;
            if (_walls.Contains(new(x + 1, y, ori)))
                return;
        } else if (ori is V) {
            if (_walls.Contains(new(x, y + 1, ori)))
                return;
            if (_walls.Contains(new(x, y - 1, ori)))
                return;
        } else {
            throw new();
        }

        if (x >= 8 || y >= 8)
            return;

        var newWall = new Wall(x, y, ori);
        _walls.Add(newWall);
        if (!DFS(_opponentPlayer) || !DFS(_currentPlayer)) {
            print("dfs fail!");
            _walls.Remove(newWall);
            return;
        }

        Instantiate(_wallPrefab, new(x + 0.5f, 0.5f, y + 0.5f), rotation);
        print($"new wall: {x}, {y}, {ori}");

        _currentPlayer.WallInstances.Dequeue().SetActive(false);

        ElapseTurn();
    }

    void ElapseTurn() {
        print("turn elapse");
        (_currentPlayer, _opponentPlayer) = (_opponentPlayer, _currentPlayer);
        _currentPlayer.MyTurn.SetActive(true);
        _opponentPlayer.MyTurn.SetActive(false);
    }

    HashSet<Vector2Int> _visited = new();
    Stack<Vector2Int> _queue = new();

    bool DFS(Player player) {
        var start = player.BoardPos();
        var endY = player.WinY;
        print("endY: " +endY);

        _visited.Clear();
        _queue.Clear();

        _visited.Add(start);
        _queue.Push(start);
        int tmp_counter = 0;

        while (_queue.Count > 0) {
            var cur = _queue.Pop();
            ++tmp_counter;
            if (cur.y == endY) {
                print(cur + $"dfs end! ({tmp_counter})");
                return true;
            }

            foreach (var d in _directions) {
                var target = cur.Go(d);
                if (!_visited.Contains(target)) {
                    if (!target.IsValid())
                        continue;
                    if (CanMoveTo(cur, d)) {
                        _queue.Push(target);
                        _visited.Add(cur);
                    }
                }
            }
        }
        return false;
    }

    readonly List<Direction> _directions = new() {
        Up, Left, Down, Right
    };
}

enum WallOrientation {
    H, V
}

public enum Direction {
    Up, Down, Left, Right
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

