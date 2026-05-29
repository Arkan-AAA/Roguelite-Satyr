using System.Collections.Generic;
using UnityEngine;
using Combat;

public class WeaponInventory : MonoBehaviour {
    [SerializeField] private List<Weapon> weapons = new List<Weapon>();
    private int currentIndex = 0;

    public Weapon CurrentWeapon => weapons.Count > currentIndex ? weapons[currentIndex] : null;

    // Публичные методы для доступа из UI
    public List<Weapon> GetWeapons() => weapons;
    public int GetCurrentIndex() => currentIndex;
    public Weapon GetCurrentWeapon() => CurrentWeapon;

    private void Start() {
        if (weapons.Count > 0)
            EquipWeapon(0);
    }

    public void AddWeapon(Weapon weapon) {
        if (!weapons.Contains(weapon)) {
            weapons.Add(weapon);
            weapon.gameObject.SetActive(false);
            if (weapons.Count == 1)
                EquipWeapon(0);
        }
    }

    public void EquipWeapon(int index) {
        if (index < 0 || index >= weapons.Count) return;

        if (CurrentWeapon != null)
            CurrentWeapon.gameObject.SetActive(false);

        currentIndex = index;

        if (CurrentWeapon != null)
            CurrentWeapon.gameObject.SetActive(true);

        ActiveWeapon.Instance?.SetWeapon(CurrentWeapon);
    }

    public void NextWeapon() {
        if (weapons.Count == 0) return;
        EquipWeapon((currentIndex + 1) % weapons.Count);
    }

    public void PreviousWeapon() {
        if (weapons.Count == 0) return;
        EquipWeapon((currentIndex - 1 + weapons.Count) % weapons.Count);
    }
}