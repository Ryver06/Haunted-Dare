using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("UI Lists")]
    [SerializeField] private List<GameObject> dialogUIList; // gameobjects that need to be turned off when in dialog
    [SerializeField] private List<GameObject> deathUIList; // gameobjects that need to be turned off when dead

    private void Awake()
    {
        Instance = this;
    }

    #region Disable and Enable
    
    private void DisableUI(List<GameObject> uiList)
    {
        uiList.ForEach(ui => ui.SetActive(false));
    }
    
    private void EnableUI(List<GameObject> uiList)
    {
        uiList.ForEach(ui => ui.SetActive(true));

        //            ui            uilist       =>
      //  foreach (var VARIABLE in COLLECTION) {...}
        
    }
    
    #endregion

    public void DisableForDialogUI()
    {
        DisableUI(dialogUIList);
    }
    
    public void EnableForDialogUI()
    {
       EnableUI(dialogUIList);
    }
    
    public void DisableForDeathUI()
    {
        DisableUI(deathUIList);
    }
}
