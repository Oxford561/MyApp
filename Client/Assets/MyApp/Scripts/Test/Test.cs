using UnityEngine;

public class Test : MonoBehaviour
{

    void Start()
    {
        Debug.Log(DevicePerformanceUtil.GetDevicePerformanceLevel());
        //根据设备配置自动设置质量
        DevicePerformanceUtil.ModifySettingsBasedOnPerformance();
        //手动设置质量
        //DevicePerformanceUtil.SetQualitySettings(QualityLevel.Low);
    }
}