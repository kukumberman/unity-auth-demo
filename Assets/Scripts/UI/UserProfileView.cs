using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Text = TMPro.TextMeshProUGUI;

public sealed class UserProfileView : MonoBehaviour
{
    public event Action OnSignOutButtonClicked;

    [SerializeField] private Text _txtNickname;
    [SerializeField] private Image _imgAvatar;
    [SerializeField] private Button _btnSignOut;

    private void OnEnable()
    {
        _btnSignOut.onClick.AddListener(ButtonSignOutClickHandler);
    }

    private void OnDisable()
    {
        _btnSignOut.onClick.RemoveListener(ButtonSignOutClickHandler);
    }

    public void UpdateVisual(UserSchema data)
    {
        var avatarUrl = data.App.Avatar;
        var nickname = data.App.Nickname.Value;

        _txtNickname.text = nickname;

        UpdateAvatar(avatarUrl);
    }

    private async void UpdateAvatar(string url)
    {
        var request = UnityWebRequestTexture.GetTexture(url);
        await request.SendWebRequest();

        var texture = DownloadHandlerTexture.GetContent(request);
        var rect = new Rect(0, 0, texture.width, texture.height);
        var pivot = new Vector2(0.5f, 0.5f);
        var sprite = Sprite.Create(texture, rect, pivot);

        _imgAvatar.sprite = sprite;
    }

    private void ButtonSignOutClickHandler()
    {
        OnSignOutButtonClicked?.Invoke();
    }
}
