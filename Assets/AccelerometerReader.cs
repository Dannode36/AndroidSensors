using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AccelerometerReader : MonoBehaviour
{
    public TMP_Text XAxis;
    public TMP_Text YAxis;
    public TMP_Text ZAxis;

    // Update is called once per frame
    void FixedUpdate()
    {
        XAxis.text = Input.acceleration.x.ToString();
        YAxis.text = Input.acceleration.y.ToString();
        ZAxis.text = Input.acceleration.z.ToString();
    }
}
