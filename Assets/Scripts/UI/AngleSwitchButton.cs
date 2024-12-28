using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class AngleSwitchButton : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color angle45Color = Color.blue;
    [SerializeField] private Color angle30Color = Color.green;
    
    private bool is45Degree = true;
    private UIInputController inputController;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        inputController = GetComponentInParent<UIInputController>();
        
        UpdateButtonVisual();
    }

    public void ToggleAngle()
    {
        if (!photonView.IsMine) return;
        
        is45Degree = !is45Degree;
        UpdateButtonVisual();
        
        // 通知UIInputController角度改变
        if (inputController != null)
        {
            inputController.SetShootingAngle(is45Degree);
        }
    }

    private void UpdateButtonVisual()
    {
        if (buttonImage != null)
        {
            buttonImage.color = is45Degree ? angle45Color : angle30Color;
        }
    }
}
