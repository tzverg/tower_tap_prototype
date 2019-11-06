using UnityEngine;

public class BrickModel : MonoBehaviour
{
    private Transform _endTriggerTR;

    public BrickState State{ get; set; }
    public Transform EndTriggerTR { set { _endTriggerTR = value; } }

    public delegate void EndTheGame();
    public event EndTheGame OnEndTheGame;

    private void OnTriggerStay(Collider other)
    {
        if (State == BrickState.LOCK && _endTriggerTR.Equals(other.transform))
        {
            OnEndTheGame?.Invoke();
        }
    }
}