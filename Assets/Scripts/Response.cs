using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public sealed class Response
{
    private readonly UnityWebRequest _request;
    
    public readonly bool Ok;

    public Response(UnityWebRequest request)
    {
        _request = request;
        Ok = _request.result == UnityWebRequest.Result.Success;
    }

    public T Get<T>()
    {
        var settings = new JsonSerializerSettings();
        settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

        var json = _request.downloadHandler.text;
        var data = JsonConvert.DeserializeObject<T>(json, settings);
        return data;
    }
}
