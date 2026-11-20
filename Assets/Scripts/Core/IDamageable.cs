namespace SurvivalGame.Core
{
    public interface IDamageable
    {
        void TakeDamage(float damage, UnityEngine.Vector3 hitPoint, UnityEngine.Vector3 hitNormal);
    }
}
