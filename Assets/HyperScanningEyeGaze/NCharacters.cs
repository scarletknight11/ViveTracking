using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NCharacters : MonoBehaviour
{

    public int n;

    int frame;
    GameObject[] characters;

    // Start is called before the first frame update
    void Start()
    {
      frame = 0;
      characters = null;
    }

    // Update is called once per frame
    void Update()
    {
      frame += 1;

      if(frame % 30 == 0){
        characters = GameObject.FindGameObjectsWithTag("3D Character");

        if(n < characters.Length){
          for(int i = characters.Length - 1; i >= n; i--){
            Destroy(characters[i]);
          }
        }

        characters = null;

        frame = 0;
      }
    }
}
