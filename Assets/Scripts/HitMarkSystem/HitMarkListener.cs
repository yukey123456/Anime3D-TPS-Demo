using Invector;
using Invector.vEventSystems;
using UnityEngine;

public class HitMarkListener : MonoBehaviour
{
    public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker, bool isDead)
    {
        if (attacker == null)
            return;

        if (attacker.gameObject.TryGetComponent(out HitMarkController hitMarkController))
        {
            hitMarkController.PlayOnHit();

            if (isDead)
            {
                hitMarkController.PlayOnKill();
            }
        }
    }
}
