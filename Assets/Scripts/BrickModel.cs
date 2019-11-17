using System;
using UnityEngine;

public class BrickModel : MonoBehaviour
{
    private bool _freezeUpdate;
    private bool _freezeTriggerUpdate;
    private Transform _endTriggerTR;
    private Rigidbody _rigidBody;

    public BrickState State{ get; set; }
    public Transform EndTriggerTR { set { _endTriggerTR = value; } }

    public delegate void EndTheGame();
    public delegate void AddLockedBrick();

    public event EndTheGame OnEndTheGame;
    public event AddLockedBrick OnAddLockedBrick;

    private void Start()
    {
        _rigidBody = gameObject.GetComponent<Rigidbody>();

        if (_rigidBody == null)
        {
            Debug.LogError("_rigidBody is empty");
        }
    }

    private void Update()
    {
        if (!_freezeUpdate)
        {
            UpdateBrickState();
        }
    }

    private void UpdateBrickState()
    {
        switch (State)
        {
            case BrickState.IDLE:
                if (!_rigidBody.IsSleeping())
                {
                    State = BrickState.FALL;
                }
                break;
            case BrickState.FALL:
                if (_rigidBody.IsSleeping())
                {
                    State = BrickState.LOCK;
                }
                break;
            case BrickState.LOCK:
                OnAddLockedBrick?.Invoke();
                FreezeBlock();//TODO solve solution wrong calculate locked block (on last layers before win - OnAddLockedBrick do not invoked)
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_freezeTriggerUpdate)
        {
            if (State == BrickState.LOCK)
            {
                if (_endTriggerTR.Equals(other.transform))
                {
                    OnEndTheGame?.Invoke();
                }
                else
                {
                    _freezeTriggerUpdate = true;
                }
            }
        }
    }

    private void FreezeBlock()
    {
        _rigidBody.constraints = RigidbodyConstraints.FreezePositionX |
                                 RigidbodyConstraints.FreezePositionY | 
                                 RigidbodyConstraints.FreezePositionZ;

        _rigidBody.freezeRotation = true;

        _freezeUpdate = true;
    }
}