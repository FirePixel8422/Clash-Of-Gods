using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickableCollider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public UnityEvent OnClickEvent;
    [HideInInspector]
    public UnityEvent OnMouseEnterEvent;
    [HideInInspector]
    public UnityEvent OnMouseExitEvent;

    public bool interactable = true;

    public bool triggerAnimator;
    private Animator anim;

    public virtual void Start()
    {
        anim = GetComponent<Animator>();
    }


    private void OnMouseEnter()
    {
        if (interactable)
        {
            if (triggerAnimator)
            {
                anim.SetTrigger("Highlighted");
            }

            OnMouseEnterGUI();
        }
    }
    protected virtual void OnMouseEnterGUI()
    {
        OnMouseEnterEvent.Invoke();
    }

    private void OnMouseExit()
    {
        if (interactable)
        {
            if (triggerAnimator)
            {
                anim.SetTrigger("Normal");
            }

            OnMouseExitGUI();
        }
    }
    protected virtual void OnMouseExitGUI()
    {
        OnMouseExitEvent.Invoke();
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



    public void OnPointerEnter(PointerEventData eventData)
    {
        if (interactable)
        {
            if (triggerAnimator)
            {
                anim.SetTrigger("Highlighted");
            }

            OnMouseEnterGUI();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (interactable)
        {
            if (triggerAnimator)
            {
                anim.SetTrigger("Normal");
            }

            OnMouseExitGUI();
        }
    }
}
