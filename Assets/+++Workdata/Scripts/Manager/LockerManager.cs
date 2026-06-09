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
        public PlayableDirector cutscene;
        public Transform tp_pos;
    }

    [SerializeField] private Transform player;
    [SerializeField] private Transform test;
    public List<LockerTimeline> timeline = new List<LockerTimeline>();

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
        
        timeline[id].cutscene.Play();
        player.position = timeline[id].tp_pos.position;
    }

    public void Test()
    {
        player.position = test.position;
    }
}
