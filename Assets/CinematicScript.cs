using PSXShadersPro.URP.Demo;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CinematicScript : MonoBehaviour
{
    public CinemachineCamera cam;
    public CinemachineInputAxisController controller;
    public GameObject player;
    public Animator animator;
    public MouseLook mouseLook;
    public GameObject GetFood;
    [SerializeField] Trigger triggerInstance;
    private bool isDone;
    public Raycasttt instance;

    bool AnimatorIsPlaying(string stateName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
    private void Awake()
    {
        cam.enabled = false;
        player.SetActive(false);
        controller.enabled = false;
        mouseLook.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GetFood.SetActive(false);
        isDone = false;
        triggerInstance.squeak.enabled = false;
        triggerInstance.Generator.SetActive(false);
        triggerInstance.playerLight.SetActive(false);
        triggerInstance.Intruder.SetActive(false);
        triggerInstance.switc.enabled = false;
        triggerInstance.switch2.enabled = false;
        instance.ahh.SetActive(false);
        for (int i = 0; i < instance.Press.Length; i++)
        {
            instance.Press[i].SetActive(false);
        }
        animator.Play("New Animation");
        
        
        ///animCam.Play();
    }

    private void Update()
    {
        
        if(AnimatorIsPlaying("New Animation") == false)
        {
            //Debug.Log(AnimatorIsPlaying("New Animation"));
            cam.enabled = true;
            player.SetActive(true);
            controller.enabled = true;
            mouseLook.enabled = true;
            if(triggerInstance.trigger2 == false)
                GetFood.SetActive(true);
            else GetFood.SetActive(false);
        }

    }
}
