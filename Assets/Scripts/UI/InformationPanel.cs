using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InformationPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private static InformationPanel instance;

    

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void callPanel(string text)
    {
        instance.text.text = text;
        instance.gameObject.SetActive(true);
    }
    public static void callPanel(string text, Vector2 pos)
    {
        instance.text.text = text;
        instance.transform.position = pos;

        instance.gameObject.SetActive(true);
    }
    public static void dismissPanel()
    {
        instance.gameObject.SetActive(false);
    }

}
