using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;


public class Client : MonoBehaviour
{
    public string host;

    public Client(string host)
    {
        this.host = host;
    }

    public IEnumerator Ping()
    {
        var endpoint = "/ping";
        var request = UnityWebRequest.Get(host + endpoint);
        yield return request.SendWebRequest();

        Debug.Log("Pinged" + request.downloadHandler.text);

        var response = request.downloadHandler.text; // maybe
        request.Dispose();
        yield return response;
    }

    public IEnumerator Upload(byte[] data)
    {
        List<IMultipartFormSection> formData = new()
        {
            new MultipartFormFileSection("file", data, "", "object/octet-stream")
        };

        var endpoint = "/data";
        var request = UnityWebRequest.Post(host + endpoint, formData);
        yield return request.SendWebRequest();

        Debug.Log("Uploaded" + request.downloadHandler.text);

        var dataId = JObject.Parse(request.downloadHandler.text)["id"].ToString();
        request.Dispose();
        yield return dataId;
    }

    public IEnumerator Download(string dataId)
    {
        var endpoint = $"/data/{dataId}/blob";
        var request = UnityWebRequest.Get(host + endpoint);
        yield return request.SendWebRequest();

        Debug.Log("Downloaded" + request.downloadHandler.data.Length);

        var data = request.downloadHandler.data; // maybe
        request.Dispose();
        yield return data;
    }

    public IEnumerator CreateLambda(string runtime, string dataId)
    {
        var json = $"{{\"runtime\": \"{runtime}\", \"codex\": \"{dataId}\"}}";

        var endpoint = "/lambda";
        var request = new UnityWebRequest(host + endpoint, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        yield return request.SendWebRequest();

        Debug.Log("Lambda created" + request.downloadHandler.text);

        var lambdaId = JObject.Parse(request.downloadHandler.text)["id"].ToString();
        request.Dispose();
        yield return lambdaId;
    }

    public IEnumerator CreateJob(string lambdaId, string dataId)
    {
        var json = $"{{\"lambda\": \"{lambdaId}\", \"input\": \"{dataId}\", \"tags\": []}}";

        var endpoint = "/job";
        var request = new UnityWebRequest(host + endpoint, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        yield return request.SendWebRequest();

        Debug.Log("Job created" + request.downloadHandler.text);

        var jobId = JObject.Parse(request.downloadHandler.text)["id"].ToString();
        request.Dispose();
        yield return jobId;
    }

    public IEnumerator JobInfo(string jobId, string except)
    {
        string endpoint;
        if (except == null)
        {
            endpoint = $"/job/{jobId}";
        }
        else
        {
            endpoint = $"/job/{jobId}?except=Finished&timeout=20";
        }

        var request = UnityWebRequest.Get(host + endpoint);
        yield return request.SendWebRequest();

        Debug.Log("Job info" + request.downloadHandler.text);

        var response = JObject.Parse(request.downloadHandler.text);
        request.Dispose();
        yield return response;
    }

    public IEnumerator JobUpdate(string jobId, string dataId)
    {
        var json = $"{{\"output\": \"{dataId}\", \"status\": \"finished\"}}";

        var endpoint = $"/job/{jobId}";
        var request = new UnityWebRequest(host + endpoint, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        yield return request.SendWebRequest();

        Debug.Log("Job update" + request.downloadHandler.text);
        
        var response = JObject.Parse(request.downloadHandler.text);
        request.Dispose();
        yield return response;
    }

    public IEnumerator WorkerRegister(string runtime)
    {
        var json = $"{{\"runtime\": [\"{runtime}\"]}}";

        var endpoint = "/worker";
        var request = new UnityWebRequest(host + endpoint, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        yield return request.SendWebRequest();

        Debug.Log("Worker registered" + request.downloadHandler.text);

        var workerId = JObject.Parse(request.downloadHandler.text)["id"].ToString();
        request.Dispose();
        yield return workerId;
    }

    public IEnumerator WorkerContract(string workerId)
    {
        var json = $"{{\"worker\": \"{workerId}\", \"tags\": [], \"timeout\": 10}}";

        var endpoint = $"/worker/{workerId}/contract";
        var request = new UnityWebRequest(host + endpoint, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        yield return request.SendWebRequest();

        Debug.Log("Worker contracted" + request.downloadHandler.text);

        var jobId = JObject.Parse(request.downloadHandler.text)["job"].ToString();
        request.Dispose();      
        yield return jobId;
    }
}
