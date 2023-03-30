using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlatformSignInData
{
    public string Name;
    public string AuthorizationUri;
}

public sealed class TokenPairData
{
    public string AccessToken;
    public string RefreshToken;
}

public sealed class UserSchema
{
    public AppFields App;
}

public sealed class AppFields
{
    public string Avatar;
    public NicknameFields Nickname;
}

public sealed class NicknameFields
{
    public string Value;
}
