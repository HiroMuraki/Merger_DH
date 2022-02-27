using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreText : MonoBehaviour {
    Animator _animator;

    private void Awake() {
        _animator = gameObject.GetComponent<Animator>();
    }

    public void Pop() {
        _animator.SetTrigger("value_changed");
    }
}
