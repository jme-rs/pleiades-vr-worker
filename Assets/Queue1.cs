using System.Collections;
// using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;


public class Queue1 : MonoBehaviour
{
    //Queue�̐錾
    //Queue<InputItem> IQ;
    // BlockingCollection<InputItem> IQ;
    private ConcurrentQueue<InputItem> IQ;

    const int MAX_QUEUE_SIZE = 10;
    const int MAX_QUEUE_WAIT = 1000;

    // Start is called before the first frame update
    void Start()
    {
        //������
        // IQ = new BlockingCollection<InputItem>(MAX_QUEUE_SIZE);
        IQ = new ConcurrentQueue<InputItem>();
    }

    // Update is called once per frame
    // public void enqueue(InputItem data)
    public void Enqueue(InputItem data)
    {
        // IQ.Enqueue(data);
        // return IQ.TryAdd(data, MAX_QUEUE_WAIT);
        IQ.Enqueue(data);
    }

    // public InputItem dequeue()
    public InputItem Dequeue()
    {
        // return IQ.TryTake(out InputItem item, MAX_QUEUE_WAIT) ? item : null;
        return IQ.TryDequeue(out InputItem item) ? item : null;
    }

    // public bool Check()
    // {
    //     if (IQ.Count > 0)
    //     {
    //         return true;
    //     }
    //     else
    //     {
    //         return false;
    //     }
    // }
}

public class InputItem
{
    public string jobId;
    public string input;

    public InputItem(string jobId, string input)
    {
        this.jobId = jobId;
        this.input = input;
    }
}
