using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathProjectileMovement : MonoBehaviour
{

    public float PathTravelTime;
    public string PathName;


    // Use this for initialization
    void Start()
    {
        var path = iTweenPath.GetPath(PathName);
        Debug.Log("Moving projectile on path: " + PathName + ", " + PathTravelTime);
        iTween.MoveTo(gameObject, iTween.Hash("path", path, "time", PathTravelTime, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.none, "oncomplete", "PathComplete"));
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PathComplete()
    {
        Debug.Log("Path complete");
        Destroy(this.gameObject);
    }
}