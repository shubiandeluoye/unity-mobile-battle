using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ShootButton : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button button;
    [SerializeField] private ShootDirection direction;
    
    private UIInputController inputController;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        inputController = GetComponentInParent<UIInputController>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnShootButtonClicked);
        }
    }

    private void OnShootButtonClicked()
    {
        if (!photonView.IsMine) return;
        
        // 通知UIInputController发射子弹
        if (inputController != null)
        {
            inputController.TriggerShoot(direction);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnShootButtonClicked);
        }
    }
}
