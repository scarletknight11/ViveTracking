

//// NOTES !!! : ***** Don't forget to adjust the path to save file in SavingFile function below, before using this script  *****
////             ***** Press ENTER / RETURN to make EEG data (from OpenBCI) available in the memory, otherwise it will not record the data into the file *****
//// 1. Adjust the file name (from Master) for each new participant in the inspector (EEG & EyeTracker game object)
//// 2. Set the time in seconds (from Master) for recording in the inspector (EEG & EyeTracker game object)
//// 3. Press SPACEBAR for recording the data (EEG and Eye tracker).

//using Photon.Pun;
//using System;
//using System.IO;
//using UnityEngine;
//using ViveSR.anipal.Eye;

//// From TriggerOpenVibe
//using System.Linq;
//using System.Net.Sockets;

//public class EyeInfo : MonoBehaviourPunCallbacks, IPunObservable
//{
//    public string fileName = "EyeTracker-S1.csv";
//    public float timeTotal = 120;
//    public float timeRemaining = 0;
//    public bool record = false;
//    public bool timerIsRunning = false;
//    public string timeText;
//    public int counter = 0;

//    private string currentStatus;
//    private FocusInfo focusInfo;
//    private Ray gazeRay;
//    private static EyeData eyeData = new EyeData();
//    public int unixTimeStamp;
//    private float pupilDiameterRight;
//    private float pupilDiameterLeft;
//    private Vector2 pupilPositionRight;
//    private Vector2 pupilPositionLeft;
//    private Vector3 gazeOriginRight;
//    private Vector3 gazeOriginLeft;
//    private Vector3 gazeDirectionRight;
//    private Vector3 gazeDirectionLeft;
//    private float eyeOpenRight;
//    private float eyeOpenLeft;
//    private string currentFocusObject;
//    private bool spaceBar = false;
//    private int markerOpenVibe = 9999999;

//    // From TriggerOpenVibe
//    private static TcpClient _socketConnection1;
//    private static TcpClient _socketConnection2;


//    //private EyeExpression eyeExpression;
//    //private float rightEyeFrown;
//    //private float leftEyeFrown;
//    //private float rightEyeSqueeze;
//    //private float leftEyeSqueeze;
//    //private float rightEyeWide;
//    //private float leftEyeWide;

//    // Start is called before the first frame update
//    void Start()
//    {
//        SRanipal_Eye_API.GetEyeData(ref eyeData);
//        PhotonNetwork.SendRate = 20; //20
//        PhotonNetwork.SerializationRate = 5; //10

//        // From TriggerOpenVibe
//        try
//        {
//            /*
//             * Connect to the OpenVibe Acquisition Server. Unless settings have been modified
//             * or you're running OpenVibe on a different machine, the address should be
//             * localhost and the port 15361. These settings are in Acquisition Server -> Preferences.
//             */
//            _socketConnection1 = new TcpClient("localhost", 15361);
//            _socketConnection2 = new TcpClient("127.0.0.1", 15361);
//            Debug.Log("Successfully connected to OpenVibe");
//        }
//        catch (SocketException se)
//        {
//            Debug.Log("Socket exception: " + se);
//        }

//    }


//    // Update is called once per frame
//    void Update()
//    {
//        CheckStatus();
//        Time();
//        Pupil();
//        EyeOpenClose();
//        CheckFocus();
//        GazeDirections();
//        //EyeExpressions();

//        if (Input.GetKeyDown("space"))
//        {
//            photonView.RPC("RPC_SpaceBarEyeInfo", RpcTarget.All);

//            // Calling sendTrigger
//            //SendTrigger(markerOpenVibe);
//        }

//        if (record == true)
//        {
//            TimeCounter();
//        }
//    }


//    // From TriggerOpenVibe

//    /*
//     * Call this from your experiment code to send a trigger. It can take
//     * any integer. You can have as many different trigger codes as you like.
//     */
//    private void SendTrigger(int eventId)
//    {
//        SendTrigger1(eventId);
//        SendTrigger2(eventId);
//    }
//    private void SendTrigger1(int eventId)
//    {
//        var stream = _socketConnection1.GetStream();

//        if (!stream.CanWrite) return;

//        var buffer = BitConverter.GetBytes((ulong)0);
//        var eventTag = BitConverter.GetBytes((ulong)eventId);

//        var sendArray = buffer.Concat(eventTag.Concat(buffer)).ToArray();

//        stream.Write(sendArray, 0, sendArray.Length);
//    }

//    private void SendTrigger2(int eventId)
//    {
//        var stream = _socketConnection2.GetStream();

//        if (!stream.CanWrite) return;

//        var buffer = BitConverter.GetBytes((ulong)0);
//        var eventTag = BitConverter.GetBytes((ulong)eventId);

//        var sendArray = buffer.Concat(eventTag.Concat(buffer)).ToArray();

//        stream.Write(sendArray, 0, sendArray.Length);
//    }

//    // EyeInfo Original
//    private void TimeCounter()
//    {
//        if (timerIsRunning)
//        {

//            if (timeRemaining > 0)
//            {
//                SavingFile();
//                timeRemaining -= UnityEngine.Time.deltaTime;
//            }
//            else
//            {
//                Debug.Log("END" + counter);
//                // Calling sendTrigger
//                //SendTrigger(markerOpenVibe);
//                timeRemaining = 0;
//                SavingFile();
//                timerIsRunning = false;
//                record = false;

//            }
//        }

//        DisplayTime(timeRemaining);
//    }

//    void DisplayTime(float timeToDisplay)
//    {
//        timeToDisplay += 1;

//        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
//        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

//        timeText = string.Format("{0:00}:{1:00}", minutes, seconds);
//    }

//    public void CheckStatus()
//    {
//        if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
//            currentStatus = "Eye tracker  is WORKING";
//        else if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.STOP)
//            currentStatus = "Eye tracker  is STOPING";
//        else if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.START)
//            currentStatus = "Eye tracker  is STARTING";
//        else if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.ERROR)
//            currentStatus = "Eye tracker  is having an ERROR";
//        else if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
//            currentStatus = "Your eye tracker is not supported Bro !";

//        print(currentStatus);
//    }

//    public void Time()
//    {
//        // Machine Timestamp (unix)
//        unixTimeStamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
//    }

//    public void Pupil()
//    {
//        // Update eye data
//        SRanipal_Eye_API.GetEyeData(ref eyeData);

//        // Add pupil diameters (Indicator of emotion) 
//        pupilDiameterRight = eyeData.verbose_data.right.pupil_diameter_mm;
//        pupilDiameterLeft = eyeData.verbose_data.left.pupil_diameter_mm;

//        // Pupil positions (Where participants look at, where the eyes are in the headset)
//        pupilPositionRight = eyeData.verbose_data.right.pupil_position_in_sensor_area;
//        pupilPositionLeft = eyeData.verbose_data.left.pupil_position_in_sensor_area;
//    }

//    public void EyeOpenClose()
//    {
//        SRanipal_Eye_API.GetEyeData(ref eyeData);

//        // Add whether eyes are open or not into a list ( How the eyes open; 0 = closed, 1 = fully opened)
//        eyeOpenRight = eyeData.verbose_data.right.eye_openness;
//        eyeOpenLeft = eyeData.verbose_data.left.eye_openness;

//    }
//    public void CheckFocus()
//    {
//        bool eyeFocus;
//        if (SRanipal_Eye.Focus(GazeIndex.COMBINE, out gazeRay, out focusInfo))
//        {
//            eyeFocus = SRanipal_Eye.Focus(GazeIndex.COMBINE, out gazeRay, out focusInfo);
//            if (eyeFocus)
//            {
//                currentFocusObject = focusInfo.collider.gameObject.name;
//            }
//        }
//        else if (SRanipal_Eye.Focus(GazeIndex.RIGHT, out gazeRay, out focusInfo))
//        {
//            eyeFocus = SRanipal_Eye.Focus(GazeIndex.COMBINE, out gazeRay, out focusInfo);
//            if (eyeFocus)
//            {
//                currentFocusObject = focusInfo.collider.gameObject.name;
//            }
//        }
//        else if (SRanipal_Eye.Focus(GazeIndex.LEFT, out gazeRay, out focusInfo))
//        {
//            eyeFocus = SRanipal_Eye.Focus(GazeIndex.COMBINE, out gazeRay, out focusInfo);
//            if (eyeFocus)
//            {
//                currentFocusObject = focusInfo.collider.gameObject.name;
//            }
//        }
//        else
//        {
//            currentFocusObject = "Nothing";
//        }

//    }

//    public void GazeDirections()
//    {
//        gazeOriginRight = eyeData.verbose_data.right.gaze_origin_mm;
//        gazeOriginLeft = eyeData.verbose_data.left.gaze_origin_mm;
//        gazeDirectionRight = eyeData.verbose_data.right.gaze_direction_normalized;
//        gazeDirectionLeft = eyeData.verbose_data.left.gaze_direction_normalized;
//    }

//    //public void EyeExpressions()
//    //{
//    //    // Eye frowns
//    //    rightEyeFrown = eyeExpression.right.eye_frown;
//    //    leftEyeFrown = eyeExpression.left.eye_frown;

//    //    // Eye squeezes
//    //    rightEyeSqueeze = eyeExpression.right.eye_squeeze;
//    //    leftEyeSqueeze = eyeExpression.left.eye_squeeze;

//    //    // Eye wides
//    //    rightEyeWide = eyeExpression.right.eye_wide;
//    //    leftEyeWide = eyeExpression.left.eye_wide;
//    //}


//    public void SavingFile()
//    {
//        // Saving variables into a csv file
//        var pathDirectory = @"C:\Users\igum002\Documents\Experiments\HyperscanningEyeGaze_2exp_ready4experiment_latest\Assets\Data\";
//        var pathFile = pathDirectory + fileName;
//        var fileInfo = new FileInfo(pathFile);
//        var directoryInfo = new DirectoryInfo(pathDirectory);

//        // Create a new directory if not available yet
//        if (!directoryInfo.Exists)
//        {
//            directoryInfo.CreateSubdirectory(pathDirectory);
//        }

//        // Create a new file, if not available yet
//        if (!fileInfo.Exists)
//        {
//            using (var w = new StreamWriter(pathFile))
//            {
//                var headers = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}", "UnixTimeStamp", "PupilDiameterRight", "PupilDiameterLeft", "PupilPositionRight(X)", "PupilPositionRight(Y)", "PupilPositionLeft(X)", "PupilPositionLeft(Y)", "EyeOpenRight", "EyeOpenLeft",
//                                                                                             "GazeOriginRight(X)", "GazeOriginRight(Y)", "GazeOriginRight(Z)", "GazeOriginLeft(X)", "GazeOriginLeft(Y)", "GazeOriginLeft(Z)",
//                                                                                             "GazeDirectionRight(X)", "GazeDirectionRight(Y)", "GazeDirectionRight(Z)", "GazeDirectionLeft(X)", "GazeDirectionLeft(Y)", "GazeDirectionLeft(Z)",
//                                                                                             "CurrentFocusObject");

//                w.WriteLine(headers);

//                if (record && spaceBar == true) // Set "BEGIN" Marker (1st Marker)
//                {
//                    w.WriteLine("BEGIN" + counter);
//                    // Sending Trigger (for client use only)
//                    SendTrigger(markerOpenVibe);
//                    spaceBar = false;
//                }

//                var line = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}", unixTimeStamp, pupilDiameterRight, pupilDiameterLeft, pupilPositionRight.x, pupilPositionRight.y, pupilPositionLeft.x, pupilPositionLeft.y, eyeOpenRight, eyeOpenLeft,
//                                                                                            gazeOriginRight.x, gazeOriginRight.y, gazeOriginRight.z, gazeOriginLeft.x, gazeOriginLeft.y, gazeOriginLeft.z,
//                                                                                            gazeDirectionRight.x, gazeDirectionRight.y, gazeDirectionRight.z, gazeDirectionLeft.x, gazeDirectionLeft.y, gazeDirectionLeft.z, currentFocusObject);
//                w.WriteLine(line);
//                w.Flush();

//            }

//        }
//        // If the file already exists, then goes here
//        else
//        {
//            using (var w = new StreamWriter(pathFile, true))
//            {

//                if (record && spaceBar == true) // Set "BEGIN" Marker (2nd, 3rd,... Markers)
//                {
//                    w.WriteLine("BEGIN" + counter);
//                    // Sending Trigger (for client use only)
//                    SendTrigger(markerOpenVibe);

//                    spaceBar = false;
//                }

//                var line = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}", unixTimeStamp, pupilDiameterRight, pupilDiameterLeft, pupilPositionRight.x, pupilPositionRight.y, pupilPositionLeft.x, pupilPositionLeft.y, eyeOpenRight, eyeOpenLeft,
//                                                                            gazeOriginRight.x, gazeOriginRight.y, gazeOriginRight.z, gazeOriginLeft.x, gazeOriginLeft.y, gazeOriginLeft.z,
//                                                                            gazeDirectionRight.x, gazeDirectionRight.y, gazeDirectionRight.z, gazeDirectionLeft.x, gazeDirectionLeft.y, gazeDirectionLeft.z, currentFocusObject);

//                w.WriteLine(line);
//                w.Flush();


//                if (record && timeRemaining == 0) // Set "END" Marker (END1, END2,...Markers)
//                {
//                    w.WriteLine("END" + counter);
//                    SendTrigger(markerOpenVibe);
//                }
//            }
//        }
//    }

//    // ***********  Photon (PUN 2) related functions ***********
//    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
//    {
//        if (stream.IsWriting)
//        {
//            Debug.Log("Is Writing Eye Info");
//            stream.SendNext(timeTotal);
//            stream.SendNext(fileName);
//        }
//        else
//        {
//            Debug.Log("Is Reading Eye Info");
//            timeTotal = (float)stream.ReceiveNext();
//            fileName = (string)stream.ReceiveNext();
//        }
//    }

//    [PunRPC]
//    private void RPC_SpaceBarEyeInfo()
//    {
//        // Preparing to record
//        counter += 1;
//        Debug.Log("BEGIN" + counter);
//        timerIsRunning = true;
//        timeRemaining = timeTotal;
//        record = !record;
//        spaceBar = !spaceBar;
//    }

//}










