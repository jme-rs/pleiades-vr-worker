using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class Finisher : MonoBehaviour
{
    private Client client;
    private Queue2 outputQ;

    const int MAX_FINISH = 16;

    void Start()
    {
        // this.client = new Client("http://pleiades.local:8332/api/v0.5");
        this.client = new("http://192.168.168.127:8332/api/v0.5");
        this.outputQ = GetComponent<Queue2>();

        for (int i = 0; i < MAX_FINISH; i++)
        {
            StartCoroutine(Finish());
        }
    }

    IEnumerator Finish()
    {
        while (true)
        {
            // if (!this.outputQ.Check())
            //     continue;

            var output = this.outputQ.Dequeue();

            if (output == null)
            {
                // Debug.Log("Q2 is empty");
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            Debug.Log("Q2 dequeued");

            // upload
            var upload = client.Upload(output.filedata);
            yield return StartCoroutine(upload);
            var outputId = upload.Current.ToString();

            Debug.Log(outputId);
            
            // update job status
            var jobUpdate = client.JobUpdate(output.jobId, outputId);
            yield return StartCoroutine(jobUpdate);
            Debug.Log("job finished");
        }
    }
}
