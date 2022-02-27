using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dialog : MonoBehaviour {
    public UnityEvent OK;
    public UnityEvent Cancel;

    Animator _animator;

    void Awake() {
        _animator = gameObject.GetComponent<Animator>();
    }
    void Start() {
        Hide();
    }

    public void OnOK() {
        OK?.Invoke();
    }
    public void OnCancel() {
        Cancel?.Invoke();
    }
    public void Show() {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("dialog_show") ||
            _animator.GetCurrentAnimatorStateInfo(0).IsName("dialog_idle")) {
            return;
        }
        _animator.SetTrigger("show");
    }
    public void Hide() {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("dialog_hide")) {
            return;
        }
        _animator.SetTrigger("hide");
    }
}
