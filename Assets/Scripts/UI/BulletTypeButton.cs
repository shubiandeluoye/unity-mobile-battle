using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class BulletTypeButton : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color smallBulletColor = Color.white;
    [SerializeField] private Color mediumBulletColor = Color.yellow;
    
    private BulletType currentType = BulletType.Small;
    private UIInputController inputController;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        inputController = GetComponentInParent<UIInputController>();
        
        UpdateButtonVisual();
    }

    public void ToggleBulletType()
    {
        if (!photonView.IsMine) return;
        
        currentType = currentType == BulletType.Small ? BulletType.Medium : BulletType.Small;
        UpdateButtonVisual();
        
        // 通知UIInputController子弹类型改变
        if (inputController != null)
        {
            inputController.SetBulletType(currentType);
        }
    }

    private void UpdateButtonVisual()
    {
        if (buttonImage != null)
        {
            buttonImage.color = currentType == BulletType.Small ? smallBulletColor : mediumBulletColor;
        }
    }
}
