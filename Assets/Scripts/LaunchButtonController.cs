using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchButtonController : MonoBehaviour
{
    public GameObject gantry;
    public List<GameObject> triggers;
    // Start is called before the first frame update
    private bool launchReady;
    AudioSource button;
    void Start()
    {
        launchReady = false;
        button = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (launchReady)
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        button.Play();
        Debug.Log("launchReady: " + launchReady);
        if (launchReady)
        {
            StartCoroutine("BePressed");
            gantry.SendMessage("Launch");
        }
    }

    IEnumerator BePressed()
    {
        for (int i = 0; i < 105; i++)
        {
            transform.Translate(new Vector3(0, (float)-0.0002, 0));
            yield return new WaitForSeconds((float)0.0002);
        }

        yield return new WaitForSeconds((float)0.5);
        StartCoroutine("Unpress");
    }

    IEnumerator Unpress()
    {
        for (int i = 0; i < 53; i++)
        {
            transform.Translate(new Vector3(0, (float)0.0004, 0));
            yield return new WaitForSeconds((float)0.0002);
        }
        yield return null;
    }

    void FueledUp(bool r)
    {
        launchReady = r;
    }
}
