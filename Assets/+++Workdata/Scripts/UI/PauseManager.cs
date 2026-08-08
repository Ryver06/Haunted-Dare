using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    
    public GameObject pauseMenu;
    
    
    private bool isPaused = false; 
    public PlayerController pc;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pauseMenu.SetActive(false); 
        Time.timeScale = 1f; 
    }
    
    
    public void Pause()
    {
        isPaused = !isPaused; //toggles the pause bool
        pauseMenu.SetActive(isPaused); //activates/deactivates the 
        Time.timeScale = isPaused ? 0f : 1f; //freezes or unfreezes the game

        // Disables or enables player input based on the pause bool
        if (isPaused)
        {
            pc.DisableInput();
        }
        else
        {
            pc.EnableInput();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ReturnToGame() // Resumes the game by disabling the pause menu, re-enabling player controls and unfreezing the game
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        pc.EnableInput();
        Time.timeScale = 1f;
    }
    
}

