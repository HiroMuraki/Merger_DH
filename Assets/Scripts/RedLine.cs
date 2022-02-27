using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedLine : MonoBehaviour {
    Animator _animator;

    private void Awake() {
        _animator = gameObject.GetComponent<Animator>();
    }

    public void Flash() {
        _animator.SetTrigger("flash");
    }
    public void Tip() {
        _animator.SetTrigger("tip");
    }
}
