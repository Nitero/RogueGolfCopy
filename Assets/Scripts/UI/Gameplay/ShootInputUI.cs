using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ShootInputUI : MonoBehaviour
{
    [SerializeField] private ShootConfig _config;
    [SerializeField] private GameObject _dragIndicationParent;
    [SerializeField] private UILineRenderer _dragLine;
    [SerializeField] private Transform _dragStart;
    [SerializeField] private Transform _dragEnd;
    [SerializeField] private Image[] _dragCrossParts;
    [SerializeField] private float _dragCrossAnimationSpeed = 100f;
    
    private void Start()
    {
        _dragIndicationParent.SetActive(false);
    }

    public void ShowCross()
    {
        foreach (Image dragCrossPart in _dragCrossParts)
        {
            Vector3 localScale = dragCrossPart.transform.localScale;
            dragCrossPart.transform.localScale = new Vector3(Mathf.Min(localScale.x + Time.deltaTime * _dragCrossAnimationSpeed, 3), localScale.y, localScale.z);
        }
    }

    public void HideCross()
    {
        foreach (Image dragCrossPart in _dragCrossParts)
        {
            Vector3 localScale = dragCrossPart.transform.localScale;
            dragCrossPart.transform.localScale = new Vector3(Mathf.Max(localScale.x - Time.deltaTime * _dragCrossAnimationSpeed, 0), localScale.y, localScale.z);
        }
    }
    
    // Show circles connected with a line under finger & at start dragging pos
    public void ShowDragIndication(Vector3 startDrag, Vector3 endDrag)
    {
        // Clamp max drag distance
        if(Vector2.Distance(startDrag, endDrag) > _config.DragDistMinMax.magnitude)
        {
            var dir = endDrag - startDrag;
            endDrag = startDrag + dir.normalized * _config.DragDistMinMax.magnitude;
        }

        _dragStart.position = new Vector3(startDrag.x, startDrag.y, 0);
        _dragEnd.position = new Vector3(endDrag.x, endDrag.y, 0);
        _dragEnd.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, _dragStart.position - _dragEnd.position));
        float dragDist = Vector2.Distance(_dragStart.position, _dragEnd.position);
        _dragLine.LineThickness = dragDist.Remap(_config.DragDistMinMax.x, _config.DragDistMinMax.y, _config.DragLineMinMaxWidth.y, _config.DragLineMinMaxWidth.x);
        _dragIndicationParent.SetActive(true);
    }


    public void HideDragIndication(bool animated)
    {
        if(!animated)
            _dragIndicationParent.SetActive(false);
        else
            StartCoroutine(HideDragIndicationAnimated());
    }
    
    // Do quick animations of circles snapping together after release
    private IEnumerator HideDragIndicationAnimated()
    {
        var startPos = _dragEnd.position;
        float timer = 0;
        while(timer <= _config.DragIndicationSnapDur)
        {
            _dragEnd.position = Vector2.Lerp(startPos, _dragStart.position, _config.DragIndicationSnap.Evaluate(timer.Remap(0, _config.DragIndicationSnapDur, 0, 1)));
            float dragDist = Vector2.Distance(_dragStart.position, _dragEnd.position);
            _dragLine.LineThickness = dragDist.Remap(_config.DragDistMinMax.x, _config.DragDistMinMax.y, _config.DragLineMinMaxWidth.y, _config.DragLineMinMaxWidth.x);

            timer += Time.deltaTime;

            yield return null;
        }
        _dragIndicationParent.SetActive(false);

        //TODO: only after animation done should shoot
    }
}