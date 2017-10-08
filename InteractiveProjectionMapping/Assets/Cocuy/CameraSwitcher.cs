using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour {

    [SerializeField]
    Camera firstPersonCamera;
    [SerializeField]
    Camera overheadCamera;

    public void ShowOverheadView()
    {
        firstPersonCamera.enabled = false;
        overheadCamera.enabled = true;
    }

    public void ShowFirstPersonView()
    {
        firstPersonCamera.enabled = true;
        overheadCamera.enabled = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("up called");
            ShowOverheadView();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("down called");
            ShowFirstPersonView();
        }
    }
}
