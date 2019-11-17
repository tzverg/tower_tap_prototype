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
    private bool _blockerGenStart;

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

        InitBrickLayers();
        CalculateMaxBricks();
        InitBrickPool();
        UpdateBrickPrewiews();
    }

    private void InitBrickLayers()
    {
        _gameConfig.BrickLayers = new System.Collections.Generic.List<int>();
        for (int cnt = 0; cnt < _gameConfig.EndGameTriggerY; cnt++)
        {
            _gameConfig.BrickLayers.Add(0);
        }
    }

    private void CalculateMaxBricks()
    {
        float circumference = 2 * Mathf.PI * _gameConfig.RotateRadius;
        float longestLSX = GetLongestPrefabLocalScaleX();
        float bricksOnRow = Mathf.Floor(circumference / longestLSX) - longestLSX * 0.5F;
        float maxBricks = bricksOnRow * (_gameConfig.EndGameTriggerY - 1);

        Debug.Log("rowB: " + bricksOnRow + ", maxB: " + maxBricks);

        _gameConfig.BricksOnRow = (int)bricksOnRow;
        _gameConfig.MaxBrick = (int)maxBricks;

        _uiController.UpdateBricksLable(_gameConfig.LockedBrick, _gameConfig.MaxBrick);
    }

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
            if (_gamePrefabs.BricksGO.Count > 0 && _gameConfig.BrickPool.Count > 0)
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

    public void CreateNewBlocker()
    {
        int blockerLayerID = (int) CalculateBlockerStartY();

        if (_gamePrefabs.BlockersGO.Count > 0 && _gameConfig.BrickLayers[blockerLayerID] == _gameConfig.BricksOnRow)
        {
            int randomBlockerID = Mathf.FloorToInt(UnityEngine.Random.Range(0F, _gamePrefabs.BlockersGO.Count));
            float randomRotAngle = Mathf.Floor(UnityEngine.Random.Range(0F, 360F));

            GameObject currentBlocker = Instantiate(
                                                        _gamePrefabs.BlockersGO[randomBlockerID],
                                                        _gameConfig.connComp.BricksParentTR
                                                    );

            BrickModel currentBrickM = currentBlocker.GetComponent<BrickModel>();
            if (currentBrickM != null)
            {
                currentBrickM.EndTriggerTR = _gameConfig.connComp.EndTriggerTR;
            }

            Vector3 blockerOffset = CalculateBlockerOffset(currentBlocker, blockerLayerID);

            currentBlocker.transform.position = _gameConfig.BrickPos - blockerOffset;
            currentBlocker.transform.RotateAround(Vector3.zero, Vector3.up, randomRotAngle);

            _gameConfig.LockedBlocker++;
        }
    }

    private Vector3 CalculateBlockerOffset(GameObject currentBlocker, float blockerOffsetY)
    {
        float blockerHalfHeight = currentBlocker.transform.localScale.y * 0.5F;
        float resultY = _gameConfig.EndGameTriggerY - blockerOffsetY - blockerHalfHeight - _gameConfig.connComp.FoundTR.localScale.y;

        Vector3 result = new Vector3(0F, resultY, 0F);

        return result;
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

        if (Input.GetMouseButtonUp(0))
        {
            _gameConfig.RequestedAngle = 0F;
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
            _rotateBrick = false;

            //_gameConfig.BrickPos = _currentBrickRB.transform.position;
            _gameConfig.BrickRot = _currentBrickRB.transform.rotation.eulerAngles;
            _gameConfig.SavedAngle = _currentBrick.transform.rotation.eulerAngles.y;

            SetRequestedAngle();

            //Debug.Log("SavedAngle: " + _gameConfig.SavedAngle);
            //Debug.Log("RequestedAngle: " + _gameConfig.RequestedAngle);
            //Debug.Log("_hackRotation: " + _hackRotation);

            _currentBrickRB.freezeRotation = true;
            _currentBrickRB.useGravity = true;
            _currentBrickRB.detectCollisions = true;

            CreateNewBrick();
        }
    }

    private void SetRequestedAngle()
    {
        float resultAngle = _gameConfig.SavedAngle + _gameConfig.MinAngle;
        if (resultAngle >= 360F)
        {
            resultAngle -= 360F;
            _hackRotation = true;
        }

        _gameConfig.RequestedAngle = resultAngle;
    }

    private bool CanDropBrick()
    {
        bool result = false;

        float currentBrickRotationAngle = _currentBrick.transform.rotation.eulerAngles.y;
        //float requestedBrickRotationAngle = _gameConfig.SavedAngle + _gameConfig.MinAngle;

        if (_hackRotation)
        {
            if (_gameConfig.RequestedAngle > currentBrickRotationAngle)
            {
                _hackRotation = false;
            }
        }

        if (!_hackRotation)
        {
            if (currentBrickRotationAngle > _gameConfig.RequestedAngle)
            {
                result = true;
            }
        }

        return result;
    }

    private void CheckGameState()
    {
        if (_gameConfig.LockedBrick >= _gameConfig.MaxBrick)
        {
            _endTheGame();
        }
        else
        {
            if (_gameConfig.LockedBrick % _gameConfig.BricksOnRow == 0) //TODO remove from here
            {
                //CreateRandomBlockersNum();
            }
        }
    }

    private void CreateRandomBlockersNum()
    {
        //for (int cnt = _gameConfig.LockedBlocker; cnt < _gameConfig.MaxBlocker; cnt++)
        //{
            float randomSeed = UnityEngine.Random.Range(0F, 1F);
            //if (randomSeed > 0.6F)
            {
                CreateNewBlocker();
            }
        //}
    }

    private float CalculateBlockerStartY()
    {
        float result = Mathf.Floor(_gameConfig.LockedBrick / _gameConfig.BricksOnRow);
        return result;
    }

    private void UpdateEndTriggerColor()
    {
        Material endTriggerMat = _gameConfig.connComp.EndTriggerTR.gameObject.GetComponent<MeshRenderer>().material;
        endTriggerMat.color = new Color(0F, 1F, 0F, endTriggerMat.color.a);
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

        SetBrickLable();

        UpdateBrickPrewiews();
        CheckGameState();
    }

    private void SetBrickLable()
    {
        Transform brickParentTR = _gameConfig.connComp.BricksParentTR;
        Transform lastLockedBrick = brickParentTR.GetChild(brickParentTR.childCount - 2);

        // I'm sorry х(
        int brickLayer = Mathf.FloorToInt(lastLockedBrick.position.y);

        _gameConfig.BrickLayers[brickLayer - 1]++;
    }
}
