using UnityEngine;
using UnityEngine.Events;

public class ClickableCollider : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent OnClickEvent;

    public bool interactable = true;

    public bool triggerAnimator;
    private Animator anim;

    public virtual void Start()
    {
        anim = GetComponent<Animator>();
    }


    private void OnMouseEnter()
    {
        if (interactable && triggerAnimator)
        {
            anim.SetTrigger("Highlighted");
        }
    }
    private void OnMouseExit()
    {
        if (interactable && triggerAnimator)
        {
            anim.SetTrigger("Normal");
        }
    }

    private void OnMouseOver()
    {
        if (interactable && Input.GetKeyDown(KeyCode.Mouse0))
        {
            OnClick();

            if (triggerAnimator)
            {
                anim.SetTrigger("Pressed");
            }
        }
    }

    protected virtual void OnClick()
    {
        OnClickEvent.Invoke();
    }
}
