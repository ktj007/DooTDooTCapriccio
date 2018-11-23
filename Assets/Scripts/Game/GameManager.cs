﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {

    // Game objects
    public GameObject pauseButton;
    public TextMeshProUGUI songName;
    public TextMeshProUGUI difficultyText;
    public Transform gameCanvas;
    public GameObject notePlatform;
    AudioManager audioManager;

    // Note objects
    public Transform noteObject;
    public Transform holdNoteObject;
    public Transform tailNoteObject;
    public Transform doubleNoteObject;
    public Transform doubleHoldObject;
    public Transform doubleTailObject;
    public Transform oneNoteOneTailObject;
    public Transform oneNoteOneHoldObject;
    public Transform oneHoldOneTailObject;
    Transform holdNoteInstance1;
    Transform holdNoteInstance2;
    Transform holdNoteInstance3;
    Transform holdNoteInstance4;
    Transform holdNoteInstance5;

    // Postgame objects
    public GameObject fireworksParticle;
    public GameObject fireworksText;

    // Song variables
    string song;
    string difficulty;
    int noteCount;
    float speedOffset;
    float noteSpeed;
    float songTimer;
    float dspStart;

    // Notechart variables
    string jsonPath;
    string json;
    NoteSpawn[] noteChart;
    int chartIndex = 0;

    // Use this for initialization
    void Start()
    {
        FindObjectOfType<Fade>().BeginFade(-1);
        InitPlayerPrefs();
        InitSongInfo();

        // load the note chart from the json file
        jsonPath = Application.dataPath + "/Notecharts/" + song + "_" + difficulty + ".json";
        json = File.ReadAllText(jsonPath);
        noteChart = JsonHelper.FromJson<NoteSpawn>(json);

        // play song
        audioManager = FindObjectOfType<AudioManager>();
        audioManager.Play(song);

        // initialize time syncing variables
        speedOffset = (Constants.spawnZ - Constants.activatorZ) / noteSpeed;
        dspStart = (float)AudioSettings.dspTime;
    }

    // Update is called once per frame
    void Update()
    {
        songTimer = (float)(AudioSettings.dspTime - dspStart);
        
        if (chartIndex >= noteChart.Length)
        {
            enabled = false;
            StartCoroutine(EndGame());
            return;
        }

        int lane1 = 0;
        float length1 = 0; 
        bool tail1 = false;

        if ((noteChart[chartIndex].headHitTime - speedOffset) <= songTimer && noteChart[chartIndex].headHitTime > 0)
        {
            lane1 = noteChart[chartIndex].laneIndex;

            if (noteChart[chartIndex].tailHitTime > 0)
            {
                length1 = noteSpeed * (noteChart[chartIndex].tailHitTime - noteChart[chartIndex].headHitTime);
            }
            chartIndex++;
        }
        else if ((noteChart[chartIndex].tailHitTime - speedOffset) <= songTimer && noteChart[chartIndex].headHitTime == 0)
        {
            lane1 = noteChart[chartIndex].laneIndex;
            tail1 = true;
            chartIndex++;
        }
        else
        {
            return;
        }

        if (chartIndex >= noteChart.Length)
        {
            InstantiateNotes(lane1, 0, length1, 0, tail1, false);
            enabled = false;
            StartCoroutine(EndGame());
            return;
        }

        int lane2 = 0;
        float length2 = 0;
        bool tail2 = false;

        if ((noteChart[chartIndex].headHitTime - speedOffset) <= songTimer && noteChart[chartIndex].headHitTime > 0)
        {
            lane2 = noteChart[chartIndex].laneIndex;

            if (noteChart[chartIndex].tailHitTime > 0)
            {
                length2 = noteSpeed * (noteChart[chartIndex].tailHitTime - noteChart[chartIndex].headHitTime);
            }
            chartIndex++;
        }
        else if ((noteChart[chartIndex].tailHitTime - speedOffset) <= songTimer && noteChart[chartIndex].headHitTime == 0)
        {
            lane2 = noteChart[chartIndex].laneIndex;
            tail2 = true;
            chartIndex++;
        }

        InstantiateNotes(lane1, lane2, length1, length2, tail1, tail2);
    }

    void InstantiateNotes(int lane1, int lane2, float length1, float length2, bool tail1, bool tail2)
    {
        if (lane1 == 0)
        {
            return;
        }

        if (lane2 == 0)
        {
            if (length1 == 0)
            {
                if (!tail1)
                {
                    InstantiateNote(lane1);
                }
                else
                {
                    InstantiateTail(lane1);
                }
            }
            else
            {
                InstantiateHold(lane1, length1);
            }
        }

        else
        {
            if (length1 == 0 && length2 == 0)
            {
                if(tail1 && tail2)
                {
                    InstantiateDoubleTail(lane1, lane2);
                }
                else if (tail1)
                {
                    InstantiateOneNoteOneTail(lane2, lane1);
                }
                else if (tail2)
                {
                    InstantiateOneNoteOneTail(lane1, lane2);
                }
                else
                {
                    InstantiateDoubleNote(lane1, lane2);
                }
            }
            else if (length1 == 0)
            {
                if (tail1)
                {
                    InstantiateOneHoldOneTail(lane2, lane1, length2);
                }
                else
                {
                    InstantiateOneNoteOneHold(lane1, lane2, length2);
                }
            }
            else if (length2 == 0)
            {
                if (tail2)
                {
                    InstantiateOneHoldOneTail(lane1, lane2, length1);
                }
                else
                {
                    InstantiateOneNoteOneHold(lane2, lane1, length1);
                }
            }
            else
            {
                InstantiateDoubleHold(lane1, lane2, length1, length2);
            }
        }
    }

    void InstantiateNote(int laneIndex)
    {
        float xPosition;
        switch (laneIndex)
        {
            case 1:
                xPosition = Constants.lane1X;
                break;
            case 2:
                xPosition = Constants.lane2X;
                break;
            case 3:
                xPosition = Constants.lane3X;
                break;
            case 4:
                xPosition = Constants.lane4X;
                break;
            case 5:
                xPosition = Constants.lane5X;
                break;
            default:
                return;
        }

        Instantiate(noteObject, new Vector3(xPosition, noteObject.localPosition.y, Constants.spawnZ), noteObject.rotation);
    }

    void InstantiateTail(int laneIndex)
    {
        float xPosition;
        switch (laneIndex)
        {
            case 1:
                xPosition = Constants.lane1X;
                break;
            case 2:
                xPosition = Constants.lane2X;
                break;
            case 3:
                xPosition = Constants.lane3X;
                break;
            case 4:
                xPosition = Constants.lane4X;
                break;
            case 5:
                xPosition = Constants.lane5X;
                break;
            default:
                return;
        }

        Transform tail = Instantiate(tailNoteObject, new Vector3(xPosition, tailNoteObject.localPosition.y, Constants.spawnZ), tailNoteObject.rotation);
        
        switch (laneIndex)
        {
            case 1:
                holdNoteInstance1.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 2:
                holdNoteInstance2.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 3:
                holdNoteInstance3.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 4:
                holdNoteInstance4.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 5:
                holdNoteInstance5.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            default:
                return;
        }
    }

    void InstantiateHold(int laneIndex, float length)
    {
        float xPosition;
        switch (laneIndex)
        {
            case 1:
                xPosition = Constants.lane1X;
                break;
            case 2:
                xPosition = Constants.lane2X;
                break;
            case 3:
                xPosition = Constants.lane3X;
                break;
            case 4:
                xPosition = Constants.lane4X;
                break;
            case 5:
                xPosition = Constants.lane5X;
                break;
            default:
                return;
        }

        Transform holdNote = Instantiate(holdNoteObject, new Vector3(xPosition, holdNoteObject.localPosition.y, Constants.spawnZ), holdNoteObject.rotation);
        Transform holdLane = holdNote.Find("HoldLane");
        holdLane.localPosition = new Vector3(holdLane.localPosition.x, holdLane.localPosition.y, length / 2.0f);
        holdLane.localScale = new Vector3(holdLane.localScale.x, holdLane.localScale.y, length);

        switch (laneIndex)
        { 
            case 1:
                holdNoteInstance1 = holdNote;
                break;
            case 2:
                holdNoteInstance2 = holdNote;
                break;
            case 3:
                holdNoteInstance3 = holdNote;
                break;
            case 4:
                holdNoteInstance4 = holdNote;
                break;
            case 5:
                holdNoteInstance5 = holdNote;
                break;
            default:
                return;
        }
    }

    void InstantiateDoubleNote(int lane1, int lane2)
    {
        float xPosition1;
        switch (lane1)
        {
            case 1:
                xPosition1 = Constants.lane1X;
                break;
            case 2:
                xPosition1 = Constants.lane2X;
                break;
            case 3:
                xPosition1 = Constants.lane3X;
                break;
            case 4:
                xPosition1 = Constants.lane4X;
                break;
            case 5:
                xPosition1 = Constants.lane5X;
                break;
            default:
                xPosition1 = 0;
                break;
        }

        float xPosition2;
        switch (lane2)
        {
            case 1:
                xPosition2 = Constants.lane1X;
                break;
            case 2:
                xPosition2 = Constants.lane2X;
                break;
            case 3:
                xPosition2 = Constants.lane3X;
                break;
            case 4:
                xPosition2 = Constants.lane4X;
                break;
            case 5:
                xPosition2 = Constants.lane5X;
                break;
            default:
                xPosition2 = 0;
                break;
        }

        Transform doubleNote = Instantiate(doubleNoteObject, new Vector3(doubleNoteObject.localPosition.x, doubleNoteObject.localPosition.y, Constants.spawnZ), doubleNoteObject.rotation);

        Transform note1 = doubleNote.Find("Note1");
        note1.localPosition = new Vector3(xPosition1, note1.localPosition.y, note1.localPosition.z);

        Transform note2 = doubleNote.Find("Note2");
        note2.localPosition = new Vector3(xPosition2, note1.localPosition.y, note1.localPosition.z);

        Transform tether = doubleNote.Find("Tether");
        tether.localPosition = new Vector3((xPosition2 + xPosition1) / 2.0f, tether.localPosition.y, note1.localPosition.z);
        tether.localScale = new Vector3(Mathf.Abs(xPosition2 - xPosition1), tether.localScale.y, tether.localScale.z);
    }

    void InstantiateDoubleTail(int lane1, int lane2)
    {
        float xPosition1;
        switch (lane1)
        {
            case 1:
                xPosition1 = Constants.lane1X;
                break;
            case 2:
                xPosition1 = Constants.lane2X;
                break;
            case 3:
                xPosition1 = Constants.lane3X;
                break;
            case 4:
                xPosition1 = Constants.lane4X;
                break;
            case 5:
                xPosition1 = Constants.lane5X;
                break;
            default:
                xPosition1 = 0;
                break;
        }

        float xPosition2;
        switch (lane2)
        {
            case 1:
                xPosition2 = Constants.lane1X;
                break;
            case 2:
                xPosition2 = Constants.lane2X;
                break;
            case 3:
                xPosition2 = Constants.lane3X;
                break;
            case 4:
                xPosition2 = Constants.lane4X;
                break;
            case 5:
                xPosition2 = Constants.lane5X;
                break;
            default:
                xPosition2 = 0;
                break;
        }

        Transform doubleTail = Instantiate(doubleTailObject, new Vector3(doubleTailObject.localPosition.x, doubleTailObject.localPosition.y, Constants.spawnZ), doubleTailObject.rotation);

        Transform tail1 = doubleTail.Find("Tail1");
        tail1.localPosition = new Vector3(xPosition1, tail1.localPosition.y, tail1.localPosition.z);

        Transform tail2 = doubleTail.Find("Tail2");
        tail2.localPosition = new Vector3(xPosition2, tail1.localPosition.y, tail1.localPosition.z);

        Transform tether = doubleTail.Find("Tether");
        tether.localPosition = new Vector3((xPosition2 + xPosition1) / 2.0f, tether.localPosition.y, tail1.localPosition.z);
        tether.localScale = new Vector3(Mathf.Abs(xPosition2 - xPosition1), tether.localScale.y, tether.localScale.z);

        switch (lane1)
        {
            case 1:
                holdNoteInstance1.GetComponent<HoldNote>().SetTail(tail1.gameObject);
                break;
            case 2:
                holdNoteInstance2.GetComponent<HoldNote>().SetTail(tail1.gameObject);
                break;
            case 3:
                holdNoteInstance3.GetComponent<HoldNote>().SetTail(tail1.gameObject);
                break;
            case 4:
                holdNoteInstance4.GetComponent<HoldNote>().SetTail(tail1.gameObject);
                break;
            case 5:
                holdNoteInstance5.GetComponent<HoldNote>().SetTail(tail1.gameObject);
                break;
            default:
                return;
        }

        switch (lane2)
        {
            case 1:
                holdNoteInstance1.GetComponent<HoldNote>().SetTail(tail2.gameObject);
                break;
            case 2:
                holdNoteInstance2.GetComponent<HoldNote>().SetTail(tail2.gameObject);
                break;
            case 3:
                holdNoteInstance3.GetComponent<HoldNote>().SetTail(tail2.gameObject);
                break;
            case 4:
                holdNoteInstance4.GetComponent<HoldNote>().SetTail(tail2.gameObject);
                break;
            case 5:
                holdNoteInstance5.GetComponent<HoldNote>().SetTail(tail2.gameObject);
                break;
            default:
                return;
        }
    }

    void InstantiateDoubleHold(int lane1, int lane2, float length1, float length2)
    {
        float xPosition1;
        switch (lane1)
        {
            case 1:
                xPosition1 = Constants.lane1X;
                break;
            case 2:
                xPosition1 = Constants.lane2X;
                break;
            case 3:
                xPosition1 = Constants.lane3X;
                break;
            case 4:
                xPosition1 = Constants.lane4X;
                break;
            case 5:
                xPosition1 = Constants.lane5X;
                break;
            default:
                xPosition1 = 0;
                break;
        }

        float xPosition2;
        switch (lane2)
        {
            case 1:
                xPosition2 = Constants.lane1X;
                break;
            case 2:
                xPosition2 = Constants.lane2X;
                break;
            case 3:
                xPosition2 = Constants.lane3X;
                break;
            case 4:
                xPosition2 = Constants.lane4X;
                break;
            case 5:
                xPosition2 = Constants.lane5X;
                break;
            default:
                xPosition2 = 0;
                break;
        }

        Transform doubleHead = Instantiate(doubleHoldObject, new Vector3(doubleHoldObject.localPosition.x, doubleHoldObject.localPosition.y, Constants.spawnZ), doubleHoldObject.rotation);

        Transform holdNote1 = doubleHead.Find("HoldNote1");
        holdNote1.localPosition = new Vector3(xPosition1, holdNote1.localPosition.y, holdNote1.localPosition.z);

        Transform holdNote2 = doubleHead.Find("HoldNote2");
        holdNote2.localPosition = new Vector3(xPosition2, holdNote1.localPosition.y, holdNote1.localPosition.z);

        Transform holdLane1 = holdNote1.Find("HoldLane");
        holdLane1.localPosition = new Vector3(holdLane1.localPosition.x, holdLane1.localPosition.y, length1 / 2.0f);
        holdLane1.localScale = new Vector3(holdLane1.localScale.x, holdLane1.localScale.y, length1);

        Transform holdLane2 = holdNote2.Find("HoldLane");
        holdLane2.localPosition = new Vector3(holdLane2.localPosition.x, holdLane2.localPosition.y, length2 / 2.0f);
        holdLane2.localScale = new Vector3(holdLane2.localScale.x, holdLane2.localScale.y, length2);

        Transform tether = doubleHead.Find("Tether");
        tether.localPosition = new Vector3((xPosition2 + xPosition1) / 2.0f, tether.localPosition.y, holdNote1.localPosition.z);
        tether.localScale = new Vector3(Mathf.Abs(xPosition2 - xPosition1), tether.localScale.y, tether.localScale.z);

        switch (lane1)
        {
            case 1:
                holdNoteInstance1 = holdNote1;
                break;
            case 2:
                holdNoteInstance2 = holdNote1;
                break;
            case 3:
                holdNoteInstance3 = holdNote1;
                break;
            case 4:
                holdNoteInstance4 = holdNote1;
                break;
            case 5:
                holdNoteInstance5 = holdNote1;
                break;
            default:
                return;
        }

        switch (lane2)
        {
            case 1:
                holdNoteInstance1 = holdNote2;
                break;
            case 2:
                holdNoteInstance2 = holdNote2;
                break;
            case 3:
                holdNoteInstance3 = holdNote2;
                break;
            case 4:
                holdNoteInstance4 = holdNote2;
                break;
            case 5:
                holdNoteInstance5 = holdNote2;
                break;
            default:
                return;
        }
    }

    void InstantiateOneNoteOneTail(int noteLane, int tailLane)
    {
        float noteXPosition;
        switch (noteLane)
        {
            case 1:
                noteXPosition = Constants.lane1X;
                break;
            case 2:
                noteXPosition = Constants.lane2X;
                break;
            case 3:
                noteXPosition = Constants.lane3X;
                break;
            case 4:
                noteXPosition = Constants.lane4X;
                break;
            case 5:
                noteXPosition = Constants.lane5X;
                break;
            default:
                noteXPosition = 0;
                break;
        }

        float tailXPosition;
        switch (tailLane)
        {
            case 1:
                tailXPosition = Constants.lane1X;
                break;
            case 2:
                tailXPosition = Constants.lane2X;
                break;
            case 3:
                tailXPosition = Constants.lane3X;
                break;
            case 4:
                tailXPosition = Constants.lane4X;
                break;
            case 5:
                tailXPosition = Constants.lane5X;
                break;
            default:
                tailXPosition = 0;
                break;
        }

        Transform oneNoteOneTail = Instantiate(oneNoteOneTailObject, new Vector3(oneNoteOneTailObject.localPosition.x, oneNoteOneTailObject.localPosition.y, Constants.spawnZ), oneNoteOneTailObject.rotation);

        Transform note = oneNoteOneTail.Find("Note");
        note.localPosition = new Vector3(noteXPosition, note.localPosition.y, note.localPosition.z);

        Transform tail = oneNoteOneTail.Find("Tail");
        tail.localPosition = new Vector3(tailXPosition, note.localPosition.y, note.localPosition.z);

        Transform tether = oneNoteOneTail.Find("Tether");
        tether.localPosition = new Vector3((tailXPosition + noteXPosition) / 2.0f, tether.localPosition.y, note.localPosition.z);
        tether.localScale = new Vector3(Mathf.Abs(tailXPosition - noteXPosition), tether.localScale.y, tether.localScale.z);

        switch (tailLane)
        {
            case 1:
                holdNoteInstance1.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 2:
                holdNoteInstance2.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 3:
                holdNoteInstance3.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 4:
                holdNoteInstance4.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 5:
                holdNoteInstance5.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            default:
                return;
        }
    }

    void InstantiateOneNoteOneHold(int noteLane, int holdNoteLane, float length)
    {
        float noteXPosition;
        switch (noteLane)
        {
            case 1:
                noteXPosition = Constants.lane1X;
                break;
            case 2:
                noteXPosition = Constants.lane2X;
                break;
            case 3:
                noteXPosition = Constants.lane3X;
                break;
            case 4:
                noteXPosition = Constants.lane4X;
                break;
            case 5:
                noteXPosition = Constants.lane5X;
                break;
            default:
                noteXPosition = 0;
                break;
        }

        float holdXPosition;
        switch (holdNoteLane)
        {
            case 1:
                holdXPosition = Constants.lane1X;
                break;
            case 2:
                holdXPosition = Constants.lane2X;
                break;
            case 3:
                holdXPosition = Constants.lane3X;
                break;
            case 4:
                holdXPosition = Constants.lane4X;
                break;
            case 5:
                holdXPosition = Constants.lane5X;
                break;
            default:
                holdXPosition = 0;
                break;
        }

        Transform oneNoteOneHold = Instantiate(oneNoteOneHoldObject, new Vector3(oneNoteOneHoldObject.localPosition.x, oneNoteOneHoldObject.localPosition.y, Constants.spawnZ), oneNoteOneHoldObject.rotation);

        Transform note = oneNoteOneHold.Find("Note");
        note.localPosition = new Vector3(noteXPosition, note.localPosition.y, note.localPosition.z);

        Transform holdNote = oneNoteOneHold.Find("HoldNote");
        holdNote.localPosition = new Vector3(holdXPosition, note.localPosition.y, note.localPosition.z);

        Transform holdLane = holdNote.Find("HoldLane");
        holdLane.localPosition = new Vector3(holdLane.localPosition.x, holdLane.localPosition.y, length / 2.0f);
        holdLane.localScale = new Vector3(holdLane.localScale.x, holdLane.localScale.y, length);

        Transform tether = oneNoteOneHold.Find("Tether");
        tether.localPosition = new Vector3((holdXPosition + noteXPosition) / 2.0f, tether.localPosition.y, note.localPosition.z);
        tether.localScale = new Vector3(Mathf.Abs(holdXPosition - noteXPosition), tether.localScale.y, tether.localScale.z);

        switch (holdNoteLane)
        {
            case 1:
                holdNoteInstance1 = holdNote;
                break;
            case 2:
                holdNoteInstance2 = holdNote;
                break;
            case 3:
                holdNoteInstance3 = holdNote;
                break;
            case 4:
                holdNoteInstance4 = holdNote;
                break;
            case 5:
                holdNoteInstance5 = holdNote;
                break;
            default:
                return;
        }
    }

    void InstantiateOneHoldOneTail(int holdNoteLane, int tailLane, float length)
    {
        float holdXPosition;
        switch (holdNoteLane)
        {
            case 1:
                holdXPosition = Constants.lane1X;
                break;
            case 2:
                holdXPosition = Constants.lane2X;
                break;
            case 3:
                holdXPosition = Constants.lane3X;
                break;
            case 4:
                holdXPosition = Constants.lane4X;
                break;
            case 5:
                holdXPosition = Constants.lane5X;
                break;
            default:
                holdXPosition = 0;
                break;
        }

        float tailXPosition;
        switch (tailLane)
        {
            case 1:
                tailXPosition = Constants.lane1X;
                break;
            case 2:
                tailXPosition = Constants.lane2X;
                break;
            case 3:
                tailXPosition = Constants.lane3X;
                break;
            case 4:
                tailXPosition = Constants.lane4X;
                break;
            case 5:
                tailXPosition = Constants.lane5X;
                break;
            default:
                tailXPosition = 0;
                break;
        }

        Transform oneHoldOneTail = Instantiate(oneHoldOneTailObject, new Vector3(oneHoldOneTailObject.localPosition.x, oneHoldOneTailObject.localPosition.y, Constants.spawnZ), oneHoldOneTailObject.rotation);

        Transform tail = oneHoldOneTail.Find("Tail");
        tail.localPosition = new Vector3(tailXPosition, tail.localPosition.y, tail.localPosition.z);

        Transform holdNote = oneHoldOneTail.Find("HoldNote");
        holdNote.localPosition = new Vector3(holdXPosition, tail.localPosition.y, tail.localPosition.z);

        Transform holdLane = holdNote.Find("HoldLane");
        holdLane.localPosition = new Vector3(holdLane.localPosition.x, holdLane.localPosition.y, length / 2.0f);
        holdLane.localScale = new Vector3(holdLane.localScale.x, holdLane.localScale.y, length);

        Transform tether = oneHoldOneTail.Find("Tether");
        tether.localPosition = new Vector3((tailXPosition + holdXPosition) / 2.0f, tether.localPosition.y, tail.localPosition.z);
        tether.localScale = new Vector3(Mathf.Abs(tailXPosition - holdXPosition), tether.localScale.y, tether.localScale.z);

        switch (holdNoteLane)
        {
            case 1:
                holdNoteInstance1 = holdNote;
                break;
            case 2:
                holdNoteInstance2 = holdNote;
                break;
            case 3:
                holdNoteInstance3 = holdNote;
                break;
            case 4:
                holdNoteInstance4 = holdNote;
                break;
            case 5:
                holdNoteInstance5 = holdNote;
                break;
            default:
                return;
        }

        switch (tailLane)
        {
            case 1:
                holdNoteInstance1.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 2:
                holdNoteInstance2.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 3:
                holdNoteInstance3.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 4:
                holdNoteInstance4.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            case 5:
                holdNoteInstance5.GetComponent<HoldNote>().SetTail(tail.gameObject);
                break;
            default:
                return;
        }
    }

    IEnumerator EnablePauseButton()
    {
        yield return new WaitForSeconds(8.0f);
        pauseButton.SetActive(true);
    }

    void InitSongInfo()
    {
        difficultyText.text = difficulty.ToUpper();
        songName.text = PlayerPrefs.GetString(Constants.selectedSongTitle);
        StartCoroutine(EnablePauseButton());
    }

    void InitPlayerPrefs()
    {
        // initialize player prefs
        noteSpeed = PlayerPrefs.GetFloat(Constants.noteSpeed);
        song = PlayerPrefs.GetString(Constants.selectedSong);
        difficulty = PlayerPrefs.GetString(Constants.difficulty);
        noteCount = PlayerPrefs.GetInt(song + difficulty + Constants.noteCount);

        PlayerPrefs.SetInt(Constants.score, 0);
        PlayerPrefs.SetInt(Constants.combo, 0);
        PlayerPrefs.SetInt(song + difficulty + Constants.maxCombo, 0);
        PlayerPrefs.SetInt(Constants.perfects, 0);
        PlayerPrefs.SetInt(Constants.greats, 0);
        PlayerPrefs.SetInt(Constants.goods, 0);
        PlayerPrefs.SetInt(Constants.bads, 0);
        PlayerPrefs.SetInt(Constants.misses, 0);
        PlayerPrefs.SetInt(Constants.notesHit, 0);
    }

    void SetScoreRank()
    {
        float rank = (float)PlayerPrefs.GetInt(Constants.score) / (float)(noteCount * Constants.perfectScore);

        if (PlayerPrefs.GetInt(Constants.perfects) == noteCount)
        {
            PlayerPrefs.SetString(Constants.scoreRank, "SS");
        }
        else if (PlayerPrefs.GetInt(Constants.perfects) + PlayerPrefs.GetInt(Constants.greats) == noteCount)
        {
            PlayerPrefs.SetString(Constants.scoreRank, "S");
        }
        else if (rank >= Constants.rankA)
        {
            PlayerPrefs.SetString(Constants.scoreRank, "A");
        }
        else if (rank >= Constants.rankB)
        {
            PlayerPrefs.SetString(Constants.scoreRank, "B");
        }
        else if (rank >= Constants.rankC)
        {
            PlayerPrefs.SetString(Constants.scoreRank, "C");
        }
        else
        {
            PlayerPrefs.SetString(Constants.scoreRank, "F");
        }

        SetHighRank();
    }

    void SetHighRank()
    {
        int scoreRank = 0;
        int highRank = 0;

        switch (PlayerPrefs.GetString(Constants.scoreRank))
        {
            case "SS":
                scoreRank = 6;
                break;
            case "S":
                scoreRank = 5;
                break;
            case "A":
                scoreRank = 4;
                break;
            case "B":
                scoreRank = 3;
                break;
            case "C":
                scoreRank = 2;
                break;
            case "F":
                scoreRank = 1;
                break;
            default:
                break;
        }

        switch (PlayerPrefs.GetString(song + difficulty + Constants.highRank))
        {
            case "SS":
                highRank = 6;
                break;
            case "S":
                highRank = 5;
                break;
            case "A":
                highRank = 4;
                break;
            case "B":
                highRank = 3;
                break;
            case "C":
                highRank = 2;
                break;
            case "F":
                scoreRank = 1;
                break;
            default:
                break;
        }

        if (scoreRank > highRank)
        {
            PlayerPrefs.SetString(song + difficulty + Constants.highRank, PlayerPrefs.GetString(Constants.scoreRank));
        }
    }

    IEnumerator EndGame()
    {
        pauseButton.SetActive(false);
        yield return new WaitForSeconds(speedOffset + 3.0f);
        StartCoroutine(SetOffFireworks(10));
        yield return new WaitForSeconds(3.0f);
        audioManager.Stop(song);
        SetScoreRank();
        SceneManager.LoadScene(Constants.scoreScreen);
    }

    IEnumerator SetOffFireworks(int numOfParticles)
    {
        notePlatform.SetActive(false);
        GameObject fireworks = Instantiate(fireworksParticle, new Vector3(fireworksParticle.transform.localPosition.x, fireworksParticle.transform.localPosition.y, fireworksParticle.transform.localPosition.z), fireworksParticle.transform.rotation);
        fireworks.transform.localScale = new Vector3(fireworksParticle.transform.localScale.x * 2, fireworksParticle.transform.localScale.x * 2, fireworksParticle.transform.localScale.x * 2);

        GameObject text = Instantiate(fireworksText, gameCanvas.position, Quaternion.identity, gameCanvas);
        if (PlayerPrefs.GetInt(Constants.perfects) + PlayerPrefs.GetInt(Constants.greats) == noteCount)
        {
            text.GetComponent<TextMeshProUGUI>().text = "FULL COMBO";
        }

        float duration = fireworksParticle.GetComponent<ParticleSystem>().main.duration / 4.0f;
        audioManager.Play(Constants.fireworks);

        for (int i = 0; i < numOfParticles; i++)
        {
            Instantiate(fireworksParticle, new Vector3(Random.Range(-2f, 2f), Random.Range(-1f, 0.5f), fireworksParticle.transform.localPosition.z), fireworksParticle.transform.rotation);
            yield return new WaitForSeconds(duration);
        }
    }

    void WriteJson()
    {
        string jsonChart = JsonHelper.ToJson<NoteSpawn>(noteChart, true);
        File.WriteAllText(jsonPath, jsonChart);
        Debug.Log("done");
    }

    float RoundToHundreth(float number)
    {
        number *= 100;
        number = Mathf.Round(number);
        return number / 100;
    }

    void OffsetChart(float offset)
    {
        if (chartIndex >= noteChart.Length)
        {
            WriteJson();
            enabled = false;
            return;
        }

        if (noteChart[chartIndex].headHitTime > 0)
        {
            noteChart[chartIndex].headHitTime = noteChart[chartIndex].headHitTime + offset;
            noteChart[chartIndex].headHitTime = RoundToHundreth(noteChart[chartIndex].headHitTime);
        }
        if (noteChart[chartIndex].tailHitTime > 0)
        {
            noteChart[chartIndex].tailHitTime = noteChart[chartIndex].tailHitTime + offset;
            noteChart[chartIndex].tailHitTime = RoundToHundreth(noteChart[chartIndex].tailHitTime);
        }
        chartIndex++;
    }
}
