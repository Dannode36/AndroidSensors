using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.IO;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

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
    public RawImage IMAGE;

    public TMP_Text XAxis;
    public TMP_Text YAxis;
    public TMP_Text ZAxis;
    public Toggle recordToggle;
    public TMP_InputField rateInput;
    public Button SaveButton;
    public Button ClearButton;
    public Button CalibrateButton;

    public TMP_InputField minRecValueInput;
    public TMP_InputField maxRecValueInput;
    private float minRecValue = -2f;
    private float maxRecValue = 2f;

    bool calibrating;
    public List<Triple> recData = new();
    Color bgColour = new(0.1921569f, 0.3019608f, 0.4745098f);
    readonly Triple calibratedOffset = new(0, 0, 0);
    public int calibrationRequirement = 240;

    void Start()
    {
        rateInput.onSubmit.AddListener(SetRefreshRate);
        minRecValueInput.onSubmit.AddListener(SetMinRecordingValue);
        maxRecValueInput.onSubmit.AddListener(SetMaxRecordingValue);

        recordToggle.onValueChanged.AddListener(SetRecording);
        SaveButton.onClick.AddListener(SaveRecordingCache);
        ClearButton.onClick.AddListener(ClearRecordingCache);
        CalibrateButton.onClick.AddListener(Calibrate);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!calibrating)
        {
            XAxis.text = (Input.acceleration.x - calibratedOffset.x).ToString();
            YAxis.text = (Input.acceleration.y - calibratedOffset.y).ToString();
            ZAxis.text = (Input.acceleration.z - calibratedOffset.z).ToString();

            //Debug.Log("Tick");
            if (recordToggle.isOn)
            {
                recData.Add(new Triple(
                    Input.acceleration.x - calibratedOffset.x,
                    Input.acceleration.y - calibratedOffset.y,
                    Input.acceleration.z - calibratedOffset.z));
                Debug.Log(recData.Count);
            }
        }
        else
        {
            if(recData.Count < calibrationRequirement) 
            {
                Debug.Log("ASOLHJd");
                recData.Add(new Triple(
                    Input.acceleration.x,
                    Input.acceleration.y,
                    Input.acceleration.z));
            }
            else
            {
                foreach (var triple in recData)
                {
                    calibratedOffset.x += triple.x;
                    calibratedOffset.y += triple.y;
                    calibratedOffset.z += triple.z;
                }

                calibratedOffset.x /= calibrationRequirement;
                calibratedOffset.y /= calibrationRequirement;
                calibratedOffset.z /= calibrationRequirement;

                recData.Clear();
                Camera.main.backgroundColor = bgColour;
                calibrating = false;
            }
        }
    }

    public void SetRefreshRate(string frequency)
    {
        if (!calibrating)
        {
            Time.fixedDeltaTime = 1 / float.Parse(frequency.Replace("Hz", ""));
            if (!rateInput.text.Contains("Hz"))
            {
                rateInput.text += " Hz";
            }
        }
    }
    public void Calibrate()
    {
        recData.Clear();
        Time.fixedDeltaTime = 1 / 60f;
        Camera.main.backgroundColor = Color.yellow;
        calibrating = true;
    }

    public void SetRecording(bool value)
    {
        if(!calibrating)
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
    }
    public void SetMinRecordingValue(string value)
    {
        float parsed = float.Parse(value.Replace("G", ""));
        if(parsed < maxRecValue)
        {
            minRecValue = parsed;
            if (!minRecValueInput.text.Contains("G"))
            {
                minRecValueInput.text += " G";
            }
        }
        else
        {
            minRecValueInput.text = minRecValue + " G";
            Debug.LogError("Min recording value can not be greater or equal to the max recording value");
        }
    }
    public void SetMaxRecordingValue(string value)
    {
        float parsed = float.Parse(value.Replace("G", ""));
        if (parsed > minRecValue)
        {
            maxRecValue = parsed;
            if (!maxRecValueInput.text.Contains("G"))
            {
                maxRecValueInput.text += " G";
            }
        }
        else
        {
            maxRecValueInput.text = maxRecValue + " G";
            Debug.LogError("Max recording value can not be lesser or equal to the min recording value");
        }
    }
    public void ClearRecordingCache()
    {
        if (!calibrating)
        {
            recData.Clear();
        }
    }
    public void SaveRecordingCache()
    {
        if (!calibrating)
        {
            //Encode data as an image (First a Texture2D)
            int magicLength = Mathf.CeilToInt(Mathf.Sqrt(recData.Count));
            var tex = new Texture2D(magicLength, magicLength);

            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    //Gets from 1D array using 2D coords. Takes the x pos and adds any previous completed rows (y * tex.width)
                    int index = x + (y * tex.width);

                    //Avoids overruns and inserts a "0"
                    if (index >= recData.Count)
                    {
                        tex.SetPixel(x, y, Color.black);
                        continue;
                    }
                    Triple triple = recData[index];
                    tex.SetPixel(x, y, ClampC(triple));
                }
            }
            tex.Apply();

            IMAGE.texture = tex;
            IMAGE.mainTexture.filterMode = FilterMode.Point;
            byte[] texBytes = tex.EncodeToPNG();
            //Array.Reverse(texBytes);
#if UNITY_EDITOR
            File.WriteAllBytes("Data " + Guid.NewGuid().ToString() + ".png", texBytes);
#endif
            File.WriteAllBytes(Application.persistentDataPath + "Data " + Guid.NewGuid().ToString() + ".png", texBytes);
        }
    }

    Color ClampC(Triple triple)
    {
        //Range to clamp between 0-1
        float div = maxRecValue - minRecValue;
        return new Color(
            ClampF(triple.x) / div,
            ClampF(triple.y) / div,
            ClampF(triple.z) / div);
    }
    float ClampF(float value)
    {
        return Math.Clamp(value, minRecValue, maxRecValue);
    }
}
