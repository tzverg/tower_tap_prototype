using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BrickState { IDLE, FALL, LOCK }

[Serializable]
public class Prefabs
{
    internal object brickPrefabGO;
    [SerializeField] private List<GameObject> _bricksGO;
    [SerializeField] private List<GameObject> _blockersGO;
    [SerializeField] private List<GameObject> _bricksPreviewGO;

    public List<GameObject> BricksGO => _bricksGO;
    public List<GameObject> BlockersGO => _blockersGO;
    public List<GameObject> BricksPreviewGO => _bricksPreviewGO;
}

[Serializable]
public class PreviewCameraModel
{
    [SerializeField] private Rect _viewportRect;
    [SerializeField] private float _offsetY;

    public Rect ViewportRect { get { return _viewportRect; } }
    public float OffsetY { get { return _offsetY; } }
}

[Serializable]
public class ConnectedComponents
{
    [SerializeField] private Transform _foundTR;
    [SerializeField] private Transform _endTriggerTR;
    [SerializeField] private Transform _bricksParentTR;
    [SerializeField] private Transform _bricksPoolTR;

    public Transform BricksParentTR { get { return _bricksParentTR; } }
    public Transform BricksPoolTR { get { return _bricksPoolTR; } }
    public Transform EndTriggerTR { get { return _endTriggerTR; } set { _endTriggerTR = value; } }
    public Transform FoundTR { get { return _foundTR; } set { _foundTR = value; } }
}

[Serializable]
public class Config
{
    public PreviewCameraModel previewCamM;
    public ConnectedComponents connComp;

    [SerializeField] public int _maxBrickPreview;

    [SerializeField] public float _rotateBrickSpeed;
    [SerializeField] public float _rotateRadius;
    [SerializeField] public float _endGameTriggerY;
    /*[SerializeField]*/ public float _savedAngle;
    [SerializeField] public float _minAngle;

    [SerializeField] private Vector3 _cameraOffset;
    [SerializeField] private Vector3 _brickPreviewOffset;

    private int _lockedBrick;
    private int _maxBrick;

    private float _requestedBrickRot;

    private Vector3 _brickPos;
    private Vector3 _brickRot;

    /*[SerializeField]*/
    private List<int> _brickPool;

    public int LockedBrick { get { return _lockedBrick; } set { _lockedBrick = value; } }
    public int MaxBrick { get { return _maxBrick; } set { _maxBrick = value; } }
    public int MaxBrickPreview { get { return _maxBrickPreview; }}

    public float SavedAngle { get { return _savedAngle; } set { _savedAngle = value; } }
    public float MinAngle { get { return _minAngle; } set { _minAngle = value; } }
    public float RotateBrickSpeed { get { return _rotateBrickSpeed; } }
    public float RotateRadius { get { return _rotateRadius; } }
    public float EndGameTriggerY { get { return _endGameTriggerY; } }

    public Vector3 BrickPos { get { return _brickPos; } set { _brickPos = value; } }
    public Vector3 BrickRot { get { return _brickRot; } set { _brickRot = value; } }
    public Vector3 CameraOffset { get { return _cameraOffset; } }
    public Vector3 BrickPreviewOffset { get { return _brickPreviewOffset; } }

    public List<int> BrickPool { get { return _brickPool; } set { _brickPool = value; } }
}

public class CoreGameplayController : MonoBehaviour
{
    public Prefabs prefabs;
    public Config config;

    private LevelGenerator _levelGen;

    void Start()
    {
        UIController uiController = GetComponent<UIController>();
        _levelGen = GetComponent<LevelGenerator>();

        if (_levelGen != null && uiController != null)
        {
            _levelGen.Init(config, prefabs, uiController);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}