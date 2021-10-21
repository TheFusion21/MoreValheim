using System;
using UnityEngine;

namespace MoreValheimInterface
{
    public class Flag : MonoBehaviour, Hoverable, Interactable
    {
        public string m_name = "door";
        public EffectList m_openEffects = new EffectList();
        public EffectList m_closeEffects = new EffectList();
        public EffectList m_lockedEffects = new EffectList();
        private ZNetView m_nview;
        private Animator m_animator;

        private void Awake()
        {
            this.m_nview = this.GetComponent<ZNetView>();
            if (this.m_nview.GetZDO() == null)
                return;
            this.m_animator = this.GetComponent<Animator>();
            if ((bool)(UnityEngine.Object)this.m_nview)
                this.m_nview.Register<bool>("UseFlag", new Action<long, bool>(this.RPC_UseFlag));
            this.InvokeRepeating("UpdateState", 0.0f, 0.2f);
        }

        private void UpdateState()
        {
            if (!this.m_nview.IsValid())
                return;
            this.SetState(this.m_nview.GetZDO().GetInt("state"));
        }
        private void SetState(int state)
        {
            if (this.m_animator.GetInteger(nameof(state)) == state)
                return;
            if (state != 0)
                this.m_openEffects.Create(this.transform.position, this.transform.rotation);
            else
                this.m_closeEffects.Create(this.transform.position, this.transform.rotation);
            this.m_animator.SetInteger(nameof(state), state);
        }
        public string GetHoverName() => this.m_name;

        public string GetHoverText()
        {
            if (!this.m_nview.IsValid())
                return "";
            if (!PrivateArea.CheckAccess(this.transform.position, flash: false))
                return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
            if (!this.CanInteract())
                return Localization.instance.Localize(this.m_name);

            return this.m_nview.GetZDO().GetInt("state") != 0 ? Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_flag_down") : Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_flag_up");
        }

        public bool Interact(Humanoid character, bool hold, bool alt)
        {
            if (hold || !this.CanInteract())
                return false;
            if (!PrivateArea.CheckAccess(this.transform.position))
                return true;
            this.m_nview.InvokeRPC("UseFlag", (object)((double)Vector3.Dot(this.transform.forward, (character.transform.position - this.transform.position).normalized) < 0.0));
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

        private bool CanInteract()
        {
            return !m_animator.IsInTransition(0);
        }

        private void RPC_UseFlag(long uid, bool forward)
        {
            if (!this.CanInteract())
                return;
            if (this.m_nview.GetZDO().GetInt("state") == 0)
                this.m_nview.GetZDO().Set("state", 1);
            else
                this.m_nview.GetZDO().Set("state", 0);
            this.UpdateState();
        }
    }
}
