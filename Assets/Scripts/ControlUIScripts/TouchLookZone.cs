using UnityEngine;
using UnityEngine.EventSystems;

public class TouchLookZone : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    // これをプレイヤー側のスクリプトから読み取る
    public Vector2 TouchDelta { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 触れた瞬間にDeltaをリセット
        TouchDelta = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // EventSystemが、このUIを触っている指の「移動量」を自動で計算してくれる
        TouchDelta = eventData.delta;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 離したら入力を止める
        TouchDelta = Vector2.zero;
    }
}