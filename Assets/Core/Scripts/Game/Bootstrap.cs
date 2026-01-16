using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null;
        
        // Загрузка сцены с явным режимом Single для правильного применения освещения
        var asyncLoad = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = true;
        
        yield return asyncLoad;
        
        // Принудительно обновляем освещение после загрузки
        DynamicGI.UpdateEnvironment();
    }
}