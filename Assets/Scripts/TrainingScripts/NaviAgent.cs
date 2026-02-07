using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class NaviAgent : Agent {
    public SimulationPlayer player;
    public Transform target;

    public override void OnEpisodeBegin() {
        // 配置をランダムにリセット（学習効率アップのため）
        player.transform.localPosition = new Vector3(Random.Range(-4f, 4f), 1f, Random.Range(-4f, 4f));
        target.localPosition = new Vector3(Random.Range(-4f, 4f), 1f, Random.Range(-4f, 4f));
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(player.transform.localPosition); // プレイヤーの位置
        sensor.AddObservation(target.localPosition);           // ゴールの位置
    }

    public override void OnActionReceived(ActionBuffers actions) {
        // AIの決定：0なら何もしない、1なら誘導の光を出す
        int action = actions.DiscreteActions[0];
        player.isGuiding = (action == 1);

        // 報酬設計
        float dist = Vector3.Distance(player.transform.position, target.position);
        
        if (dist < 1.5f) {
            SetReward(1.0f); // ゴール報酬
            // EndEpisode();
        }

        // 光を出している間は微小なペナルティ（「出しっぱなし」を防ぐ）
        if (player.isGuiding) AddReward(-0.01f);
    }
}