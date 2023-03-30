using UnityEngine;

public sealed class PlatformSignInView : MonoBehaviour
{
    [SerializeField] private PlatformSignInButtonView _btnPrefab;
    [SerializeField] private Transform _parent;

    public void UpdateVisual(PlatformSignInData[] platforms)
    {
        _parent.DestroyChildrens();

        for (int i = 0; i < platforms.Length; i++)
        {
            var platform = platforms[i];

            var btn = Instantiate(_btnPrefab, _parent);
            btn.UpdateVisual(platform.Name, platform.AuthorizationUri);
        }
    }
}
