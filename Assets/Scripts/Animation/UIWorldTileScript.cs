using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class UIWorldTileScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    

    private Animator anim;
    [SerializeField] private TextMeshProUGUI letterText;
    [SerializeField] private Animator letterAnim;
    [SerializeField] private string[] states = new string[2];

    [Header("Optional")]
    [SerializeField] private string scene;
    [SerializeField] private bool loadNewScene;

    public bool activated { get { return anim.GetBool("TileFlipped"); } }
    public void OnPointerClick(PointerEventData pointerEventData)
    {
 
        if (!(anim.GetCurrentAnimatorStateInfo(0).IsName("TileAnimation")))
        {
            // activate the trigger
            anim.SetTrigger("TileFlip");

            // change the value of tile flipped
            anim.SetBool("TileFlipped", !anim.GetBool("TileFlipped"));
            StartCoroutine(changeText(states[anim.GetBool("TileFlipped") ? 0 : 1], .2f));
            if (loadNewScene) StartCoroutine(newSceneDelay(.6f));
            letterAnim.SetBool("TileFlipped", !letterAnim.GetBool("TileFlipped"));
        }
    
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

    public void setReveal(bool revealed)
    {
        // reveal tile function
        anim.SetBool("TileFlipped", revealed);
        letterAnim.SetBool("TileFlipped", revealed);
    }

    private IEnumerator changeText(string text, float time)
    {
        yield return new WaitForSeconds(time);
        letterText.text = text;
    }

    private IEnumerator newSceneDelay(float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(scene);
    }

    public void revealTile()
    {
        // reveal tile function
        anim.SetBool("TileFlipped", false);
        letterAnim.SetBool("TileFlipped", false);
    }

    public void hideTile()
    {
        // hide tile function
        anim.SetBool("TileFlipped", true);
        letterAnim.SetBool("TileFlipped", true);
    }

    // Start is called before the first frame update
    void Start()
    {
        // get the current animation
        anim = GetComponent<Animator>();
        letterText.text = states[0];
    }

    public void setValue(bool newValue)
    {
        if (activated != newValue)
        {

            anim.SetBool("TileFlipped", newValue);
            letterAnim.SetBool("TileFlipped", newValue);
            StartCoroutine(changeText(states[anim.GetBool("TileFlipped") ? 0 : 1], .2f));
        }
    }
}
