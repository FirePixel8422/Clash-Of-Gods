using UnityEngine;
using UnityEngine.Events;

public class ClickableCollider : MonoBehaviour
{
    public UnityEvent OnClickEvent;

    public bool triggerAnimator;
    private Animator anim;

    public virtual void Start()
    {
        anim = GetComponent<Animator>();
    }


    private void OnMouseEnter()
    {
        if (triggerAnimator)
        {
            anim.SetTrigger("Highlighted");
        }
    }
    private void OnMouseExit()
    {
        if (triggerAnimator)
        {
            anim.SetTrigger("Normal");
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            OnClick();

            if (triggerAnimator)
            {
                anim.SetTrigger("Pressed");
            }
        }
    }

    public virtual void OnClick()
    {
        OnClickEvent.Invoke();
    }
}
