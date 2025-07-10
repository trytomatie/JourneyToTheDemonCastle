using UnityEngine;

public class GUI_MadraCore : MonoBehaviour
{


    public void UpdateMadraValues(Vector4 madraInfo)
    {
        float madraPercentage = madraInfo.x / madraInfo.y;
        float madraCorePercentage = madraInfo.z / madraInfo.w;
        MadraCoreVFXController.Instance.coreFillPercentage = madraCorePercentage;
        MadraCoreVFXController.Instance.madraFillPercentage = madraPercentage;
        MadraCoreVFXController.Instance.UpdateParticleSystem();
    }
    public void ShowMadraCoreUI(bool show)
    {
    }
}