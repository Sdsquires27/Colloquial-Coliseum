using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using TMPro;

public class GameModeManager : MonoBehaviour
{
    public string fileLocation;
    TextAsset premadeFile;
    public string folderName;
    public string fileName;
    public GameObject presetButton;
   


    private void Start()
    {
         BasicSaveData loadedData = SaveLoad<BasicSaveData>.Load(folderName, fileName) ?? new BasicSaveData();
        if(loadedData.stringData == null)
        {
            premadeFile = Resources.Load<TextAsset>(fileLocation);
            loadedData.stringData = new List<string>(premadeFile.text.Split('\n'));
        }
        for(int i = 0; i < loadedData.stringData.Count / 7; i++)
        {
            GameObject x = Instantiate(presetButton, this.transform);
            x.GetComponentInChildren<TextMeshProUGUI>().text = loadedData.stringData[i * 7];
            string[] settings = new string[6];
            for(int j = 1; j < 7; j++)
            {
                settings[j] = loadedData.stringData[j];
            }
            x.GetComponent<PresetButton>().instantiate(settings);
        }

      
    }
}

[System.Serializable]
public class BasicSaveData
{
    public List<string> stringData;

    public BasicSaveData()
    {
        stringData = new List<string>();
    }

}
