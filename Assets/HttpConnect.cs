using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ScreenshotUtility;
using Newtonsoft.Json.Linq;
using System.Text;

public class HttpConnect : MonoBehaviour
{
    private ScreenShot ss;
    private SendPNGFile spf;


    //???N?G?X?g??M????url
    //    private string url = "https://example.com/";
    private string url = "http://172.21.39.32:8332/api/v0.5/ping";
    private string position;

    // MEC-RM
    private Client client;
    private string workerId;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        // client init
        this.client = new Client(this.url);

        // register worker
        var awaitWorker = client.WorkerRegister("vr");
        yield return StartCoroutine(awaitWorker);
        this.workerId = awaitWorker.Current.ToString();

        //ScreenShot,SendPNGFile??X?N???v?g???擾
        client = GetComponent<Client>();
        ss = GetComponent<ScreenShot>();
        spf = GetComponent<SendPNGFile>();
        //Coroutine???J?n
        StartCoroutine(ExecuteEveryFiveSeconds());
    }


    IEnumerator GetRequest(string url)
    {
        //MEC????J???????u?????擾????
        // using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        // {
        //     //???N?G?X?g??M??????????@
        //     yield return webRequest.SendWebRequest();

        //     //?G???[??m?F
        //     if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        //     {
        //         //?G???[???b?Z?[?W??o??
        //         Debug.LogError(webRequest.error);
        //     }
        //     else
        //     {
        //         //????????S??o??
        //         Debug.Log(webRequest.downloadHandler.text);
        //         //position??i?[
        //         position = webRequest.downloadHandler.text;

        //         // JSON?f?[?^??CameraData?I?u?W?F?N?g??f?V???A???C?Y
        //         CameraData cameraData = JsonUtility.FromJson<CameraData>(position);

        //         // ?J???????u????????X?V
        //         transform.position = cameraData.position;
        //         transform.rotation = Quaternion.Euler(cameraData.rotation);

        //         Debug.Log("Camera data successfully updated!");

        var workerJob = client.WorkerContract(workerId);
        yield return workerJob;
        var jobId = workerJob.Current.ToString();

        if (jobId != null)
        {
            //SendPNGFileにjobIdを送る
            spf.jobId = jobId;

            var jobInfo = client.JobInfo(jobId, null);
            yield return jobInfo;
            var inputId = (jobInfo.Current as JObject)["input"]["id"].ToString();

            var input = client.Download(inputId);
            yield return input;
            //カメラ位置情報
            var position = workerJob.Current.ToString();


            // JSON?f?[?^??CameraData?I?u?W?F?N?g??f?V???A???C?Y
            CameraData cameraData = JsonUtility.FromJson<CameraData>(position);

            // ?J???????u????????X?V
            transform.position = cameraData.position;
            transform.rotation = Quaternion.Euler(cameraData.rotation);
            //スクショ撮ってローカル保存
            //ss.getScreenShots();

            Debug.Log("Camera data successfully updated!");
        }
        else {
            Debug.Log("no job");
        }
    }

    IEnumerator ExecuteEveryFiveSeconds()
    {
        //5秒おきに動く
        while (true)
        {
            //位置情報貰う カメラ動かす 画像撮る
            yield return GetRequest(url);
            //PNG???M
            StartCoroutine(spf.SendPNGCoroutine(ss.path, ss.filename));
            Debug.Log(ss.path + ss.name);

            // 5?b??@
            yield return new WaitForSeconds(5f);
        }
    }

    [System.Serializable]
    public class CameraData
    {
        public Vector3 position;
        public Vector3 rotation;
    }
}

/*
public class Client
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
        yield return request.downloadHandler.text;
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
        Debug.Log(JObject.Parse(request.downloadHandler.text)["id"]);
        yield return JObject.Parse(request.downloadHandler.text)["id"];
    }

    public IEnumerator Download(string dataId)
    {
        var endpoint = $"/data/{dataId}/blob";
        var request = UnityWebRequest.Get(host + endpoint);
        yield return request.SendWebRequest();

        Debug.Log("Downloaded" + request.downloadHandler.data.Length);
        yield return request.downloadHandler.data;
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
        yield return JObject.Parse(request.downloadHandler.text)["id"];
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
        yield return JObject.Parse(request.downloadHandler.text)["id"];
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
        yield return JObject.Parse(request.downloadHandler.text);
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
        yield return JObject.Parse(request.downloadHandler.text);
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
        yield return JObject.Parse(request.downloadHandler.text)["id"];
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
        yield return JObject.Parse(request.downloadHandler.text)["job"];
    }
}
*/