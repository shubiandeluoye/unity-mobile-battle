using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SpecialAttackButton : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color availableColor = new Color(1f, 0.3f, 0.3f, 0.8f);
    [SerializeField] private Color unavailableColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    private UIInputController inputController;
    private bool isAvailable = false;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        inputController = GetComponentInParent<UIInputController>();
        
        UpdateButtonState(false);
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        
        // 检查特殊攻击是否可用
        bool canUseSpecialAttack = GameManager.Instance.IsSpecialAttackAvailable(photonView.Owner.ActorNumber);
        if (canUseSpecialAttack != isAvailable)
        {
            UpdateButtonState(canUseSpecialAttack);
        }
    }

    public void OnSpecialAttackButtonClicked()
    {
        if (!photonView.IsMine || !isAvailable) return;
        
        // 通知UIInputController触发特殊攻击
        if (inputController != null)
        {
            inputController.TriggerSpecialAttack();
            UpdateButtonState(false);
        }
    }

    private void UpdateButtonState(bool available)
    {
        isAvailable = available;
        if (button != null)
        {
            button.interactable = isAvailable;
        }
        if (buttonImage != null)
        {
            buttonImage.color = isAvailable ? availableColor : unavailableColor;
        }
    }
}
