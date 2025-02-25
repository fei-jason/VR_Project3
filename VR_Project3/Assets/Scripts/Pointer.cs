/* SceneHandler.cs*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.Extras;
using TMPro;
using OpenAI;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class Pointer : MonoBehaviour
{
    public SteamVR_LaserPointer laserPointer;

    public TMP_Text textField;
    public TMP_InputField inputField;
    public TextAsset textFile;
    public TextAsset textFile2;
    public Slider slider;
    
    // canvas
    public Canvas canvas;
    private bool isCanvas = true;

    // record buttons
    public GameObject recordButton;



    //scene transition
    public FadeScreen fadeScreen;

    // extras
    //[SerializeField] private Button recordButton;
    [SerializeField] private Image progressBar;

    // speech to text
    private OpenAIApi openai = new OpenAIApi("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");
    private readonly string fileName = "output.wav";
    private readonly int duration = 3;
    private float startRecordingTime;
    public AudioClip clip;
    public AudioSource audioSource;
    private bool isRecording;
    private float time;



    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        //index = PlayerPrefs.GetInt("user-mic-device-index");
        //Debug.Log(index);
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }

        Debug.Log("Current Microphone: " + Microphone.devices[0]);

        //recordButton.onClick.AddListener(StartRecording);

    }

    void Awake()
    {
        laserPointer.PointerIn += PointerInside;
        laserPointer.PointerOut += PointerOutside;
        laserPointer.PointerClick += PointerClick;
    }

    public void PointerClick(object sender, PointerEventArgs e)
    {
        switch (e.target.name)
        {
            case "OkButton":
                Debug.Log("Ok was clicked");
                GetResponse();
                break;
            case "Rosie":
                if (isCanvas == true) {
                    isCanvas = false;
                    canvas.enabled = true;
                } else {
                    isCanvas = true;
                    canvas.enabled = false;
                }
                break;  
            case "RecordButton":
                Debug.Log("Record Button Clicked");
                break;
            case "FruitButton":
                Debug.Log("Clicked button");
                GoToScene(1);
                break;
        }
    }

    public void PointerInside(object sender, PointerEventArgs e)
    {
        switch (e.target.name)
        {
            case "OkButton":
                laserPointer.color = Color.yellow;
                break;
            case "Rosie":
                laserPointer.color = Color.yellow;
                break;
            case "FruitButton":
                laserPointer.color = Color.blue;
                break;
        }
    }

    public void PointerOutside(object sender, PointerEventArgs e)
    {
        switch (e.target.name)
        {
            case "OkButton":
                laserPointer.color = Color.black;
                break;
            case "Rosie":
                laserPointer.color = Color.black;
                break;
            case "FruitButton":
                laserPointer.color = Color.black;
                break;             
        }
    }

    public async void GetResponse()
    {
        // STILL TESTING ALL OF THIS
        // Debug.Log(textFile.text);
        textField.text = string.Format(textFile.text);

        Debug.Log(textFile2.text);

        string s = textFile2.text;

        Match match = Regex.Match(s, @"\d+/\d+");
        if (match.Success) {
            string[] parts = match.Value.Split('/');
            float value = (float)Convert.ToInt32(parts[0]) / Convert.ToInt32(parts[1]);
            Debug.Log(value);
            slider.value = value;
        } else {
            Debug.Log("was not able to compute intimacy");
        }
    }

    public void StartRecording()
    {
        isRecording = true;
        //recordButton.SetActive(false);

        clip = Microphone.Start(Microphone.devices[0], false, duration, 44100);
    }

    public async void EndRecording()
    {
        Microphone.End(null);
        byte[] data = SaveWav.Save(fileName, clip);

        var req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = data, Name = "audio.wav" },
            // File = Application.persistentDataPath + "/" + fileName,
            Model = "whisper-1",
            Language = "en"
        };
        var res = await openai.CreateAudioTranscription(req);

        progressBar.fillAmount = 0;
        inputField.text = res.Text;
        Debug.Log(res.Text);
        Debug.Log("inside method: EndRecording");
        //recordButton.SetActive(true);
    }

    void Update()
    {
        if (isRecording)
        {
            time += Time.deltaTime;
            progressBar.fillAmount = time / duration;

            if (time >= duration)
            {
                time = 0;
                isRecording = false;
                EndRecording();
            }
        }
    }


    public void GoToScene(int sceneIndex)
    {
        StartCoroutine(GoToSceneRoutine(sceneIndex));
    }

    IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen.fadeDuration);

        //Launch new scene
        SceneManager.LoadScene(sceneIndex);
        fadeScreen.FadeIn();
    }
}