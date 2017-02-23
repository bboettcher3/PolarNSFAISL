﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class VisualizeEvent : MonoBehaviour {

    public string eventFile;
    public string eventDirectory;
    //public GameObject particle;
    public bool eventPlaying = false;
    private int currIndex = 0;
    private const float BELOW_ICE = -1950.0f;
    private GameObject eventSphere;
    public float playSpeed = 0.01f;
    private float eventStartTime = 0.0f;
    private float eventEndTime = 0.0f;
    private float playStartTime = 0.0f;
    private float playEndTime = 0.0f;
    private bool advancedIndex = false;
    private int currEvent = 0;
    private float newPlayTime = 0.0f;
    private DomData domData;

    public struct EventData
    {
        public int str;
        public int dom;
        public Vector3 pos;
        public float charge;
        public float time;
    };

    public struct EventVis
    {
        public List<EventData> eventData;
        public string fileName;
        public Vector3 startPos;
        public Vector3 endPos;
    };

    public List<EventVis> events = new List<EventVis>();

    public static int IntParseFast(string value)
    {
        int result = 0;
        for (int i = 0; i < value.Length; i++)
        {
            char letter = value[i];
            result = 10 * result + (letter - 48);
        }
        return result;
    }

	// Use this for initialization
	void Start () {

        if(eventDirectory.Length > 0)
        {
            string[] files = System.IO.Directory.GetFiles(eventDirectory);
            //events = new List<EventVis>();
            
            foreach (string file in files)
            {
                if (file.EndsWith(".txt"))
                {
                    EventVis e = new EventVis();
                    e.eventData = new List<EventData>();
                    e.fileName = file;
                    Debug.Log(file);
                    StreamReader sr = new StreamReader(e.fileName, Encoding.Default);
                    string s = sr.ReadLine();
                    int lineCount = 0;
                    while (s != null)
                    {
                        if (lineCount < 2)
                        {
                            string[] data = s.Split(' ');
                            float x = (float)double.Parse(data[0]);
                            float y = (float)double.Parse(data[1]);
                            float z = (float)double.Parse(data[2]);
                            if(lineCount < 1)
                            {
                                e.startPos = new Vector3(x * 0.3048f, y * 0.3048f, z * 0.3048f);
                            }
                            else 
                            {
                                e.endPos = new Vector3(x * 0.3048f, y * 0.3048f, z * 0.3048f);
                            }
                        }
                        else
                        {
                            string[] data = s.Split('\t');
                            if(data.Length != 7)
                            {
                                data = s.Split(' ');
                            }
                            EventData d;
                            d.str = IntParseFast(data[0])-1;
                            d.dom = IntParseFast(data[1])-1;
                            d.pos.x = (float)double.Parse(data[2]);
                            d.pos.y = BELOW_ICE + (float)double.Parse(data[4]);
                            d.pos.z = (float)double.Parse(data[3]);
                            d.charge = (float)double.Parse(data[5]);
                            d.time = (float)double.Parse(data[6]);
                            e.eventData.Add(d);
                        }

                        lineCount++;
                        s = sr.ReadLine();
                    }

                    e.eventData.Sort((s1, s2) => s1.time.CompareTo(s2.time));
                    events.Add(e);
                }
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        
        float t = UnityEngine.Time.time;

        //r or every 60 seconds
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.R) || t - newPlayTime > 60.0f)
        {
            newPlayTime = t;
            Debug.Log("Playing event!");
            eventPlaying = true;
            currIndex = 0;
            
            currEvent = UnityEngine.Random.Range(0, events.Count);
            eventStartTime = events[currEvent].eventData[0].time;
            playStartTime = t;
            eventEndTime = events[currEvent].eventData[events[currEvent].eventData.Count - 1].time;
            eventFile = events[currEvent].fileName;
            advancedIndex = true;
        }

	    if(eventPlaying)
        {
            if (currIndex < events[currEvent].eventData.Count && advancedIndex)
            {
                if (domData == null)
                {
                    domData = gameObject.GetComponent<DomData>();
                }

                GameObject d = domData.DOMArray[events[currEvent].eventData[currIndex].dom, events[currEvent].eventData[currIndex].str];
                if (d != null)
                {
                    float fTimeFrac = (events[currEvent].eventData[currIndex].time - eventStartTime) / (eventEndTime - eventStartTime);
                    d.GetComponent<DOMController>().TurnOn(fTimeFrac, Mathf.Log(10000.0f * events[currEvent].eventData[currIndex].charge * events[currEvent].eventData[currIndex].charge));
                }
                
                //Vector3 dir = (events[currEvent].endPos - events[currEvent].startPos);
                //float mag = (events[currEvent].endPos - events[currEvent].startPos).magnitude;
                //particle.transform.position = events[currEvent].startPos + dir * fTimeFrac;
            }
            
            //advance index depending on timing...
            if (currIndex < events[currEvent].eventData.Count - 1)
            {
                if ((events[currEvent].eventData[currIndex + 1].time - eventStartTime) > (t - playStartTime) * playSpeed)
                {
                    currIndex++;
                }
                else
                {
                    //spin the existing spheres?
                    //fade out option?
                    advancedIndex = false;
                }
            }

            if (currIndex >= events[currEvent].eventData.Count - 1)
            {
                Debug.Log("Stopped playing");
                currIndex = 0;
                eventPlaying = false;
                advancedIndex = false;
                playStartTime = 0.0f;
                playEndTime = 0.0f;
                eventStartTime = 0.0f;
                eventEndTime = 0.0f;

                //turn off all event visualization?
                for(int i = 0; i < events[currEvent].eventData.Count; ++i)
                {
                    GameObject d = domData.DOMArray[events[currEvent].eventData[i].dom, events[currEvent].eventData[i].str];
                    if(d != null)
                    {
                        d.GetComponent<DOMController>().TurnOff();
                    }
                }
            }
        }
	}
}
