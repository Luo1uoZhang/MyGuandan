using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp : MonoBehaviour
{
    public GameObject obj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!LeanTween.isTweening(obj))
        {
            LeanTween.rotateZ(obj, 60f, 0.25f).setOnComplete(() => LeanTween.rotateZ(obj, 0f, 0.25f));
            LeanTween.scaleX(obj, 1.5f, 0.25f).setOnComplete(() => LeanTween.scaleX(obj, 1f, 0.25f));
        }
    }
}
