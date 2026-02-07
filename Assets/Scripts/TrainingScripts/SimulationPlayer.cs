using UnityEngine;

public class SimulationPlayer : MonoBehaviour {
    public Transform target; // 入手する目的のアイテム
    public float assistPower = 10f; // 引き寄せの強さ(高いほどプレイヤーがナビを追いやすいことを示す)
    public bool isGuiding = false; // NaviAgentが操作するフラグ(光が出ていることを示す)
    private Rigidbody rb; //PlayerのRigidbody

    void Start() => rb = GetComponent<Rigidbody>(); //Player自身のRigidbody

    void FixedUpdate() {
        if (isGuiding) {
            Vector3 dir = (target.position - transform.position).normalized;
            rb.AddForce(dir * assistPower);
        }
    }
}