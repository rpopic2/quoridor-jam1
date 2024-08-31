using UnityEngine;

class Cell : MonoBehaviour
{
    int _x;
    int _y;

    void OnMouseUpAsButton() {
        Main.Instance.OnCellClick(_x, _y);
    }

    public void Init(int x, int y) {
        _x = x;
        _y = y;
    }
}

