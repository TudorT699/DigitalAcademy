using UnityEngine;
using System.Collections;

public class HoverButtonGroup : MonoBehaviour
{
    public RectTransform physicalButton;
    public RectTransform digitalButton;
    public RectTransform phoneButton;

    public float slideAmount = 200f;
    public float smoothTime = 0.25f;

    private Vector2 physicalStart;
    private Vector2 digitalStart;
    private Vector2 phoneStart;

    private Coroutine moveRoutine;

    void Awake()
    {
        physicalStart = physicalButton.anchoredPosition;
        digitalStart = digitalButton.anchoredPosition;
        phoneStart = phoneButton.anchoredPosition;
    }

    public void HoverPhysical()
    {
        StartMove(
            physicalStart,
            digitalStart + Vector2.left * (slideAmount * 0.5f),
            phoneStart + Vector2.left * slideAmount
        );
    }

    public void HoverDigital()
    {
        StartMove(
            physicalStart + Vector2.left * (slideAmount * 0.5f),
            digitalStart,
            phoneStart + Vector2.left * (slideAmount * 0.5f)
        );
    }

    public void HoverPhone()
    {
        StartMove(
            physicalStart + Vector2.left * (slideAmount * 0.5f),
            digitalStart + Vector2.left * (slideAmount * 0.5f),
            phoneStart
        );
    }

    public void ResetPositions()
    {
        StartMove(physicalStart, digitalStart, phoneStart);
    }

    void StartMove(Vector2 phys, Vector2 digi, Vector2 phone)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveButtons(phys, digi, phone));
    }

    IEnumerator MoveButtons(Vector2 phys, Vector2 digi, Vector2 phone)
    {
        Vector2 pVel = Vector2.zero;
        Vector2 dVel = Vector2.zero;
        Vector2 phVel = Vector2.zero;

        while (
            Vector2.Distance(physicalButton.anchoredPosition, phys) > 0.1f ||
            Vector2.Distance(digitalButton.anchoredPosition, digi) > 0.1f ||
            Vector2.Distance(phoneButton.anchoredPosition, phone) > 0.1f
        )
        {
            physicalButton.anchoredPosition =
                Vector2.SmoothDamp(physicalButton.anchoredPosition, phys, ref pVel, smoothTime);

            digitalButton.anchoredPosition =
                Vector2.SmoothDamp(digitalButton.anchoredPosition, digi, ref dVel, smoothTime);

            phoneButton.anchoredPosition =
                Vector2.SmoothDamp(phoneButton.anchoredPosition, phone, ref phVel, smoothTime);

            yield return null;
        }
    }
}