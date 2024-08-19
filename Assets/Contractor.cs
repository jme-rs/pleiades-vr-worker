using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Text;

public class Contractor : MonoBehaviour
{
    private Client client;
    private string workerId;
    private Queue1 inputQ;
    const int MAX_CONTRACT = 16;

    IEnumerator Start()
    {
        Unity.Collections.NativeLeakDetection.Mode = Unity.Collections.NativeLeakDetectionMode.EnabledWithStackTrace;

        // this.client = new Client("http://pleiades.local:8332/api/v0.5");
        this.client = new("http://192.168.168.127:8332/api/v0.5");
        this.inputQ = GetComponent<Queue1>();

        // register
        var register = client.WorkerRegister("vr");
        yield return StartCoroutine(register);
        var workerId = register.Current.ToString();

        if (workerId.Length != 0)
            this.workerId = workerId;
        else
            Debug.Log("failed to register worker");

        // start
        for (int i = 0; i < MAX_CONTRACT; i++)
        {

            StartCoroutine(Contract());
        }
    }

    IEnumerator Contract()
    {
        while (true)
        {
            // contract
            var contract = client.WorkerContract(this.workerId);
            yield return StartCoroutine(contract);
            var jobId = contract.Current.ToString();

            if (jobId == "")
            {
                Debug.Log("no job");
                continue;
            }

            // job info
            var jobInfo = client.JobInfo(jobId, null);
            yield return StartCoroutine(jobInfo);
            var inputId = (jobInfo.Current as JObject)["input"]["id"].ToString();


            // download
            var download = client.Download(inputId);
            yield return StartCoroutine(download);
            var pos = Encoding.UTF8.GetString(download.Current as byte[]);

            Debug.Log(pos);

            // enqueue
            this.inputQ.Enqueue(new(jobId, pos));
            Debug.Log("job enqueued");
        }
    }
}
