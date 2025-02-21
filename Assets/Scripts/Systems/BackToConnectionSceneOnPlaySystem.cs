#if UNITY_EDITOR
using Unity.Entities;
using UnityEngine.SceneManagement;

public partial class BackToConnectionSceneOnPlaySystem : SystemBase {
    protected override void OnCreate() {
        base.OnCreate();
        Enabled = false;
        
        // if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName(SceneNameHelper.ConnectionScene)) return;
        if (SceneManager.GetActiveScene().name != "GameScene") return;
        
        SceneManager.LoadSceneAsync(SceneNameHelper.ConnectionScene);
    }

    protected override void OnUpdate() { }
}
#endif
