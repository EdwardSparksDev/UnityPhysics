using Cinemachine;
using UnityEngine;


public class TargetGroupHandler : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    CinemachineTargetGroup targetGroup;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        targetGroup = GetComponent<CinemachineTargetGroup>();
    }


    private void OnEnable()
    {
        EventManager.AddTargetToCameraGroup += OnAddTargetToCameraGroup;
    }


    private void OnDisable()
    {
        EventManager.AddTargetToCameraGroup -= OnAddTargetToCameraGroup;
    }
    #endregion


    #region Methods
    /// <summary>
    /// Adds target to camera target group
    /// </summary>
    /// <param name="target">The target to be added</param>
    private void OnAddTargetToCameraGroup(Transform target)
    {
        targetGroup.AddMember(target, 1f, 1f);
    }
    #endregion
}
