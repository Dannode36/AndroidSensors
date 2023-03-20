using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.IO;

[Serializable]
public class Triple
{
    public float x;
    public float y;
    public float z;

    public Triple(float x, float y, float z)
    {
        this.x = x; 
        this.y = y; 
        this.z = z;
    }
}

public class AccelerometerReader : MonoBehaviour
{
    public TMP_Text XAxis;
    public TMP_Text YAxis;
    public TMP_Text ZAxis;
    public Toggle recordToggle;
    public TMP_InputField rateInput;
    public Button SaveButton;
    public Button ClearButton;

    public List<Triple> recData = new();
    Color bgColour = new(0.1921569f, 0.3019608f, 0.4745098f);

    void Start()
    {
        rateInput.onSubmit.AddListener(SetRefreshRate);
        recordToggle.onValueChanged.AddListener(SetRecording);
        SaveButton.onClick.AddListener(SaveRecordingCache);
        ClearButton.onClick.AddListener(ClearRecordingCache);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        XAxis.text = Input.acceleration.x.ToString();
        YAxis.text = Input.acceleration.y.ToString();
        ZAxis.text = Input.acceleration.z.ToString();

        //Debug.Log("Tick");
        if (recordToggle.isOn)
        {
            recData.Add(new Triple(
                Input.acceleration.x,
                Input.acceleration.y,
                Input.acceleration.z));
            Debug.Log(recData.Count);
        }
    }

    public void SetRefreshRate(string frequency)
    {
        Time.fixedDeltaTime = 1 / float.Parse(frequency.Replace("Hz", ""));
        if (!rateInput.text.Contains("Hz"))
        {
            rateInput.text += " Hz";
        }
    }

    public void SetRecording(bool value)
    {
        if (value)
        {
            Camera.main.backgroundColor = Color.red;
        }
        else
        {
            Camera.main.backgroundColor = bgColour;
        }
    }
    public void ClearRecordingCache()
    {
        recData.Clear();
    }
    public void SaveRecordingCache()
    {
        //Encode data as an image (First a Texture2D)
        var tex = new Texture2D((recData.Count + 1) / 2, (recData.Count + 1) / 2);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                //Gets from 1D array using 2D coords. Takes the x pos and adds any previous completed rows (y * tex.width)
                int index = x + y * tex.width;
                if(index >= recData.Count) { break; }
                Triple triple = recData[index];

                tex.SetPixel(x, y, new Color(Math.Clamp((triple.x + 4f) / 8f, 0, 1), Math.Clamp((triple.y + 4f) / 8f, 0, 1), Math.Clamp((triple.z + 4f) / 8f, 0, 1)));
            }
        }
        tex.Apply();

        byte[] texBytes = tex.EncodeToPNG();
        File.WriteAllBytes(Application.persistentDataPath + "Data " + Guid.NewGuid().ToString() + ".png", texBytes);
    }
}
