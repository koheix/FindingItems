[System.Serializable]
public class SaveData
{
    public string nowStageName = "Stage1_1";
    // 前のステージの名前を保存する変数
    public string previousStageName = "Stage1_1";
    
    // ステージをまたいでのライフ
    public int remainingLives = 3;
    public int score = 0;
    public int coinNum = 0;
}