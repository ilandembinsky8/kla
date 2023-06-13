using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SerialController))]
public class LED_Controller : MonoBehaviour
{
    SerialController sc;
    // Start is called before the first frame update
    void Start()
    {
        sc = GetComponent<SerialController>();

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
        Debug.Log(startColor);

        string message = $"scan:{startColor}:{endColor}:{time}";
        sc.SendSerialMessage(message);
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
        sc.SendSerialMessage(message);
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
        sc.SendSerialMessage(message);

    }

    /// <summary>
    /// Stops the current running operation and set the led to 0 on all channels
    /// </summary>
    public void Terminate(){
        sc.SendSerialMessage("*");
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