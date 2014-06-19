using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiPathManager : MonoBehaviour {

    public List<string> Paths;
    public float Time;
    public float DoubleBackOn;

    private int _pathIndex = 0;
    private float _baseDistance;

    private SpriteRotator _spriteRotator;

	// Use this for initialization
	void Start () {
        _baseDistance = iTween.PathLength(iTweenPath.GetPath(Paths[0]));
        _spriteRotator = GetComponent<SpriteRotator>();
        StartNextPath();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void StartNextPath()
    {

        string pathName = Paths[_pathIndex];
        var path = iTweenPath.GetPath(pathName);
        float distance = iTween.PathLength(path);
        float ratio = distance / _baseDistance;

        float adjustedTime = ratio * Time;

        iTween.MoveTo(gameObject, iTween.Hash("path", path, "time", adjustedTime, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.none, "oncomplete", "StartNextPath"));
        //Debug.Log("Moving to path: " + pathName);
        _pathIndex++;
        if (_pathIndex >= Paths.Count)
        {
            _pathIndex = 0;
        }

        if (_pathIndex == DoubleBackOn || _pathIndex == 1)
        {

            if (_spriteRotator != null)
            {
               // Debug.Log("Rotating sprite");
                _spriteRotator.Direction *= -1;
            }
        }
    }
}
