using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using Newtonsoft.Json.Linq;


public class SendPNGFile : MonoBehaviour
{
    // �T�[�o�[URL
    private string serverUrl = "http://172.21.39.32:8332/api/v0.5/ping";

    // PNG�t�@�C�����ۑ�����Ă��郍�[�J���p�X
    private string localPath = "";

    // ���M�������t�@�C���̖��O
    private string fileName = "";

    private Client client;

    public string jobId;
    private string previd;

    void Start()
    {
        client = GetComponent<Client>();
        this.client = new Client(serverUrl);
        // �t�@�C�����������A���M����R���[�`�����J�n
        StartCoroutine(SendPNGCoroutine(localPath, fileName));
    }

    public IEnumerator SendPNGCoroutine(string localPath, string fileName)
    {
        if (jobId != previd)
        {
            previd = jobId;
            // �t���p�X���擾
            string filePath = Path.Combine(localPath, fileName);

            // �t�@�C�������݂��邩�m�F
            if (File.Exists(filePath))
            {
                // �t�@�C�����o�C�g�z��ɓǂݍ���
                byte[] fileData = File.ReadAllBytes(filePath);

                // �t�H�[���f�[�^�̍쐬
                // WWWForm form = new WWWForm();
                // form.AddBinaryData("file", fileData, fileName, "image/png");

                // // HTTP���N�G�X�g�̍쐬
                // UnityWebRequest www = UnityWebRequest.Post(serverUrl, form);

                // // ���N�G�X�g�𑗐M���A���X�|���X��҂�
                // yield return www.SendWebRequest();

                // // �G���[�`�F�b�N
                // if (www.result != UnityWebRequest.Result.Success)
                // {
                //     Debug.LogError("Error: " + www.error);
                // }
                // else
                // {
                //     Debug.Log("File successfully uploaded!");
                // }

                var output = client.Upload(fileData);
                yield return output;
                var outputId = output.Current.ToString();

                var update = client.JobUpdate(jobId, outputId);
                yield return update;
            }
            else
            {
                Debug.LogError("File not found: " + filePath);
            }
        }
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

        var jobId = JObject.Parse(request.downloadHandler.text)["job"].ToString();
        yield return jobId;
    }
}
*/