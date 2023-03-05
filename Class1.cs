using ThunderRoad;
using UnityEngine;

namespace Whetstone
{
    public class WhetstoneModule : ItemModule
    {
        public string effectId;
        public float sharpenDamper;
        public float sharpenDamperIn;
        public float sharpenDamperOut;
        public float sharpenDismemberVelocity;
        public float sharpenDismemberAngle;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<WhetstoneComponent>().Setup(effectId, sharpenDamper, sharpenDamperIn, sharpenDamperOut, sharpenDismemberVelocity, sharpenDismemberAngle);
        }
    }
    public class WhetstoneComponent : MonoBehaviour
    {
        Item item;
        Collider stone;
        ColliderGroup stoneColliderGroup;
        public bool sharpening = false;
        public string effectId = "Sharpen";
        public float sharpenDamper;
        public float sharpenDamperIn;
        public float sharpenDamperOut;
        public float sharpenDismemberVelocity;
        public float sharpenDismemberAngle;
        public void Start()
        {
            item = GetComponent<Item>();
            stone = item.GetCustomReference("Stone").GetComponent<Collider>();
            stoneColliderGroup = stone.GetComponentInParent<ColliderGroup>();
        }
        public void OnCollisionStay(Collision c)
        {
            if (c.collider.GetComponentInParent<ColliderGroup>() is ColliderGroup group && group.collisionHandler?.item != null && c.GetContact(0).thisCollider == stone)
            {
                bool canSharpen = false;
                if (sharpening)
                {
                    if (item.rb.velocity.magnitude < group.collisionHandler.rb.velocity.magnitude + 0.5 && group.collisionHandler.rb.velocity.magnitude < item.rb.velocity.magnitude + 0.5)
                    {
                        sharpening = false;
                    }
                    else return;
                }
                else if (item.rb.velocity.magnitude >= group.collisionHandler.rb.velocity.magnitude + 2 || group.collisionHandler.rb.velocity.magnitude >= item.rb.velocity.magnitude + 2)
                {
                    sharpening = true;
                    foreach (Damager damager in group.collisionHandler.damagers)
                    {
                        if (damager.colliderGroup == group && damager.data.damageModifierData.damageType != DamageType.Blunt)
                        {
                            damager.data.penetrationDamper -= sharpenDamper;
                            if (damager.data.penetrationDamper <= 0) damager.data.penetrationDamper = 0;
                            damager.data.penetrationHeldDamperIn -= sharpenDamperIn;
                            if (damager.data.penetrationHeldDamperIn <= 0) damager.data.penetrationHeldDamperIn = 0;
                            damager.data.penetrationHeldDamperOut -= sharpenDamperOut;
                            if (damager.data.penetrationHeldDamperOut <= 0) damager.data.penetrationHeldDamperOut = 0;
                            if (damager.data.dismembermentAllowed)
                            {
                                damager.data.dismembermentMinVelocity -= sharpenDismemberVelocity;
                                if (damager.data.dismembermentMinVelocity <= 0) damager.data.dismembermentMinVelocity = 0;
                                damager.data.GetTier(damager.collisionHandler).dismembermentMaxHorizontalAngle += sharpenDismemberAngle;
                                if (damager.data.GetTier(damager.collisionHandler).dismembermentMaxHorizontalAngle >= 90) damager.data.GetTier(damager.collisionHandler).dismembermentMaxHorizontalAngle = 90;
                                damager.data.GetTier(damager.collisionHandler).dismembermentMaxVerticalAngle += sharpenDismemberAngle;
                                if (damager.data.GetTier(damager.collisionHandler).dismembermentMaxVerticalAngle >= 90) damager.data.GetTier(damager.collisionHandler).dismembermentMaxVerticalAngle = 90;
                            }
                            canSharpen = true;
                        }
                    }
                    if (stoneColliderGroup.imbue?.spellCastBase != null && group.modifier?.imbueType != ColliderGroupData.ImbueType.None)
                    {
                        group.imbue.Transfer(stoneColliderGroup.imbue.spellCastBase, group.imbue.maxEnergy / 10);
                        canSharpen = true;
                    }
                    if (canSharpen)
                    {
                        EffectInstance instance = Catalog.GetData<EffectData>(effectId).Spawn(item.transform, true);
                        instance.SetIntensity(1f);
                        instance.Play();
                    }
                }
            }
        }
        public void Setup(string id, float damper, float damperIn, float damperOut, float dismember, float angle)
        {
            effectId = id;
            sharpenDamper = damper;
            sharpenDamperIn = damperIn;
            sharpenDamperOut = damperOut;
            sharpenDismemberVelocity = dismember;
            sharpenDismemberAngle = angle;
        }
    }
}
