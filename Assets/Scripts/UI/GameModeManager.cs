using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    string dataPath;



    private void Start()
    {
        dataPath = Application.persistentDataPath;
        TextAsset[] texts = Resources.LoadAll<TextAsset>("Preset Settings");

    }
}
