using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionChoice : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    LevelScript levelScript;
    Action action;
    public bool selected;
    private bool highlighted;

    public void OnPointerClick(PointerEventData eventData)
    {
        levelScript.changeSelectedAction(action);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlighted = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlighted = false;
    }

    public void setAction(Action action)
    {
        this.name = action.name;
        this.action = action;
        
    }

    // Start is called before the first frame update
    void Start()
    {
       levelScript = FindObjectOfType<LevelScript>();
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Image>().color = Color.white;
        if (selected)
        {
            GetComponent<Image>().color = Color.red;
            if (highlighted)
            {
                GetComponent<Image>().color = Color.magenta;
            }

        }
        else if (highlighted)
        {
            GetComponent<Image>().color = Color.gray;
        }
    }
}
