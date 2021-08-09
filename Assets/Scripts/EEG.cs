using UnityEngine;
using System;
using System.IO;
using Photon.Pun;
using Assets.LSL4Unity.Scripts.Examples;

public class EEG : MonoBehaviourPunCallbacks, IPunObservable
{

    public string fileName = "EEG-S1.csv";

    private ExampleFloatInlet inlet;
    private bool spaceBar = false;


    public float timeTotal = 120;
    public float timeRemaining = 0;
    public bool record = false;

    public bool timerIsRunning = false;
    public string timeText;
    public int counter = 0;

    // Start is called before the first frame update
    private void Start()
    {
        PhotonNetwork.SendRate = 20; //20
        PhotonNetwork.SerializationRate = 5; //10
    }
    // Update is called once per frame
    private void FixedUpdate()
    {

        if (Input.GetKeyDown("space"))
        {
           photonView.RPC("RPC_SpaceBar", RpcTarget.All);
        }


        if (record == true)
        {
            TimeCounter();
        }
    }

    private void TimeCounter()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                SavingFile(); 
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                Debug.Log("END" + counter);
                timeRemaining = 0;
                SavingFile();
                timerIsRunning = false;
                record = false;
            }
        }
        
        DisplayTime(timeRemaining);
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timeText = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    public void SavingFile()
    {
        // Saving variables into a csv file
        var pathDirectory = @"C:\Ihshan\Codes\HyperscanningEyeGaze_Testing\HyperscanningEyeGaze_2exp\Assets\Data\";
        var pathFile = pathDirectory + fileName;
        var fileInfo = new FileInfo(pathFile);
        var directoryInfo = new DirectoryInfo(pathDirectory);

        var unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        if (!directoryInfo.Exists)
        {
            directoryInfo.CreateSubdirectory(pathDirectory);
        }

        if (!fileInfo.Exists)
        {
            using (var w = new StreamWriter(pathFile))
            {
                w.WriteLine("timestamp,FP1,FP2,F7,F3,F4,F8,T3/T7,C3,C4,T4/T8,T5/P7,P3,P4,T6/P8,O1,O2,Aux1,Aux2,Aux3,");

                if (record && spaceBar == true) // Set "BEGIN" Marker
                {
                    w.WriteLine("BEGIN" + counter);
                    spaceBar = false;
                }

                var strData = unixTimestamp.ToString();
                foreach (var item in inlet.lastSample)
                {
                    strData += "," + item.ToString();
                }
                w.WriteLine(strData);
                w.Flush();

            }

        }
        else
        {
            using (var w = new StreamWriter(pathFile, true))
            {
                if (record && spaceBar == true) // Set "BEGIN" Marker
                {
                    w.WriteLine("BEGIN" + counter);
                    spaceBar = false;
                }

                var strData = unixTimestamp.ToString();
                foreach (var item in inlet.lastSample)
                {
                    strData += "," + item.ToString();
                }
                w.WriteLine(strData);
                w.Flush();

                if(record && timeRemaining == 0) // Set "END" Marker
                {
                    w.WriteLine("END" + counter);
                }
                
            }

        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Debug.Log("Is Writing");
            stream.SendNext(timeTotal);
            stream.SendNext(fileName);
        }
        else
        {
            Debug.Log("Is Reading");
            timeTotal = (float)stream.ReceiveNext();
            fileName = (string)stream.ReceiveNext();
        }
    }

    [PunRPC]
    private void RPC_SpaceBar()
    {
        // Get ExampleFloatInlet component, which streams data from OpenBCI
        inlet = GetComponent<ExampleFloatInlet>();

        // Preparing to record
        counter += 1;
        Debug.Log("BEGIN" + counter);
        timerIsRunning = true;
        timeRemaining = timeTotal;
        record = !record;
        spaceBar = !spaceBar;
    }

   
}
