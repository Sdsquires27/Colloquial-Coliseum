using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<GameManager>().setPanel(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
