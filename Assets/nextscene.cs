using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class nextscene : MonoBehaviour
{
    public string scene;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Knight"))
        {
            SceneManager.LoadScene(scene);     
        }
    }
}
