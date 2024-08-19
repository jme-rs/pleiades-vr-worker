using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace ScreenshotUtility
{
    //�^�C���X�^���v�̏����ݒ�
    public enum TIME_STAMP
    {
        MMDDHHMMSS,
        YYYYMMDDHHMMSS,
    }

    //�w�i�F�̐ݒ�
    public enum BACK_GROUND_COLOR
    {
        Alpha, //����PNG
        CustomColor, //�J�X�^���J���[
        Skybox, //�X�J�C�{�b�N�X
    }

    //�𑜓x�̐ݒ�
    public enum SCREEN_SIZE_PIXEL
    {
        //1:1
        p256x256,
        p512x512,
        p1024x1024,
        p2048x2048,
        p4096x4096,

        //16:9
        p1280x720, //HD
        p1920x1080, //FullHD
        p2560x1440, //2k
        p3840x2160, //4k
        CustomSize //�w�肵���s�N�Z���T�C�Y�ŏo��(����32/���4096)
    }

    public class ScreenShot : MonoBehaviour
    {
        private Queue2 Q2;
        private takeScreenShot tss;

        [HideInInspector] public string filename = "";
        [HideInInspector] public string path = "";

        //UnityEditor�݂̂Ŏ��s
#if UNITY_EDITOR
        [Header("�B�e�Ɏg���J�����̊��蓖��")]
        [Tooltip("�B�e�Ɏg���J���������蓖�ĂĂ�������"), SerializeField]
        Camera _UseCamera;

        [Header("�摜�̃T�C�Y�̐ݒ�")]
        [Space(10)]
        [Tooltip("ScreenSizePixel��Custom�ɂ����CustomSize�̉𑜓x���K�p����܂�"), SerializeField]
        SCREEN_SIZE_PIXEL _screenSizePixel = SCREEN_SIZE_PIXEL.p1024x1024;

        [Tooltip("ScreenSizePixel��Custom�ɂȂ��Ă�ꍇ�̉𑜓x�ݒ�i����32Pixel/���4096Pixel�j"), SerializeField]
        Vector2Int _customSize = new Vector2Int(1024, 1024);

        [Header("�w�i�F�̐ݒ�")]
        [Space(10)]
        [Tooltip("Alpha:�w�i���� White:�� CustomColor:�w�肵���F Skybox:�X�J�C�{�b�N�X"), SerializeField]
        BACK_GROUND_COLOR _buckGroundColorType = BACK_GROUND_COLOR.Alpha;

        [Tooltip("BuckGroundColorType��Custom�̏ꍇ�̔w�i�F"), SerializeField]
        UnityEngine.Color _customColor = UnityEngine.Color.green;

        [Header("�t�@�C�����̏����ݒ�")]
        [Space(10)]
        [Tooltip("�^�C���X�^���v�̏����ݒ�"), SerializeField]
        TIME_STAMP _timeStampStyle = TIME_STAMP.MMDDHHMMSS;
        [Tooltip("�t�@�C�����̐擪�ɕt��������"), SerializeField]
        string _screenShotsTitle = "img";

        [Header("�ۑ���̃t�H���_��")]
        [Space(10)]
        [Tooltip("�X�N���[���V���b�g��ۑ�����t�H���_�� Assets�t�H���_�[�̒����ɔz�u����܂�"), SerializeField]
        string _screenShotFolderName = "ScreenShots";

        [Header("���s���̎B�e�L�[")]
        [Space(10)]
        [Tooltip("Unity���s���ɃX�N���[���V���b�g���B��ꍇ�̃L�[�o�C���h")]
        public KeyCode _screenShotsKeybinding = KeyCode.F1;

        [Header("Console�ɕۑ���̃p�X�̃��O���o�͂��܂�")]
        [Space(10)]
        [Tooltip("�`�F�b�N������ƁA�摜�̃t�@�C�����Ɖ摜�̕ۑ���̃p�X�̃��O�̏o�͂��܂�"), SerializeField]
        bool _consoleLogIsActive = true;


        private void Start()
        {
            tss = GetComponent<takeScreenShot>();
            Q2 = GetComponent<Queue2>();
        }
        void Update()
        {
            if (Input.GetKeyDown(_screenShotsKeybinding))
            {
                //getScreenShots();
            }
        }

        private bool NullCheck()
        {
            bool isNull = false;
            if (_UseCamera == null)
            {
                Debug.LogWarning("�摜�̏����o���Ɏg�p����J������ ScreenSchotCamera �Ɋ��蓖�ĂĂ�������");
                isNull = true;
            }
            if (_screenShotsTitle == "")
            {
                Debug.LogWarning("ScreenShotsTitle �ɉ摜�t�@�C���ɕt���閖���̕�������͂��Ă�������");
                isNull = true;
            }
            if (_screenShotFolderName == "")
            {
                Debug.LogWarning("ScreenShotFolderName ��ScreenShot�̕ۑ���̃t�H���_������͂��Ă�������");
                isNull = true;
            }
            return isNull;
        }

        [ContextMenu("�X�N���[���V���b�g���B�e����")]
        public void getScreenShots(string jobId)
        {

            //NullCheck
            if (NullCheck()) { return; }
            // Application.dataPath = ../Assets
            path = UnityEngine.Application.dataPath + "/" + _screenShotFolderName + "/";
            StartCoroutine(imageShooting(path, _screenShotsTitle,jobId));
        }

        //�B�e����
        //������ �t�@�C���p�X / ������ �^�C�g��
        private IEnumerator imageShooting(string path, string title, string jobId)
        {
            yield return new WaitForEndOfFrame();

            //�p�X�̍쐬
            imagePathCheck(path);
            filename = title + getTimeStamp(_timeStampStyle) + ".png";

            //���X�̔w�i�F��Cache
            Color32 CacheColor = _UseCamera.backgroundColor;
            //�w�i�F�F����
            if (_buckGroundColorType == BACK_GROUND_COLOR.Alpha)
            {
                _UseCamera.backgroundColor = new Color32(0, 0, 0, 0);
                _UseCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            }
            //�w�i�F�F�J�X�^��
            if (_buckGroundColorType == BACK_GROUND_COLOR.CustomColor)
            {
                _UseCamera.backgroundColor = _customColor;
                _UseCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            }
            //�w�i�F�F�X�J�C�{�b�N�X
            if (_buckGroundColorType == BACK_GROUND_COLOR.Skybox)
            {
                _UseCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
            }

            //�X�N�V���쐬
            //�����o���T�C�Y�̎擾
            Vector2Int size = getScreenSizePixel2Int(_screenSizePixel);
            Texture2D screenShot = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
            RenderTexture rt = new RenderTexture(screenShot.width, screenShot.height, 32);
            RenderTexture prev = _UseCamera.targetTexture;
            _UseCamera.targetTexture = rt;
            _UseCamera.Render();
            _UseCamera.targetTexture = prev;
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
            screenShot.Apply();
            byte[] bytes = screenShot.EncodeToPNG();
            Debug.Log("�o�C�g�񒷂�:" + bytes.Length);
            //UnityEngine.Object.Destroy(screenShot);


            // Texture2D��j��
            UnityEngine.Object.Destroy(screenShot);

            // RenderTexture�����
            rt.Release();
            Destroy(rt);

            //��������
            //File.WriteAllBytes(path + filename, bytes);
            //tss.filedata = bytes;

            OutputItem Test = new OutputItem(jobId, bytes);
            Debug.Log(Test.jobId);
            Debug.Log(Test.filedata.Length);

            Q2.Enqueue(Test);


            //���O�̕\��
            if (_consoleLogIsActive)
            {
                Debug.Log("Title: " + filename);
                Debug.Log("Directory: " + path);
            }

            //�J�����̔w�i�F�����ɖ߂�
            _UseCamera.backgroundColor = CacheColor;
            //Asset�t�H���_�̃����[�h
            UnityEditor.AssetDatabase.Refresh();
        }

        //�t�@�C���p�X�̊m�F
        private void imagePathCheck(string path)
        {
            if (Directory.Exists(path))
            {
                //Debug.Log("The path exists");
            }
            else
            {
                //�p�X�����݂��Ȃ���΃t�H���_���쐬
                Directory.CreateDirectory(path);
                Debug.Log("CreateFolder: " + path);
            }
        }

        private Vector2Int getScreenSizePixel2Int(SCREEN_SIZE_PIXEL ScreenSize)
        {
            //�f�t�H���g�ݒ�
            Vector2Int size = new Vector2Int(1024, 1024);
            switch (ScreenSize)
            {
                //1:1(�����`)
                case SCREEN_SIZE_PIXEL.p256x256:
                    size = new Vector2Int(256, 256);
                    break;
                case SCREEN_SIZE_PIXEL.p512x512:
                    size = new Vector2Int(512, 512);
                    break;
                case SCREEN_SIZE_PIXEL.p1024x1024:
                    size = new Vector2Int(1024, 1024);
                    break;
                case SCREEN_SIZE_PIXEL.p2048x2048:
                    size = new Vector2Int(2048, 2048);
                    break;
                case SCREEN_SIZE_PIXEL.p4096x4096:
                    size = new Vector2Int(4096, 4096);
                    break;

                //16:9
                case SCREEN_SIZE_PIXEL.p1280x720:
                    size = new Vector2Int(1280, 720);
                    break;
                case SCREEN_SIZE_PIXEL.p1920x1080:
                    size = new Vector2Int(1920, 1080);
                    break;
                case SCREEN_SIZE_PIXEL.p2560x1440:
                    size = new Vector2Int(2560, 1440);
                    break;
                case SCREEN_SIZE_PIXEL.p3840x2160:
                    size = new Vector2Int(3840, 2160);
                    break;

                //CustomscreenSize
                case SCREEN_SIZE_PIXEL.CustomSize:
                    //UpperLimit
                    if (_customSize.x > 4096)
                    {
                        _customSize.x = 4096;
                        Debug.LogWarning("PixelSize��X��4096�ɐݒ肵�܂����B");
                    }
                    if (_customSize.y > 4096)
                    {
                        _customSize.y = 4096;
                        Debug.LogWarning("PixelSize��Y��4096�ɐݒ肵�܂����B");
                    }
                    //UnderLimit
                    if (_customSize.x < 32)
                    {
                        _customSize.x = 32;
                        Debug.LogWarning("PixelSize��X��32�ɐݒ肵�܂����B");
                    }
                    if (_customSize.y < 32)
                    {
                        _customSize.y = 32;
                        Debug.LogWarning("PixelSize��Y��32�ɐݒ肵�܂����B");
                    }
                    size = _customSize;
                    break;

                //�ݒ肳��ĂȂ�SCREEN_SIZE_PIXEL���I�����ꂽ�ꍇ
                default:
                    Debug.LogWarning("�ݒ肳��ĂȂ�SCREEN_SIZE_PIXEL���I������܂����B");
                    Debug.LogWarning("�𑜓x " + size + " �ŏ����o���܂��B");
                    break;
            }
            return size;
        }

        //�^�C���X�^���v
        private string getTimeStamp(TIME_STAMP type)
        {
            string time;
            //�^�C���X�^���v�̐ݒ菑�������܂�
            switch (type)
            {
                case TIME_STAMP.MMDDHHMMSS:
                    time = DateTime.Now.ToString("MMddHHmmss");
                    return time;
                case TIME_STAMP.YYYYMMDDHHMMSS:
                    time = DateTime.Now.ToString("yyyyMMddHHmmss");
                    return time;
                default:
                    time = DateTime.Now.ToString("yyyyMMddHHmmss");
                    return time;
            }
        }

#endif
    }
}
