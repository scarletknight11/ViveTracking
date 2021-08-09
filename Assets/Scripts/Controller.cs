using UnityEngine;
using Photon.Pun;
using Assets.LSL4Unity.Scripts.Examples;

// NOTE :
// This is to activate ExampleFloatInlet which is for acquiring EEG data to be put into the memory first
// Then activating EEG that will grab the EEG data in ExampleFloatInlet to be put into CSV later on.
// If I don't use this Controller code, it creates an issue; does not save the data into the file.
//
// INSTRUCTION : 
// After Play
// Press ENTER to ACTIVATE THE COMPONENTS (ExampleFloatInlet & EEG), Then press Spacebar to begin recording

public class Controller : MonoBehaviour, IPunObservable
{
    private ExampleFloatInlet exampleFloatInlet;
    private EEG eeg;
    private bool enterPressed = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        eeg = GetComponent<EEG>();
        exampleFloatInlet = GetComponent<ExampleFloatInlet>();

        exampleFloatInlet.enabled = !exampleFloatInlet.enabled;
        eeg.enabled = !eeg.enabled;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            exampleFloatInlet.enabled = true;
            eeg.enabled = true;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Debug.Log("Is Writing");
            stream.SendNext(exampleFloatInlet.enabled);
            stream.SendNext(eeg.enabled);
        }
        else
        {
            Debug.Log("Is Reading");
            exampleFloatInlet.enabled = (bool)stream.ReceiveNext();
            eeg.enabled = (bool)stream.ReceiveNext();
        }
    }

}
