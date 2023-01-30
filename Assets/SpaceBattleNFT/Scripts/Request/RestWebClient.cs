using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RestWebClient : Singleton<RestWebClient>
{
    private const string defaultContentType = "application/json";
    [SerializeField] private int _timeout = 3000;
    
    public bool IsRequestInProcess;
    public float CurrentTime;
    
    public void Update()
    {
        if (IsRequestInProcess)
        {
            CurrentTime += Time.deltaTime;
        }
    }

    public IEnumerator HttpGet(string url, System.Action<Response> callback, 
        IEnumerable<RequestHeader> headers = null)
    {
        using var webRequest = UnityWebRequest.Get(url);
        
        IsRequestInProcess = true;
        webRequest.timeout = _timeout;
        
        if (headers != null)
        {
            foreach (var header in headers)
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }
        }
            
        yield return webRequest.SendWebRequest();
        
        long responseCode;
        string error;
        
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
            webRequest.result == UnityWebRequest.Result.ProtocolError || 
            webRequest.result == UnityWebRequest.Result.DataProcessingError)
        {
            responseCode = webRequest.responseCode;
            error = webRequest.error;
            
            
            callback(new Response
            {
                StatusCode = responseCode,
                Error = error,
            });
        }

        if (!webRequest.isDone)
        {
            yield break;
        }
        
        IsRequestInProcess = false;
        CurrentTime = 0.0f;
        
        var data = webRequest.downloadHandler.data == null ? string.Empty : System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
        //var data = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
        //Debug.Log("Data: " + data);
        
        responseCode = webRequest.responseCode;
        error = webRequest.error;
        
            
        callback(new Response
        {
            StatusCode = responseCode,
            Error = error,
            Data = data
        });
    }
    
    public IEnumerator HttpDelete(string url, Action<Response> callback, IEnumerable<RequestHeader> headers = null)
    {
        using var webRequest = UnityWebRequest.Delete(url);
        
        IsRequestInProcess = true;
        webRequest.timeout = _timeout;
        
        if (headers != null)
        {
            foreach (var header in headers)
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }
        }
            
        yield return webRequest.SendWebRequest();
        
        long responseCode;
        string error;
        
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
            webRequest.result == UnityWebRequest.Result.ProtocolError || 
            webRequest.result == UnityWebRequest.Result.DataProcessingError)
        {
            responseCode = webRequest.responseCode;
            error = webRequest.error;
            
            
            callback(new Response
            {
                StatusCode = responseCode,
                Error = error
            });
        }
            
        if (!webRequest.isDone)
        {
            yield break;
        }
        
        IsRequestInProcess = false;
        CurrentTime = 0.0f;
        
        responseCode = webRequest.responseCode;
        error = webRequest.error;
        
        callback(new Response
        {
            StatusCode = responseCode,
            Error = error
        });
    }

    public IEnumerator HttpPost(string url, string body, Action<Response> callback,
        IEnumerable<RequestHeader> headers = null)
    {
        using var webRequest = UnityWebRequest.Post(url, body);
        IsRequestInProcess = true;
        webRequest.timeout = _timeout;
        if (headers != null)
        {
            foreach (var header in headers)
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }
        }

        webRequest.uploadHandler.contentType = defaultContentType;
        webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));

        yield return webRequest.SendWebRequest();
        
        long responseCode;
        string error;
        
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
            webRequest.result == UnityWebRequest.Result.ProtocolError || 
            webRequest.result == UnityWebRequest.Result.DataProcessingError)
        {
            responseCode = webRequest.responseCode;
            error = webRequest.error;
            
            
            callback(new Response
            {
                StatusCode = responseCode,
                Error = error
            });
        }

        if (!webRequest.isDone)
        {
            yield break;
        }
        
        IsRequestInProcess = false;
        CurrentTime = 0.0f;
        
        var data = webRequest.downloadHandler.data == null ? string.Empty : System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
        
        responseCode = webRequest.responseCode;
        error = webRequest.error;
        
        
        callback(new Response
        {
            StatusCode = responseCode,
            Error = error,
            Data = data
        });
        
    }

    public IEnumerator HttpPut(string url, string body, System.Action<Response> callback,
        IEnumerable<RequestHeader> headers = null)
    {
        using var webRequest = UnityWebRequest.Put(url, body);
        
        IsRequestInProcess = true;
        webRequest.timeout = _timeout;
        
        if (headers != null)
        {
            foreach (var header in headers)
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }
        }

        webRequest.uploadHandler.contentType = defaultContentType;
        webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));

        yield return webRequest.SendWebRequest();
        
        long responseCode;
        string error;
        
        if (webRequest.result != UnityWebRequest.Result.Success || 
            webRequest.result != UnityWebRequest.Result.InProgress)
        {
            responseCode = webRequest.responseCode;
            error = webRequest.error;
            
            callback(new Response
            {
                StatusCode = responseCode,
                Error = error,
            });
        }

        if (!webRequest.isDone)
        {
            yield break;
        }
        
        IsRequestInProcess = false;
        CurrentTime = 0.0f;
        
        responseCode = webRequest.responseCode;
        
        callback(new Response
        {
            StatusCode = responseCode,
        });
    }
}