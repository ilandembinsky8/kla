using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SerialController))]
public class SerialEventDispatcher : MonoBehaviour
{
    string cfg = Application.streamingAssetsPath + "/cfg.ini";
    [Tooltip("Use one event for each trigger")]
    public UnityEvent<string>[] eventDispatch;

    SerialController sc;
    // Start is called before the first frame update
    void Awake()
    {
        sc = GetComponent<SerialController>();

        if (File.Exists(cfg)){
            string text = File.ReadAllLines(cfg)[0];
            Debug.Log(text);
            if (text != "")
                sc.portName = text;
        }
        else{
            Debug.LogWarning($"config file {cfg} not found");
        }

    }

    void Start(){
    }

    // Update is called once per frame
    void Update()
    {
        string s = sc.ReadSerialMessage();
        if (s != null){

            string[] split = s.Split(':');

            if (split.Length > 0){
                if (int.TryParse(split[0], out int triggerIndex) && triggerIndex < eventDispatch.Length){ // try to get index from first part and check if it is withing range
                    string message = split.Length > 1 ? split[1] : ""; // check if parameters exist to be sent

                    eventDispatch[triggerIndex].Invoke(message);
                }
            }
        }
    }
}
