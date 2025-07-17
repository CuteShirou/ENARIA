using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorHover : MonoBehaviour
{
    private Animator animator;
    public bool outline;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnMouseEnter()
    {
        outline = true;
        animator.SetBool("OutlineSwap", outline);
    }

    private void OnMouseExit()
    {
        outline = false;
        animator.SetBool("OutlineSwap", outline);
    }
}
