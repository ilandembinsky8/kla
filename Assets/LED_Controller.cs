using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(SerialController))]
public class LED_Controller : MonoBehaviour
{
    [SerializeField] bool useConfigFile, shouldDelayTransmit;
    [SerializeField] float delayTime;
    string cfg = Application.streamingAssetsPath + "/cfg.ini";
    SerialController sc;
    Queue<string> messageQueue;
    // Start is called before the first frame update
    void Awake()
    {
        messageQueue = new Queue<string>();
        sc = GetComponent<SerialController>();

        if (useConfigFile && File.Exists(cfg)){
            string text = File.ReadAllLines(cfg)[0];
            Debug.Log(text);
            if (text != ""){
                sc.portName = text;
                Debug.Log($"COM port set to {sc.portName}");
            }
        }
        else if (useConfigFile){
            Debug.LogWarning($"config file {cfg} not found");
        }
    }

    void Start()
    {
        StartCoroutine(DelayTransmit());
    }

    void Transmit(string message){
        if (shouldDelayTransmit){
            messageQueue.Enqueue(message);
        } else {
            sc.SendSerialMessage(message);
        }
    }

    IEnumerator DelayTransmit(){
        while (true){
            if (messageQueue.Count > 0){
                sc.SendSerialMessage(messageQueue.Dequeue());
                yield return new WaitForSeconds(delayTime);
            }
            yield return new WaitForEndOfFrame();
        }
    }
    
    /// <summary>
    /// Scan will produce a top to bottom gradial change
    /// </summary>
    /// <param name="firstColor">The color to start with</param>
    /// <param name="secondColor">The color to end with</param>
    /// <param name="time">transition duration in seconds</param>
    public void Scan(Color firstColor, Color secondColor, float time){
        RGBW startColor = RGBW.FromColor(firstColor);
        RGBW endColor = RGBW.FromColor(secondColor);

        string message = $"scan:{startColor}:{endColor}:{time}";
        Transmit(message);
    }
    /// <summary>
    /// Fade into and out of selected color
    /// </summary>
    /// <param name="color">The color for fading</param>
    /// <param name="fadeInTime">Time in seconds for color to fade in</param>
    /// <param name="fadeOutTime">Time in seconds for color to fade out</param>
    /// <param name="restTime">Time in seconds between fading in and fading out</param>
    public void Fade(Color color, float fadeInTime, float fadeOutTime, float restTime){
        string message = $"fade:{RGBW.FromColor(color)}:{fadeInTime}:{fadeOutTime}:{restTime}";
        Transmit(message);
    }
    /// <summary>
    /// Flicker alternates between 2 colors for a duration, each color is segmented to its own pixels (for high density led fixtures)
    /// </summary>
    /// <param name="color1">The color for even-numbered segments</param>
    /// <param name="color2">The color for odd-numbered segments</param>
    /// <param name="duration">The duration in seconds of flickering function</param>
    /// <param name="interval">The duration in seconds for each color to show</param>
    public void Flicker(Color color1, Color color2, float duration, float interval){

        string message = $"flic:{RGBW.FromColor(color1)}:{RGBW.FromColor(color2)}:{duration}:{interval}";
        Transmit(message);

    }

    /// <summary>
    /// Stops the current running operation and set the led to 0 on all channels
    /// </summary>
    public void Terminate(){
        Transmit("*");
    }
}

struct RGBW{
    public byte R, G, B, W;

    public static RGBW FromColor(Color32 color){
        return new RGBW{
            R = (byte)color.r,
            G = (byte)color.g,
            B = (byte)color.b,
            W = (byte)color.a
        };
    }

    public override string ToString()
    {
        return $"{R}-{G}-{B}-{W}";
    }
}