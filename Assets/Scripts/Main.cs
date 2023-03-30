using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

public sealed class Main : MonoBehaviour
{
    [SerializeField ]private string _serverUrl = "http://localhost:3000";

    [Space]
    [SerializeField] private float _externalLoginWaitInSeconds = 2;

    [Space]
    [SerializeField] private PlatformSignInView _platformsSignInView;
    [SerializeField] private UserProfileView _userProfileView;

    private string _session;
    private TokenPairData _tokenData;
    private UserSchema _userData;

    private async void Start()
    {
        _platformsSignInView.gameObject.SetActive(false);
        _userProfileView.gameObject.SetActive(false);

        if (await TryLoginUsingLocalToken())
        {
            _userProfileView.gameObject.SetActive(true);
            _userProfileView.UpdateVisual(_userData);
        }
        else
        {
            _session = CreateSessionId();

            var response = await GetAllPlatformsRequestAsync();

            if (!response.Ok)
            {
                Debug.LogError("failed to fetch platforms");
                return;
            }

            var platforms = response.Get<PlatformSignInData[]>();
            _platformsSignInView.gameObject.SetActive(true);
            _platformsSignInView.UpdateVisual(platforms);

            while(true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_externalLoginWaitInSeconds), ignoreTimeScale: true);

                Debug.Log("waiting for user");

                var result = await MakeExternalRequestAsync();

                if (result)
                {
                    break;
                }
            }

            _platformsSignInView.gameObject.SetActive(false);

            if (await TryLoginUsingLocalToken())
            {
                _userProfileView.gameObject.SetActive(true);
                _userProfileView.UpdateVisual(_userData);
            }
            else
            {
                Debug.LogError("failed to TryLoginUsingLocalToken");
            }
        }
    }

    private void OnEnable()
    {
        _userProfileView.OnSignOutButtonClicked += UserProfileView_OnSignOutButtonClicked;
    }

    private void OnDisable()
    {
        _userProfileView.OnSignOutButtonClicked -= UserProfileView_OnSignOutButtonClicked;
    }

    private async UniTask<bool> TryLoginUsingLocalToken()
    {
        var path = TokenLocalPath();

        if (!File.Exists(path))
        {
            return false;
        }

        var json = File.ReadAllText(path);
        var tokenData = JsonConvert.DeserializeObject<TokenPairData>(json);

        var userData = await FetchProfileAsync(tokenData.AccessToken);
        
        if (userData != null)
        {
            _userData = userData;
            return true;
        }

        return false;
    }

    private async UniTask<Response> GetAllPlatformsRequestAsync()
    {
        var url = $"{_serverUrl}/login/all?session={_session}";
        var request = await MakeGetRequestAsync(url);
        var response = new Response(request);
        return response;
    }

    private async UniTask<bool> MakeExternalRequestAsync()
    {
        var url = $"{_serverUrl}/login/external?session={_session}";
        var request = await MakeGetRequestAsync(url);
        var response = new Response(request);

        if (response.Ok)
        {
            _tokenData = response.Get<TokenPairData>();
            SaveTokenPair(_tokenData);
            return true;
        }

        return false;
    }

    private async UniTask<UnityWebRequest> MakeGetRequestAsync(string url, Dictionary<string, string> headers = null)
    {
        var request = UnityWebRequest.Get(url);

        if (headers != null)
        {
            foreach (var pair in headers)
            {
                var header = pair.Key;
                var value = pair.Value;
                request.SetRequestHeader(header, value);
            }
        }

        try
        {
            await request.SendWebRequest();
        }
        catch
        {
            // this block of code is executed when request has error
            // i dont want todo do anything in this case
        }

        return request;
    }

    private static string CreateSessionId()
    {
        return Guid.NewGuid().ToString().Split("-")[0];
    }

    private string TokenLocalPath()
    {
        return Path.Join(Application.persistentDataPath, "token.json");
    }

    private void SaveTokenPair(TokenPairData data)
    {
        var settings = new JsonSerializerSettings();
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.Formatting = Formatting.Indented;

        var path = TokenLocalPath();
        var json = JsonConvert.SerializeObject(data, settings);
        File.WriteAllText(path, json);
    }

    private async UniTask<UserSchema> FetchProfileAsync(string accessToken)
    {
        var url = $"{_serverUrl}/api/profile/me";

        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {accessToken}" }
        };

        var request = await MakeGetRequestAsync(url, headers);

        var response = new Response(request);
        
        if (response.Ok)
        {
            var data = response.Get<UserSchema>();
            return data;
        }

        return null;
    }

    private void SignOut()
    {
        var path = TokenLocalPath();
        
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        _session = "";
        _tokenData = null;
        _userData = null;

        Start();
    }

    private void UserProfileView_OnSignOutButtonClicked()
    {
        SignOut();
    }
}
