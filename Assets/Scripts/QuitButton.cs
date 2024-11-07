using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitButton : MonoBehaviour
{
     // Method to quit the game
    public void QuitGame()
    {
        #if UNITY_EDITOR
            // If running in the editor, stop playing
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // If running on a mobile device or standalone, quit the application
            Application.Quit();
        #endif
    }
}
