using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleVRRockets
{
    public class PlayClearAreaSound : MonoBehaviour
    {
        public List<GameObject> speakers;
        public List<GameObject> triggers;
        public GameObject fuelButton;
        // Start is called before the first frame update
        private bool fired, rocketReady;
        AudioSource button;
        void Start()
        {
            fired = rocketReady = false;
            button = GetComponent<AudioSource>();
        }
        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            button.Play();
            if (rocketReady && !fired)
            {
                fired = true;
                StartCoroutine("PlayKlaxons");
                StartCoroutine("BePressed");
                fuelButton.SendMessage("AreaClear");
            }
        }

        IEnumerator PlayKlaxons()
        {
            foreach (var s in speakers)
            {
                s.GetComponent<AudioSource>().Play();
            }
            yield return null;
        }

        IEnumerator BePressed()
        {
            for (int i = 0; i < 105; i++)
            {
                transform.Translate(new Vector3(0, (float) -0.0002, 0));
                yield return new WaitForSeconds((float) 0.0002);
            }

            yield return new WaitForSeconds((float) 0.5);
            StartCoroutine("Unpress");
        }

        IEnumerator Unpress()
        {
            for (int i = 0; i < 53; i++)
            {
                transform.Translate(new Vector3(0, (float) 0.0004, 0));
                yield return new WaitForSeconds((float) 0.0002);
            }
            yield return null;
        }

        void RocketReady(bool r)
        {
            rocketReady = r;
        }

        void RocketLaunched()
        {
            fired = false;
        }
    }
}
