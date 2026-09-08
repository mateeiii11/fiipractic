using RetroShadersPro.URP;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Raycasttt : MonoBehaviour
{
    Ray ray;
    public Volume post;
    [SerializeField] Transform target;
    public GameObject[] Press;
    [SerializeField] Trigger instance;
    public AudioSource vhs;
    public GameObject ahh;
    public bool isIntruder = false;
    public bool isDone = false;
    void Update()
    {
        ray = new Ray(target.position, target.forward);
        Debug.DrawRay(target.position, target.forward, Color.yellow);
        if(Physics.Raycast(ray, out  RaycastHit hit))
        {
            Debug.Log(hit.collider.tag);
            if (hit.collider.tag == "Intruder" && isDone == true)
            {
                
                isIntruder = true;
            }
            else
            {
                for (int i = 0; i < Press.Length; i++)
                {
                    if (hit.collider.tag == Press[i].tag)
                    {
                        Press[i].SetActive(true);
                        if (hit.collider.tag == "Power")
                            if (Input.GetMouseButton(0) && instance.trigger2 == true)
                            {
                                instance.lights.SetActive(true);
                                instance.playerLight.SetActive(false);
                                instance.switch2.enabled = true;
                                instance.Intruder.SetActive(true);
                                instance.Generator.SetActive(false);
                                isDone = true;
                                ahh.SetActive(true);
                            }
                    }
                    else
                        Press[i].SetActive(false);
                }
            }
        }

        if(isIntruder)
        {
            vhs.volume = 0;
            StartCoroutine(mata());
        }
    }

    private IEnumerator mata()
    {
        yield return new WaitForSeconds(1.5f);
        post.profile.TryGet(out CRTSettings settings);
        settings.pixelSize.value++;
        if (settings.pixelSize.value > 10)
        {
            Debug.Log(":exioted");
            Application.Quit();
        }
    }
}
