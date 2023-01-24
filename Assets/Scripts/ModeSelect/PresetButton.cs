using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PresetButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI letterText;
    [SerializeField] private string[] settings;
    public Animator anim;
    private string text;
    public Animator letterAnim;

    public void instantiate(string[] settings)
    {
        this.settings = settings;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {

        if (!anim.GetBool("TileFlipped"))
        {
            // change the value of tile flipped
            anim.SetBool("TileFlipped", true);
            StartCoroutine(changeText("", .2f));
            GameManager.instance.newSettings(settings);
            letterAnim.SetBool("TileFlipped", true);
        }

    }

    private IEnumerator changeText(string text, float time)
    {
        yield return new WaitForSeconds(time);
        letterText.text = text;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        // set the tile touched variable in animators
        anim.SetBool("TileTouched", true);
        letterAnim.SetBool("TileTouched", true);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        // set the tile touched variable in animator
        anim.SetBool("TileTouched", false);
        letterAnim.SetBool("TileTouched", false);
    }

    public void Update()
    {
        if (checkSettings())
        {
            if (!anim.GetBool("TileFlipped"))
            {
                anim.SetBool("TileFlipped", true);
                StartCoroutine(changeText("", .2f));
                letterAnim.SetBool("TileFlipped", true);
            }
        }
        else
        {
            if (anim.GetBool("TileFlipped"))
            {
                anim.SetBool("TileFlipped", false);
                StartCoroutine(changeText(text, .2f));
                letterAnim.SetBool("TileFlipped", false);
            }
        }
        Debug.Log(checkSettings());
    }

    public void Start()
    {
        text = letterText.text;
    }

    private bool checkSettings()
    {
        bool same = true;
        for(int i = 0; i < settings.Length; i++)
        {
            string setting = settings[i];
            string gameSetting = GameManager.instance.curSettings()[i];
            if (setting != gameSetting) same = false;
        }

        return same;
    
    }
}
