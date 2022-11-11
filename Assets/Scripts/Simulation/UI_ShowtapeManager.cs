using System.Collections.Generic;
using UnityEngine;
using SFB;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class FloatEvent : UnityEvent<float> { }

public class UI_ShowtapeManager : MonoBehaviour
{
    //Inspector Objects
    [Header("Inspector Objects")]
    public Mack_Valves mack;
    public InputHandler inputHandler;
    [Space(20)]

    //File Show MetaData
    [Header("File Show Metadata")]
    [HideInInspector] public BitArray[] rshwData;
    [HideInInspector] public UI_RshwCreator creator;
    public AudioSource referenceSpeaker;
    [HideInInspector] public float refSpeakerVol;
    public VideoPlayer referenceVideo;
    public AudioClip speakerClip;
    public LoopVers songLoopSetting;
    public string wavPath;
    public string videoPath;
    public string fileExtention = "rshw";
    public string[] showtapeSegmentPaths = new string[1];
    public int currentShowtapeSegment = -1;
    public float dataStreamedFPS = 60;
    [Space(20)]

    //Events
    public UnityEvent audioVideoPlay;
    public UnityEvent audioVideoPause;
    public UnityEvent audioVideoGetData;
    public UnityEvent newDataRecorded;
    public UnityEvent curtainClose;
    public UnityEvent curtainOpen;
    public UnityEvent syncTvsAndSpeakers;
    public UnityEvent updateTickets;

    //Extra Variables
    bool ticketCheck;
    bool ticketCheck2;
    [HideInInspector] public bool disableCharactersOnStart = true;

    //Sim States
    public bool recordMovements = false;
    public bool playMovements = false;

    public bool useVideoAsReference;

    //New Simulation
    [HideInInspector] public float timeSongStarted = 0;
    [HideInInspector] public float timeSongOffset = 0;
    [HideInInspector] public float timePauseStart = 0;
    [HideInInspector] public float timeInputSpeedStart = 0;
    [HideInInspector] public int previousFramePosition = 0;
    [HideInInspector] public bool previousAnyButtonHeld = false;

    //Sync TV for Large Shows (Unity's Fault)
    float syncTimer;

    public enum LoopVers
    {
        noLoop,
        loopPlaylist,
        loopSong
    }

    void Start()
    {
        creator = this.GetComponent<UI_RshwCreator>();
        referenceVideo = this.GetComponent<VideoPlayer>();
        refSpeakerVol = referenceSpeaker.volume;
    }

    void Update()
    {

        //Big Show Sync
        syncTimer += Time.deltaTime;
        if (syncTimer >= 30)
        {
            syncTimer = 0;
            syncTvsAndSpeakers.Invoke();
        }

        if (inputHandler != null)
        {
            InputDataObj inputDataObj = inputHandler.InputCheck();

            //Clear Drawers
            mack.topDrawer = inputDataObj.topDrawer;
            mack.bottomDrawer = inputDataObj.bottomDrawer;

            //Check for inputs and send to mack valves
            if (inputHandler != null && mack != null)
            {
                if ((useVideoAsReference) || (!useVideoAsReference && referenceSpeaker.clip != null))
                {
                    if (rshwData != null)
                    {
                        //Show Code
                        //Being paused means the same frame of data will loop
                        //Being unpaused means deciding where to start next sim frame
                        int arrayDestination;
                        if (useVideoAsReference)
                        {
                            arrayDestination = (int)(referenceVideo.time * dataStreamedFPS);
                        }
                        else
                        {
                            arrayDestination = (int)(referenceSpeaker.time * dataStreamedFPS);
                        }


                        //Check if new frames need to be created
                        if (arrayDestination >= rshwData.Length && rshwData.Length != 0)
                        {
                            if (recordMovements)
                            {
                                while (arrayDestination + 1 > rshwData.Length)
                                {
                                    rshwData = rshwData.Append(new BitArray(300)).ToArray();
                                }
                            }
                            else
                            {
                                arrayDestination = rshwData.Length;
                            }
                        }

                        //Record
                        if (recordMovements)
                        {
                            //Record
                            if (inputDataObj.anyButtonHeld)
                            {
                                for (int i = 0; i < 150; i++)
                                {
                                    if (inputDataObj.topDrawer[i])
                                    {
                                        rshwData[arrayDestination].Set(i, true);
                                    }
                                    if (inputDataObj.bottomDrawer[i])
                                    {
                                        rshwData[arrayDestination].Set(i + 150, true);
                                    }
                                }
                                if (previousAnyButtonHeld)
                                {
                                    //Record forward or backward
                                    if (previousFramePosition <= arrayDestination)
                                    {
                                        //Forward
                                        for (int i = 0; i < arrayDestination - previousFramePosition; i++)
                                        {
                                            for (int e = 0; e < 150; e++)
                                            {
                                                if (inputDataObj.topDrawer[e])
                                                {
                                                    rshwData[previousFramePosition + i].Set(e, true);
                                                }
                                                if (inputDataObj.bottomDrawer[e])
                                                {
                                                    rshwData[previousFramePosition + i].Set(e + 150, true);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Backward
                                        for (int i = 0; i < previousFramePosition - arrayDestination; i++)
                                        {
                                            for (int e = 0; e < 150; e++)
                                            {
                                                if (inputDataObj.topDrawer[e])
                                                {
                                                    rshwData[previousFramePosition - i].Set(e, true);
                                                }
                                                if (inputDataObj.bottomDrawer[e])
                                                {
                                                    rshwData[previousFramePosition - i].Set(e + 150, true);
                                                }
                                            }
                                        }
                                    }
                                }
                                newDataRecorded.Invoke();
                            }



                            //Ticket Earning Code
                            if (Input.anyKey)
                            {
                                ticketCheck = true;
                            }
                            if (!useVideoAsReference)
                            {
                                if (referenceSpeaker.time % 5 < 1 && !ticketCheck2)
                                {
                                    ticketCheck2 = true;
                                    if (Input.anyKey)
                                    {
                                        ticketCheck = false;
                                    }
                                    if (ticketCheck)
                                    {
                                        PlayerPrefs.SetInt("TicketCount", PlayerPrefs.GetInt("TicketCount") + 1);
                                        updateTickets.Invoke();
                                    }
                                    ticketCheck = false;
                                }
                                if (referenceSpeaker.time % 5 >= 1)
                                {
                                    ticketCheck2 = false;
                                }
                            }
                        }


                        //Apply the current frame of simulation data to the Mack Valves
                        if (arrayDestination < rshwData.Length)
                        {
                            for (int i = 0; i < 150; i++)
                            {
                                if (rshwData[arrayDestination].Get(i))
                                {
                                    mack.topDrawer[i] = true;
                                }
                                if (rshwData[arrayDestination].Get(i + 150))
                                {
                                    mack.bottomDrawer[i] = true;
                                }
                            }
                        }

                        //Check if show is over
                        if ((!useVideoAsReference && referenceSpeaker.time >= (referenceSpeaker.clip.length / referenceSpeaker.clip.channels))
                        || (useVideoAsReference && referenceVideo.time >= (referenceVideo.length / referenceVideo.GetAudioChannelCount(0))))
                        {

                            if (!recordMovements)
                            {
                                Debug.Log("Song is over. Queuing next song / stopping.");
                                Play(true, false);
                                if (songLoopSetting == LoopVers.loopSong)
                                {
                                    if (currentShowtapeSegment == -1)
                                    {
                                        referenceSpeaker.time = 0;
                                        referenceVideo.time = 0;
                                    }
                                    else
                                    {
                                        creator.LoadFromURL(showtapeSegmentPaths[currentShowtapeSegment]);
                                    }
                                }
                                else
                                {
                                    //Check if multi showtape or single
                                    if (currentShowtapeSegment == -1)
                                    {
                                        referenceSpeaker.time = 0;
                                        referenceVideo.time = 0;
                                        Unload();
                                    }
                                    else
                                    {
                                        currentShowtapeSegment++;
                                        //Check if end of multi showtape or not
                                        if (currentShowtapeSegment >= showtapeSegmentPaths.Length)
                                        {
                                            if (songLoopSetting == LoopVers.loopPlaylist)
                                            {
                                                currentShowtapeSegment = 0;
                                                creator.LoadFromURL(showtapeSegmentPaths[currentShowtapeSegment]);
                                            }
                                            else
                                            {
                                                Unload();
                                            }
                                        }
                                        else
                                        {
                                            creator.LoadFromURL(showtapeSegmentPaths[currentShowtapeSegment]);
                                        }
                                    }
                                }
                            }
                        }
                        previousFramePosition = arrayDestination;
                        previousAnyButtonHeld = inputDataObj.anyButtonHeld;
                    }
                }
            }
        }
    }

    public void Load()
    {
        CursorLockMode lockState = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("Load");
        if (referenceSpeaker != null)
        {
            referenceSpeaker.time = 0;
        }
        if (referenceVideo != null)
        {
            referenceVideo.time = 0;
        }

        //Call File Browser
        showtapeSegmentPaths = new string[1];
        string[] paths;
        if (fileExtention == "")
        {
            ExtensionFilter[] extensions;
            if (GameVersion.gameName == "Faz-Anim")
            {
                extensions = new ExtensionFilter[] { new ExtensionFilter("Show Files", "fshw", "tshw", "mshw") };
            }
            else
            {
                extensions = new ExtensionFilter[] { new ExtensionFilter("Show Files", "cshw", "sshw", "rshw", "nshw") };
            }

            paths = StandaloneFileBrowser.OpenFilePanel("Browse Showtape Audio", "", extensions, false);
        }
        else
        {
            paths = StandaloneFileBrowser.OpenFilePanel("Browse Showtape Audio", "", fileExtention, false);
        }
        if (paths.Length > 0)
        {
            showtapeSegmentPaths[0] = paths[0];
            currentShowtapeSegment = 0;
            creator.LoadFromURL(paths[0]);
        }
        Cursor.lockState = lockState;
    }

    public void LoadFolder()
    {
        CursorLockMode lockState = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        referenceSpeaker.time = 0;
        referenceVideo.time = 0;
        //Call File Browser
        var paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder of Showtapes", "", false);
        if (paths.Length > 0)
        {
            showtapeSegmentPaths = Directory.GetFiles(paths[0], "*." + fileExtention);
            currentShowtapeSegment = 0;
            creator.LoadFromURL(showtapeSegmentPaths[currentShowtapeSegment]);
        }
        Cursor.lockState = lockState;
    }

    public void Play(bool force, bool onOff)
    {
        if (force)
        {
            playMovements = onOff;
        }
        else
        {
            playMovements = !playMovements;
        }
        syncTvsAndSpeakers.Invoke();
        if (playMovements)
        {
            timeSongOffset += Time.time - timePauseStart;
            timePauseStart = 0;
            audioVideoPlay.Invoke();
        }
        else
        {
            timePauseStart = Time.time;
            audioVideoPause.Invoke();
        }
    }

    public void SwapLoop()
    {
        if (songLoopSetting == LoopVers.loopPlaylist)
        {
            songLoopSetting = LoopVers.loopSong;
        }
        else if (songLoopSetting == LoopVers.loopSong)
        {
            songLoopSetting = LoopVers.noLoop;
        }
        else
        {
            songLoopSetting = LoopVers.loopPlaylist;
        }
    }

    public void SkipSong(int skip)
    {

        playMovements = false;
        referenceSpeaker.time = 0;
        referenceVideo.time = 0;
        if (songLoopSetting == LoopVers.noLoop || songLoopSetting == LoopVers.loopPlaylist)
        {
            currentShowtapeSegment += skip;
        }

        if (currentShowtapeSegment < 0)
        {
            currentShowtapeSegment = 0;
        }
        else if (currentShowtapeSegment >= showtapeSegmentPaths.Length)
        {
            if (songLoopSetting == LoopVers.loopPlaylist)
            {
                currentShowtapeSegment = 0;
            }
            else
            {
                currentShowtapeSegment = showtapeSegmentPaths.Length - 1;
            }
        }
        creator.LoadFromURL(showtapeSegmentPaths[currentShowtapeSegment]);

    }

    public void Unload()
    {
        videoPath = "";
        playMovements = false;
        recordMovements = false;
        referenceSpeaker.time = 0;
        referenceVideo.time = 0;
        currentShowtapeSegment = -1;
        showtapeSegmentPaths = new string[1];
        rshwData = new BitArray[0];
        audioVideoPause.Invoke();
        curtainClose.Invoke();
    }



    public void DeleteMove(int bitDelete)
    {
        int combinedNewInput = bitDelete + (24 * this.GetComponent<UI_WindowMaker>().deletePage);
        Debug.Log("Deleting Move: " + combinedNewInput);


        //Call File Browser
        showtapeSegmentPaths = new string[1];
        var paths = StandaloneFileBrowser.OpenFilePanel("Browse Showtape", "", fileExtention, false);
        if (paths.Length > 0)
        {
            showtapeSegmentPaths[0] = paths[0];
            currentShowtapeSegment = 0;
            playMovements = false;
            //Check if null
            if (showtapeSegmentPaths[0] != "")
            {
                rshwFormat thefile = rshwFormat.ReadFromFile(showtapeSegmentPaths[0]);
                speakerClip = OpenWavParser.ByteArrayToAudioClip(thefile.audioData);
                List<BitArray> newSignals = new List<BitArray>();
                int countlength = 0;
                if (thefile.signalData[0] != 0)
                {
                    countlength = 1;
                    BitArray bit = new BitArray(300);
                    newSignals.Add(bit);
                }
                for (int i = 0; i < thefile.signalData.Length; i++)
                {
                    if (thefile.signalData[i] == 0)
                    {
                        countlength += 1;
                        BitArray bit = new BitArray(300);
                        newSignals.Add(bit);
                    }
                    else
                    {
                        newSignals[countlength - 1].Set(thefile.signalData[i] - 1, true);
                    }
                }
                rshwData = newSignals.ToArray();

                //Actual Deletion Code
                for (int i = 0; i < rshwData.Length; i++)
                {
                    rshwData[i].Set(combinedNewInput - 1, false);
                }
                creator.SaveRecording();
            }
        }
    }

    public void DeleteMoveNoSaving(int bitDelete, bool fill)
    {
        Debug.Log("Deleting Move (No Save): " + bitDelete);

        //Actual Deletion Code
        for (int i = 0; i < rshwData.Length; i++)
        {
            rshwData[i].Set(bitDelete - 1, fill);
        }
    }

    public void PadMove(int bitPad, int padding)
    {
        bitPad -= 1;
        if (padding > 0)
        {
            int oldLength = rshwData.Length;
            //Create new space
            for (int i = 0; i < padding; i++)
            {
                rshwData = rshwData.Append(new BitArray(300)).ToArray();
            }
            for (int e = 0; e < oldLength; e++)
            {
                rshwData[rshwData.Length - 1 - e].Set(bitPad, rshwData[oldLength - 1 - e].Get(bitPad));
            }
        }
        else
        {
            padding = Mathf.Abs(padding);
            for (int i = 0; i < rshwData.Length - padding; i++)
            {
                rshwData[i].Set(bitPad, rshwData[i + padding].Get(bitPad));
            }
        }
    }

    public void PadAllBits(int padding)
    {
        if (rshwData != null)
        {
            int address;
            if (useVideoAsReference)
            {
                address = (int)(referenceVideo.time * dataStreamedFPS);
            }
            else
            {
                address = (int)(referenceSpeaker.time * dataStreamedFPS);
            }


            if (padding > 0)
            {
                //Create new space
                for (int i = 0; i < padding; i++)
                {
                    rshwData = rshwData.Append(new BitArray(300)).ToArray();
                }

                int track = rshwData.Length;
                while (track > address)
                {
                    if (track + padding < rshwData.Length)
                    {
                        rshwData[track + padding] = (rshwData[track]);
                    }
                    track--;
                }
            }
            else
            {
                while (address < rshwData.Length)
                {
                    if (address + padding > 0)
                    {
                        rshwData[address + padding] = (rshwData[address]);
                    }
                    address++;
                }
            }

        }
    }



}