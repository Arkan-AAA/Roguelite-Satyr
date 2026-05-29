using System;
using Combat;
using UnityEngine;

public class Sword : MeleeWeapon {
    [SerializeField]
    private int _damageAmount = 10;

    public event EventHandler OnSwordSwing;

    private void Start() {
        damageAmount = _damageAmount;
    }

    public override void Attack() {
        base.Attack();
        OnSwordSwing?.Invoke(this, EventArgs.Empty);
    }

    public void AttackColliderTurnOff() {
        if (hitboxCollider != null)
            hitboxCollider.enabled = false;
    }
}