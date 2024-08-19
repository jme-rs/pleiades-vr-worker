using System.Collections;
// using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public class Queue2 : MonoBehaviour
{
    // private Queue<OutputItem> OQ;
    // private BlockingCollection<OutputItem> OQ;
    private ConcurrentQueue<OutputItem> OQ;

    const int MAX_QUEUE_SIZE = 10;
    const int MAX_QUEUE_WAIT = 1000;

    // Start is called before the first frame update
    void Start()
    {
        //�L���[�̏�����
        // OQ = new Queue<OutputItem>();
        // OQ = new BlockingCollection<OutputItem>(MAX_QUEUE_SIZE);
        if(OQ == null)OQ = new ConcurrentQueue<OutputItem>();
    }

    // Update is called once per frame
    public void Enqueue(OutputItem data)
    {
        // OQ.Enqueue(data);
        // OQ.TryAdd(data, MAX_QUEUE_WAIT);
        OQ.Enqueue(data);
    }

    public OutputItem Dequeue()
    {
        if (OQ == null) OQ = new ConcurrentQueue<OutputItem>();
        // return OQ.TryTake(out OutputItem item, MAX_QUEUE_WAIT) ? item : null;
        return OQ.TryDequeue(out OutputItem item) ? item : null;
    }

    // public bool Check()
    // {
    //     return OQ.Count > 0;
    // }
}

public class OutputItem
{
    public string jobId;
    public byte[] filedata;

    public OutputItem(string jobId, byte[] filedata)
    {
        this.jobId = jobId;
        this.filedata = filedata;
    }
}
