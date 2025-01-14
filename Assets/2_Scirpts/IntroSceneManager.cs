using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneManager : MonoBehaviour
{
    
    [Header("Other")] 
    [SerializeField] private Animator animator;


    private void Awake()
    {
        if (!animator)
        {
            TryGetComponent(out animator);
            if (!animator)
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    private IEnumerator Start()
    {
        yield return  new WaitForSeconds(1f);
    }
}
