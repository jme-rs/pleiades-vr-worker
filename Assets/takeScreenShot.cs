using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ScreenshotUtility;
using Newtonsoft.Json.Linq;
using System.Text;

public class takeScreenShot : MonoBehaviour
{
    private Queue1 Q1;
    private Queue2 Q2;
    private ScreenShot ss;

    string jobId;
    string input;

    public InputItem IQ;
    public OutputItem OQ;

    [HideInInspector]
    public byte[] filedata;

    // Start is called before the first frame update
    void Start()
    {
        Q1 = GetComponent<Queue1>();
        Q2 = GetComponent<Queue2>();
        ss = GetComponent<ScreenShot>();
    }

    // Update is called once per frame
    void Update()
    {
        var IQ = Q1.Dequeue();
        if(IQ != null)
        {
            //�L���[����id�ƈʒu�������炤
            jobId = IQ.jobId;
            input = IQ.input;

            Vector3 position;
            Quaternion rotation;

            //string�̃f�V���A���C�Y�A�J�����ړ�
            //CameraData cameraData = JsonUtility.FromJson<CameraData>(input);
            ParseJsonToTransform(input, out position, out rotation);
            transform.position = position;
            transform.rotation = rotation;

            //�X�N�V���B��
            ss.getScreenShots(jobId);
            
            //�L���[�ɒǉ�
            //Q2.Enqueue(new OutputItem (jobId,filedata));
        }
    }
    void ParseJsonToTransform(string jsonString, out Vector3 position, out Quaternion rotation)
    {
        // JSON��������p�[�X
        JObject jsonObject = JObject.Parse(jsonString);

        // position�̃p�[�X
        JObject posObject = (JObject)jsonObject["position"];
        float posX = posObject["x"].Value<float>();
        float posY = posObject["y"].Value<float>();
        float posZ = posObject["z"].Value<float>();
        position = new Vector3(posX, posY, posZ);

        // rotation�̃p�[�X
        JObject rotObject = (JObject)jsonObject["rotation"];
        float rotX = rotObject["x"].Value<float>();
        float rotY = rotObject["y"].Value<float>();
        float rotZ = rotObject["z"].Value<float>();
        rotation = Quaternion.Euler(rotX, rotY, rotZ);
    }
}
public class CameraData
{
    public Vector3 position;
    public Vector3 rotation;
}
