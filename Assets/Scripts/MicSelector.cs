using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum ClipType{ RecordedClip, MicrophoneClip }

public class MicSelector : MonoBehaviour
{
    public static MicSelector Instance { get; private set; }

    [Header("Object References")]

    public TMP_Dropdown micDropdown;
    public AudioSource audioSource;
    public AudioClip recordedClip;
    public AudioClip micClip;

    public string selectedMic;
    private bool micStarted = false;
    private bool recordStarted = false;

    [Header("Volume Calculation Settings")]

    private float[] sampleData;
    private int lastSamplePos;
    private int sampleToProcess;

    private float sumOfSquares = 0f;
    private float meanSquare = 0f;
    private float rmsValue = 0f;
    public float CurrentVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();  
        micDropdown = FindObjectOfType<TMP_Dropdown>();
        RefreshMicList();
    }

    void RefreshMicList()
    {
        micDropdown.ClearOptions();

        List<string> options = new List<string>(Microphone.devices);
        micDropdown.AddOptions(options);

        if (options.Count > 0)
        {
            selectedMic = options[0];
            micDropdown.value = 0;
        }
        else
        {
            Debug.LogWarning("No microphone devices found.");
        }

        micDropdown.onValueChanged.AddListener(OnMicChanged);
    }

    void OnMicChanged(int index)
    {
        selectedMic = micDropdown.options[index].text;
        Debug.Log("Selected Mic: " + selectedMic);
    }

    public void StartMic(ClipType selectedClip)
    {
        
        if (string.IsNullOrEmpty(selectedMic))
        {
            Debug.LogWarning("No microphone devices found.");
            return;
        }
        switch (selectedClip)
        {
            case ClipType.RecordedClip:
                recordedClip = Microphone.Start(selectedMic, false, 60, 44100);
                recordStarted = true;
                Debug.Log("KAYIT BAÞLADI");
                break;

            case ClipType.MicrophoneClip:
                micClip = Microphone.Start(selectedMic, true, 1, 44100);
                sampleData = new float[micClip.samples];
                micStarted = true;
                while (Microphone.GetPosition(selectedMic) <= 0) { }
                Debug.Log("Mikrofon dinlemesi baþladý: " + selectedMic);
                break;
        }
    }
    public void StartRecord()
    {
        StartMic(ClipType.RecordedClip);
    }

    public void StartMicrophone()
    {
        StartMic(ClipType.MicrophoneClip);
    }

    public void StopMic()
    {
        if (!recordStarted) return;

        int pos = Microphone.GetPosition(selectedMic);
        if (pos <= 0) 
        {
            Microphone.End(selectedMic);
            recordStarted = false;
            Debug.LogWarning("Kayýt verisi yok veya çok kýsa.");
            return;
        }


        float[] data = new float[pos * recordedClip.channels];
        recordedClip.GetData(data, 0);

        Microphone.End(selectedMic);

        AudioClip trimmed = AudioClip.Create("TrimmedClip", pos, recordedClip.channels, recordedClip.frequency, false);
        trimmed.SetData(data, 0);
        audioSource.clip = trimmed;

        recordStarted = false;
        Debug.Log($"KAYIT BÝTTÝ - Trim Süresi: {(pos / (float)recordedClip.frequency):F2} sn");
    }

    public void PlayRecord()
    {
        
        audioSource.Play();
        Debug.Log("KAYIT OYNUYOR");
    }
    
    void VolumeCal()
    {
        if (micStarted)
        {
            int currentSamplePos = Microphone.GetPosition(selectedMic);
            if (currentSamplePos == lastSamplePos)
            {
                return; // Yeni veri yok   
            }

            micClip.GetData(sampleData, 0);

            if (currentSamplePos > lastSamplePos)
            {
                sampleToProcess = currentSamplePos - lastSamplePos;
            }
            else
            {
                sampleToProcess = (micClip.samples - lastSamplePos) + currentSamplePos;
            }

            sumOfSquares = 0f;
            for (int i = 0; i < sampleToProcess; i++)
            {
                int index = (lastSamplePos + i) % sampleData.Length;
                float sample = sampleData[index];

                sumOfSquares += sample * sample;
            }
            if (sampleToProcess > 0)
            {
                meanSquare = sumOfSquares / sampleToProcess;
                rmsValue = Mathf.Sqrt(meanSquare);
                CurrentVolume = rmsValue;
            }
            lastSamplePos = currentSamplePos;
        }
    }

    private void Update()
    {
        VolumeCal();
    }
    
}       
