using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

public sealed class PlatformSignInButtonView : MonoBehaviour
{
    [SerializeField] private Button _btn;
    [SerializeField] private Text _txtPlatformName;

    private string _url;

    private void OnEnable()
    {
        _btn.onClick.AddListener(ClickHandler);
    }

    private void OnDisable()
    {
        _btn.onClick.RemoveListener(ClickHandler);
    }

    private void ClickHandler()
    {
        Application.OpenURL(_url);
    }

    public void UpdateVisual(string platformName, string url)
    {
        _txtPlatformName.text = platformName;
        _url = url;
    }
}
