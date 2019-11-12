using System;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    private Config _gameConfig;
    private Prefabs _gamePrefabs;
    private UIController _uiController;

    private GameObject _currentBrick;
    private Rigidbody _currentBrickRB;
    private BrickModel _currentBrickM;

    private bool _hackRotation;
    private bool _rotateBrick;

    public void Init(Config gameConfig, Prefabs gamePrefabs, UIController uiController)
    {
        _gameConfig = gameConfig;
        _gamePrefabs = gamePrefabs;
        _uiController = uiController;

        InitScene();

        CreateNewBrick();
    }

    private void InitScene()
    {
        Transform EndTriggerTR = _gameConfig.connComp.EndTriggerTR;
        Transform FoundTR = _gameConfig.connComp.FoundTR;

        EndTriggerTR.position = new Vector3(0F, _gameConfig.EndGameTriggerY + FoundTR.position.y, 0F);
        _gameConfig.BrickPos = new Vector3(0F, _gameConfig.EndGameTriggerY + FoundTR.position.y, _gameConfig.RotateRadius);

        float triggerScale = (_gameConfig.RotateRadius * 2) * 1.1F;
        float foundScale = (_gameConfig.RotateRadius * 2) * 1.2F;

        _gameConfig.connComp.EndTriggerTR.localScale = new Vector3(triggerScale, EndTriggerTR.localScale.y, triggerScale);
        _gameConfig.connComp.FoundTR.localScale = new Vector3(foundScale, EndTriggerTR.localScale.y, foundScale);

        CalculateMaxBricks();
        //CalculateBrickAngle();
        InitBrickPool();
        UpdateBrickPrewiews();
    }

    private void CalculateMaxBricks()
    {
        float circumference = 2 * Mathf.PI * _gameConfig.RotateRadius;
        float longestLSX = GetLongestPrefabLocalScaleX();
        //float bricksOnRow = Mathf.Floor(circumference / longestLSX) - longestLSX;
        float bricksOnRow = Mathf.Floor(circumference / longestLSX) - longestLSX * 0.5F;
        //float maxBricks = bricksOnRow * (_gameConfig.EndGameTriggerY - 1) - _gameConfig.EndGameTriggerY;
        float maxBricks = bricksOnRow * (_gameConfig.EndGameTriggerY - 1);
        _gameConfig.MaxBrick = (int)maxBricks;

        _uiController.UpdateBricksLable(_gameConfig.LockedBrick, _gameConfig.MaxBrick);
    }

    //TODO
    //private void CalculateBrickAngle()
    //{
    //    _gameConfig.MinAngle = Vector3.Angle()
    //    Debug.LogError("_gameConfig.MinAngle: " + _gameConfig.MinAngle);
    //}

    private float GetLongestPrefabLocalScaleX()
    {
        float resultValue = 0F;

        foreach (GameObject target in _gamePrefabs.BricksGO)
        {
            if (target.transform.localScale.x > resultValue)
            {
                resultValue = target.transform.localScale.x;
            }
        }

        return resultValue;
    }

    private void InitBrickPool()
    {
        _gameConfig.BrickPool = new System.Collections.Generic.List<int>();

        for (int cnt = 0; cnt < _gameConfig.MaxBrick; cnt++)
        {
            int randomBrickID = UnityEngine.Random.Range(0, _gamePrefabs.BricksGO.Count);
            _gameConfig.BrickPool.Add(randomBrickID);
        }
    }

    private void UpdateBrickPrewiews()
    {
        Rect cameraViewport = _gameConfig.previewCamM.ViewportRect;
        float cameraOffset = cameraViewport.height + _gameConfig.previewCamM.OffsetY;

        for (int cnt = 0; cnt < _gameConfig.connComp.BricksPoolTR.childCount; cnt++)
        {
            GameObject target = _gameConfig.connComp.BricksPoolTR.GetChild(cnt).gameObject;
            Destroy(target);
        }

        for (int cnt = 0; cnt < _gameConfig.MaxBrickPreview; cnt++)
        {
            if ((cnt + 1) < _gameConfig.BrickPool.Count)
            {
                int brickPreviewID = _gameConfig.BrickPool[cnt + 1];

                GameObject currentBrickPreview = Instantiate(_gamePrefabs.BricksPreviewGO[brickPreviewID], _gameConfig.connComp.BricksPoolTR);
                currentBrickPreview.transform.position += _gameConfig.BrickPreviewOffset * cnt;
                Camera currentBPC = currentBrickPreview.GetComponentInChildren<Camera>();

                if (currentBPC != null)
                {
                    currentBPC.rect = new Rect
                    (
                        cameraViewport.x,
                        cameraViewport.y - cameraOffset * cnt,
                        cameraViewport.width,
                        cameraViewport.height
                    );
                }
            }
        }
    }

    public void CreateNewBrick()
    {
        CameraFollowController camFC = Camera.main.GetComponent<CameraFollowController>();

        if (camFC != null)
        {
            if (_gameConfig.BrickPool.Count > 0)
            {
                int newBrickID = _gameConfig.BrickPool[0];

                _currentBrick = Instantiate(_gamePrefabs.BricksGO[newBrickID], _gameConfig.connComp.BricksParentTR);
                _currentBrickRB = _currentBrick.GetComponent<Rigidbody>();
                _currentBrickM = _currentBrick.GetComponent<BrickModel>();

                _currentBrick.transform.position = _gameConfig.BrickPos;
                _currentBrickRB.useGravity = false;
                _currentBrickRB.detectCollisions = false;

                _currentBrickM.State = BrickState.IDLE;
                _currentBrickM.EndTriggerTR = _gameConfig.connComp.EndTriggerTR;
                _currentBrickM.OnEndTheGame += _endTheGame;
                _currentBrickM.OnAddLockedBrick += _addLockedBrick;

                camFC.SetCameraTagetTR(_currentBrick.transform, _gameConfig.CameraOffset);

                _currentBrick.transform.RotateAround(Vector3.zero, Vector3.up, _gameConfig.BrickRot.y);

                _rotateBrick = true;

                _gameConfig.BrickPool.RemoveAt(0);
            }
        }
    }

    void Update()
    {
        if (_rotateBrick)
        {
            RotateCurrentBrick();
        }

        if (Input.GetMouseButton(0))
        {
            DropCurrentBrick();
        }
    }

    private void RotateCurrentBrick()
    {
        // Spin the object around the world origin.
        _currentBrick.transform.RotateAround(Vector3.zero, Vector3.up, _gameConfig.RotateBrickSpeed * Time.deltaTime);
    }

    private void DropCurrentBrick()
    {
        if (CanDropBrick())
        {
            Debug.Log("kick");
            _rotateBrick = false;

            //_gameConfig.BrickPos = _currentBrickRB.transform.position;
            _gameConfig.BrickRot = _currentBrickRB.transform.rotation.eulerAngles;
            _gameConfig.SavedAngle = _currentBrick.transform.rotation.eulerAngles.y;

            _currentBrickRB.freezeRotation = true;
            _currentBrickRB.useGravity = true;
            _currentBrickRB.detectCollisions = true;

            CreateNewBrick();
        }
    }

    private bool CanDropBrick()
    {
        bool result = false;

        float currentBrickRotationAngle = _currentBrick.transform.rotation.eulerAngles.y;
        float requestedBrickRotationAngle = _gameConfig.SavedAngle + _gameConfig.MinAngle;

        if (requestedBrickRotationAngle >= 360F)
        {
            requestedBrickRotationAngle -= 360F;
            _hackRotation = true;
        }

        if (_hackRotation)
        {
            if (requestedBrickRotationAngle > currentBrickRotationAngle)
            {
                _hackRotation = false;
                result = true;
            }
        }
        else
        {
            if (currentBrickRotationAngle > requestedBrickRotationAngle)
            {
                result = true;
            }
        }

        return result;
    }

    private void _endTheGame()
    {
        UpdateEndTriggerColor();
        _uiController.ShowMenuPanel();
    }

    public void _addLockedBrick()
    {
        _gameConfig.LockedBrick++;
        _uiController.UpdateBricksLable(_gameConfig.LockedBrick, _gameConfig.MaxBrick);

        UpdateBrickPrewiews();

        CheckGameState();
    }

    private void CheckGameState()
    {
        if (_gameConfig.LockedBrick == _gameConfig.MaxBrick)
        {
            _endTheGame();
        }
    }

    private void UpdateEndTriggerColor()
    {
        Material endTriggerMat = _gameConfig.connComp.EndTriggerTR.gameObject.GetComponent<MeshRenderer>().material;
        endTriggerMat.color = new Color(0F, 1F, 0F, endTriggerMat.color.a);
    }
}
