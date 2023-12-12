using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleUIVisibility : MonoBehaviour
{
    [SerializeField] KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] GameObject uiContainer = null;

    // Start is called before the first frame update
    void Start()
    {
        uiContainer.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
            Debug.Log("Tried Togglin");
        }
    }

    public void Toggle()
    {
        uiContainer.SetActive(!uiContainer.activeSelf);
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().TogglePlayable();
    }

}
