using System.IO;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    protected string pathDirectory =  @"C:\Ihshan\Codes\HyperscanningEyeGaze_Testing\HyperscanningEyeGaze_2exp\Assets\Data\";
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void savingFile(string fileName, string header)
    {
        var pathFile = pathDirectory + fileName;
        var fileInfo = new FileInfo(pathFile);
        var directoryInfo = new DirectoryInfo(pathDirectory);
        StreamWriter w;

        if (!directoryInfo.Exists)
            directoryInfo.CreateSubdirectory(pathDirectory);

        if (!fileInfo.Exists)
        {
            w = new StreamWriter(pathFile);
            w.WriteLine(header);
        }
        else
            w = new StreamWriter(pathFile, true);


        w.Close();

    }
}
