using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using RosSharp.RosBridgeClient;

public class Countdown : MonoBehaviour
{
    public int count = 3;
    public int timer = 60;
    public static KeyCode ToggleCountdown = KeyCode.Space;
    public AudioClip Audio1;
    public AudioClip Audio2;
    public AudioClip Audio3;
    public AudioClip AudioStart;
    public AudioClip AudioStop;

    [HideInInspector]
    public static bool started;
    public static bool counting;
    public static bool timing;
    public static bool stopped;
    [HideInInspector]
    public float elapsed = 1f;
    [HideInInspector]
    public int i = 1;
    [HideInInspector]
    public static string log = "Stopped";


    private Text text;
    private float timeStart;
    private float time;
    private new AudioSource audio;
    private List<AudioClip> audioclips = new List<AudioClip>();
    private int seconds = 0;

    // Start is called before the first frame update
    void Start()
    {
        audioclips.Add(Audio3);
        audioclips.Add(Audio2);
        audioclips.Add(Audio1);
        audioclips.Add(AudioStart);
        audioclips.Add(AudioStop);

        audio = GetComponent<AudioSource>();
        started = false;
        counting = false;
        stopped = false;
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(ToggleCountdown) || stopped == true)
        {
            if ((!counting) && (!started))
            {
                text.text = "Counting down...";
                counting = true;
                Debug.Log("Counting down...");
            } 
            else
            {
                //PrintTime();
                text.text = "Stop!";
                audio.clip = AudioStop;
                PoscmdPublisher.connect = false;                
                audio.Play();
                started = false;
                counting = false;
                timing = false;
                stopped = false;
                time = 0f;
                i = 0;
                elapsed = 0f;
                time = 0f;
                Debug.Log("Stopping...");
                log = "Stopped";
            }
        };

        if (counting)
        {
            CountingDown();
        }

        if (timing)
        {
            elapsed += Time.deltaTime;
            print(elapsed);

            if (elapsed >= LogPrepper.timer)
            {
                stopped = true;
            }
        }


    }

    void CountingDown()
    {        
        elapsed += Time.deltaTime;

        if ((elapsed >= 1f) && (i < count))
        {
            elapsed = elapsed % 1f;
            int temp = count - i;
            log = temp.ToString();
            text.text = log;
            Debug.Log(count - i);
            audio.clip = audioclips[i];
            audio.Play();
            i++;
        }
        else if ((elapsed >= 1f) && (i == count))
        {
            elapsed = elapsed % 1f;
            log = "Start!";
            timeStart = Time.time;
            text.text = log;
            Debug.Log(log);
            audio.clip = AudioStart;
            audio.Play();
            started = true;
            i++;
        }
        else if ((elapsed >= 1f) && (i > count))
        {
            text.text = "";
            counting = false;
            timing = true;
            i = 1;
        }


    }

    void PrintTime()
    {
        time = Time.time - timeStart;
        Debug.Log(time);
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time %60);

        //Debug.Log(minutes);
        //Debug.Log(seconds);
        string niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        text.text = "Time: " + niceTime;

        string subjectDataFolder = LogPrepper.subjectDataFolder;       
        Directory.CreateDirectory(subjectDataFolder + "/" + time.ToString());
    }
}

