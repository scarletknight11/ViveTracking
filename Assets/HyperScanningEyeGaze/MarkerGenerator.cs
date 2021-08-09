using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.LSL4Unity.Scripts;

public class MarkerGenerator : MonoBehaviour
{

    public float secondsBetweenMarkers = 1f;
    private LSLMarkerStream marker;
    private int markerIndex;

    // Start is called before the first frame update
    void Start()
    {
      marker = FindObjectOfType<LSLMarkerStream>();
      markerIndex = 0;
      //InvokeRepeating("sendMarker", 0f, secondsBetweenMarkers);
    }

    IEnumerator sendMarkerEnum(){
      while(true){
        yield return new WaitForSeconds(secondsBetweenMarkers);
        sendMarker();
      }
    }

    private bool startMarkerEnum = true;

    // Update is called once per frame
    void Update()
    {
      if(startMarkerEnum){
        //Only doing this because InvokeRepeating wasn't working on Windows
        startMarkerEnum = false;
        StartCoroutine(sendMarkerEnum());
      }
    }

    void sendMarker(){
      Debug.Log("Writing marker " + (markerIndex + 1));
      markerIndex += 1;
      marker.Write("Marker " + markerIndex);
    }
}
