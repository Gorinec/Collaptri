using UnityEngine;

public class MobileOptimization : MonoBehaviour
{
    void Start()
    {
        // Ограничиваем FPS для экономии батареи
        Application.targetFrameRate = 60;

        // Отключаем вертикальную синхронизацию для производительности
        QualitySettings.vSyncCount = 0;

        // Уменьшаем качество для слабых устройств
        QualitySettings.antiAliasing = 0;

        // Оптимизация физики
        Physics2D.velocityIterations = 8;
        Physics2D.positionIterations = 8;

        Debug.Log("Mobile optimization applied");
    }
}