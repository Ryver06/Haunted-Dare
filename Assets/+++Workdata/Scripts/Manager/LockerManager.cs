using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class LockerManager : MonoBehaviour
{
    
    [Serializable]
    public class LockerTimeline
    {
        public int id;
        public PlayableDirector enterCutscene;
        public PlayableDirector exitCutscene;
        public Transform tp_pos;
        public Transform tp_exitPos;
    }
    
    public static LockerManager instance;

    [SerializeField] private Transform player;
    [SerializeField] private Transform playerCam;
    
    public List<LockerTimeline> timeline = new List<LockerTimeline>();

    private int lastLockerId; 

    private void Awake()
    {
        instance = this;
    }

    public void checkId(int lockerId)
    {
        //checks if id of locker matches to the cutscene id
        for (int i = 0; i < timeline.Count; i++)
        {
            //if match is found -> player Hides in locker
            if (timeline[i].id == lockerId)
            {
                HideInLocker(i); //saves which object in the list it is
            }
        }
    }

    /// <summary>
    /// player hides in the locker and cutscene is started
    /// </summary>
    private void HideInLocker(int id)
    {
        PlayerController.instance.EnterLockerMode();
        
        timeline[id].enterCutscene.Play();
        player.position = timeline[id].tp_pos.position;
        playerCam.rotation = timeline[id].tp_pos.rotation;
        
        lastLockerId = id; //save which locker player is currently in
        
    }

    public void ExitLocker()
    {
        timeline[lastLockerId].exitCutscene.Play();
        player.position = timeline[lastLockerId].tp_exitPos.position;
        playerCam.rotation = timeline[lastLockerId].tp_exitPos.rotation;
        
    }
    
}
