using Nanover.Frontend.UI;
using UnityEngine;

public class ActivateFlotingUIFollower : MonoBehaviour
{
    [SerializeField]
    private FollowingUi follower;

    void Awake()
    {
        follower.enabled = true;
    }
}
