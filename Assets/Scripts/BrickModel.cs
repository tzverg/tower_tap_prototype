using System;
using UnityEngine;

public class BrickModel : MonoBehaviour
{
    private bool _freezeUpdate;
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
        UpdateBrickState();
    }

    private void UpdateBrickState()
    {
        if (!_freezeUpdate)
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
                    _freezeUpdate = true;
                    OnAddLockedBrick?.Invoke();
                    break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (State == BrickState.LOCK)
        {
            if (_endTriggerTR.Equals(other.transform))
            {
                OnEndTheGame?.Invoke();
            }
        }
    }
}