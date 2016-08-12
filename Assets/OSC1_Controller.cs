using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class OSC1_Controller : MonoBehaviour
{
    private const string c_HttpHead = "http://";
    private const string c_HttpPort = "80";
    [SerializeField]
    private string m_IPAddress = "192.168.107.1"; // Gear360:192.168.107.1 / Bublcam:192.168.0.100 / RICOH THETA:192.168.1.1
                                                  //private string m_IPAddress = "192.168.1.1"; // Gear360:192.168.107.1 / Bublcam:192.168.0.100 / RICOH THETA:192.168.1.1
    private string sessionId;
    private bool is_ready = false;

    public Text debug_text;
    public Toggle enable_postview;
    public InputField n_shot_inputfiled;
    public InputField interval_time_inputfiled;

    void Start()
    {
        //StartCoroutine(StartThetaS());
        is_ready = true;
    }

    IEnumerator sleep(float wait_t)
    {
        yield return new WaitForSeconds(wait_t);
    }

    public void GetInfo()
    {
        is_ready = false;
        StartCoroutine(_GetInfo());
        is_ready = true;
    }

    IEnumerator _GetInfo()
    {
        string url = MakeAPIURL("/osc/info");
        WWW www = new WWW(url);
        yield return www;
        Debug.Log(www.text);
        debug_text.text = www.text;
    }

    public void TakePicture()
    {
        is_ready = false;
        StartCoroutine(_TakePictureProc());
        is_ready = true;
    }

    IEnumerator _TakePictureProc()
    {
        yield return StartCoroutine(_StartSession());
        yield return StartCoroutine(_TakePicture());
        yield return StartCoroutine(_CloseSession());
    }

    IEnumerator _StartSession()
    {
        Debug.Log("start session");

        Dictionary<string, string> header = new Dictionary<string, string>();
        header.Add("Content-Type", "application/json;charset=utf-8");

        string url = MakeAPIURL("/osc/commands/execute");
        string jsonStr = "{\"name\": \"camera.startSession\"}";
        byte[] postBytes = Encoding.Default.GetBytes(jsonStr);
        WWW www = new WWW(url, postBytes, header);
        yield return www;
        JsonNode json = JsonNode.Parse(www.text);
        sessionId = json["results"]["sessionId"].Get<string>();
        Debug.Log(sessionId);
    }

    IEnumerator _CloseSession()
    {
        Debug.Log("close session");

        // close session
        Dictionary<string, string> header = new Dictionary<string, string>();
        header.Add("Content-Type", "application/json;charset=utf-8");

        string url = MakeAPIURL("/osc/commands/execute");
        string jsonStr = "{\"name\": \"camera.closeSession\", \"parameters\": {\"sessionId\": \"" + sessionId + "\"}}";
        byte[] postBytes = Encoding.Default.GetBytes(jsonStr);
        WWW www = new WWW(url, postBytes, header);
        yield return www;
        debug_text.text = "ready";
    }

    IEnumerator _TakePicture(int n_shot=1)
    {
        Debug.Log("take picture");
        debug_text.text ="take picture";

        /*
        string url = MakeAPIURL("/osc/info");
        WWW www = new WWW(url);

        Dictionary<string, string> header = new Dictionary<string, string>();
        header.Add("Content-Type", "application/json;charset=utf-8");

        url = MakeAPIURL("/osc/commands/execute");
        string jsonStr = "{\"name\": \"camera.startSession\"}";
        byte[] postBytes = Encoding.Default.GetBytes(jsonStr);
        www = new WWW(url, postBytes, header);
        yield return www;
        JsonNode json = JsonNode.Parse(www.text);
        string sessionId = json["results"]["sessionId"].Get<string>();
        Debug.Log(sessionId);
        */

        Dictionary<string, string> header = new Dictionary<string, string>();
        header.Add("Content-Type", "application/json;charset=utf-8");

        string url = MakeAPIURL("/osc/commands/execute");
        string jsonStr;
        byte[] postBytes;
        WWW www;
        JsonNode json;

        jsonStr = "{\"name\": \"camera.takePicture\", \"parameters\": {\"sessionId\": \"" + sessionId + "\"}}";
        postBytes = Encoding.Default.GetBytes(jsonStr);
        www = new WWW(url, postBytes, header);
        yield return www;
        json = JsonNode.Parse(www.text);
        string photo_id = json["id"].Get<string>();
        Debug.Log(photo_id);
        Debug.Log(www.text);

        /*
        // theta
        string fileUri = "";
        url = MakeAPIURL("/osc/state");
        jsonStr = "{}";
        postBytes = Encoding.Default.GetBytes(jsonStr);
        while (fileUri == null)
        {
            www = new WWW(url, postBytes, header);
            yield return www;
            Debug.Log(www.text);
            json = JsonNode.Parse(www.text);
            fileUri = json["state"]["_latestFileUri"].Get<string>();
            Debug.Log(fileUri);
        }
        Debug.Log(fileUri);
        */

        string fileUri = "";
        url = MakeAPIURL("/osc/commands/status");
        jsonStr = "{\"id\":" + photo_id + "}";
        json = JsonNode.Parse(www.text);
        Debug.Log(jsonStr);

        postBytes = Encoding.Default.GetBytes(jsonStr);
        string state = "";
        while (state != "done")
        {
            www = new WWW(url, postBytes, header);
            yield return www;
            //Debug.Log(www.text);
            json = JsonNode.Parse(www.text);
            state = json["state"].Get<string>();
            //Debug.Log(state);
        }
        fileUri = json["results"]["fileUri"].Get<string>();
        Debug.Log(fileUri);
        debug_text.text = fileUri;

        // fileUriが取れても処理が終わっていない場合があったので少し待つ
        //yield return new WaitForSeconds(3);

        if (enable_postview.isOn)
        {
            url = MakeAPIURL("/osc/commands/execute");
            jsonStr = "{\"name\": \"camera.getImage\", \"parameters\": {\"fileUri\": \"" + fileUri + "\"}}";
            postBytes = Encoding.Default.GetBytes(jsonStr);
            www = new WWW(url, postBytes, header);
            yield return www;


            // 確認のためにRawImageに表示
            //RawImage rawImage = GameObject.Find("RawImage").GetComponent<RawImage>();
            //rawImage.texture = www.textureNonReadable;
            //rawImage.SetNativeSize();


            Texture2D texture = new Texture2D(2048, 1024);
            texture.LoadImage(www.bytes);
            Debug.Log(www.bytes.Length);
            //File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", www.bytes);

            //GameObject.Find("Plane").GetComponent<Renderer>().material.mainTexture = texture;
            GameObject.Find("Sphere100").GetComponent<Renderer>().material.mainTexture = texture;
        }

        // close session
        /*
        jsonStr = "{\"name\": \"camera.closeSession\", \"parameters\": {\"sessionId\": \"" + sessionId + "\"}}";
        postBytes = Encoding.Default.GetBytes(jsonStr);
        www = new WWW(url, postBytes, header);
        yield return www;
        Debug.Log(www);
        */

    }

    public void IntervalPicture()
    {
        is_ready = false;
        StartCoroutine(_IntervalPictureProc());
        is_ready = false;
    }

    IEnumerator _IntervalPictureProc()
    {
        Debug.Log("interval picture");
        int n_shot = int.Parse(n_shot_inputfiled.text);
        float interval_time = float.Parse(interval_time_inputfiled.text);

        n_shot_inputfiled.readOnly = true;
        interval_time_inputfiled.readOnly = true;

        yield return StartCoroutine(_StartSession());
        float pre_t, cur_t, wait_t;
        for (int i = n_shot; i > 0; i--) {
            n_shot_inputfiled.text = (i-1).ToString();

            pre_t = Time.realtimeSinceStartup;
            yield return StartCoroutine(_TakePicture());
            cur_t = Time.realtimeSinceStartup;

            wait_t = interval_time - (cur_t - pre_t);
            debug_text.text = "wait: " + wait_t.ToString() + "[sec]";
            yield return StartCoroutine(sleep(wait_t));

            pre_t = cur_t;
        }
        n_shot_inputfiled.readOnly = false;
        interval_time_inputfiled.readOnly = false;

        yield return StartCoroutine(_CloseSession());
    }

    private string MakeAPIURL(string command)
    {
        return string.Format("{0}{1}:{2}{3}", c_HttpHead, m_IPAddress, c_HttpPort, command);
    }
}