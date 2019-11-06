using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private Text _bricksLable;

    public void UpdateBricksLable(int value, int max)
    {
        _bricksLable.text = string.Format("Bricks: {0} / {1}", value, max);
    }

    public void ShowMenuPanel()
    {
        Time.timeScale = 0;
        _menuPanel.SetActive(true);
    }
}
