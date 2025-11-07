using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using FoodSort;
using UnityEngine;

public class Plate : MonoBehaviour
{
    private const int NUM_POINT = 5;
    private const float SPACE = 0.4f;
    private const float TOP = 1f;
    private const float TIME_MOVE = 0.8f;

    [SerializeField] private SpriteRenderer _background;
    [SerializeField] private Transform _skewerStorage;
    private List<Skewer> _skewers = new List<Skewer>();
    private List<Vector2> _posTarget = new List<Vector2>();

    private SoundManager _soundManager;

    private Vector3 _initPos;
    private Vector3 _topPos;

    private Tween _tweenMove;

    private int _skewerOnPlate;
    private int _skewerAllOnPlate;
    private bool _isPlateChange;

    public List<Vector2> PosTarget { get => _posTarget; }

    void Awake()
    {
        _soundManager = SoundManager.Instance;
        // LevelManager.Instance.OnNumStove += SpritePointsOnX;
    }
    void Start()
    {
        InitMovePlate();
    }
    public void SpritePointsOnX(int numPoint)
    {
        if (numPoint > NUM_POINT) numPoint = NUM_POINT;

        _posTarget.Clear();

        Vector3 center = new Vector3(0, 2.88f, 0);

        for (int i = 0; i < numPoint; i++)
        {
            float offset = (i - (numPoint - 1) / 2f) * SPACE;
            Vector3 point = center + new Vector3(offset, 0f, 0f);

            _posTarget.Add(new Vector2(point.x, point.y));
        }
    }
    void InitMovePlate()
    {
        _initPos = this.transform.position;
        _topPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f, Camera.main.nearClipPlane));
        this.transform.position = new Vector3(0, _topPos.y + TOP, 0);
        _ = PlateMove(_initPos.y);

    }
    public async Task PlateMove(float yAxis)
    {
        _tweenMove?.Kill();

        _tweenMove = this.transform.DOMoveY(yAxis, TIME_MOVE).SetEase(Ease.OutBack);

        _soundManager.PlayDishMove();
        await _tweenMove.AsyncWaitForCompletion();
    }
    public void SubtractSkewerOnPlate()
    {
        if (_skewers.Count == 0) return;
        _skewers.RemoveAt(_skewers.Count - 1);
        _skewerOnPlate = _skewers.Count;
    }
    public async void AddSkewerOnPlate(Skewer skewer)
    {
        if (_skewers.Count < _posTarget.Count)
        {
            _isPlateChange = true;
            _skewers.Add(skewer);
            skewer.transform.parent = _skewerStorage;

            await skewer.SkewerAnimMoveInPlate(_posTarget[_skewers.Count - 1], false);

            _soundManager.PlaySkewerDone();
            _skewerOnPlate++;

            if (_skewerOnPlate == _posTarget.Count)
            {
                LevelManager.Instance.WinLevel();
                await EatFoodOnPlate();
                CheckAllStove();
                _isPlateChange = false;
            }
            else _isPlateChange = false;
        }
    }
    public async Task EatFoodOnPlate()
    {
        await PlateMove(_topPos.y + TOP);

        ResetFoodOnPlate();

        await PlateMove(_initPos.y);

    }
    public void CheckAllStove()
    {
        List<Skewer> skewers = LevelManager.Instance.GetSkewerDone();

        foreach (Skewer skewer in skewers)
        {
            AddSkewerOnPlate(skewer);
        }
    }
    public bool CheckUndo()
    {
        return !_isPlateChange;
    }
    public void ResetFoodOnPlate()
    {
        for (int i = 0; i < _skewers.Count; i++)
        {
            _skewers[i].gameObject.SetActive(false);
        }

        _skewerAllOnPlate += _skewerOnPlate;
        _skewerOnPlate = 0;

        SpritePointsOnX(LevelManager.Instance.SkewerHaveFood - _skewerAllOnPlate);

        _skewers.Clear();
    }
}
