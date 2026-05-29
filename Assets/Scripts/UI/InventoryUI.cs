using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Combat;

public class InventoryUI : MonoBehaviour {
    [Header("Weapon Slots")]
    public Image[] weaponSlots;
    public GameObject[] weaponBorders;

    [Header("Weapon Info")]
    public TMP_Text weaponNameText;
    public TMP_Text weaponDamageText;

    private WeaponInventory weaponInventory;

    private void Start() {
        // Исправленный устаревший метод
        weaponInventory = FindAnyObjectByType<WeaponInventory>();
        UpdateUI();
    }

    private void OnEnable() {
        UpdateUI();
    }

    public void UpdateUI() {
        if (weaponInventory == null) return;

        // Получаем оружия через публичное свойство или метод
        var weapons = GetWeaponsList();
        int currentIndex = GetCurrentIndex();

        // Обновляем слоты оружия
        for (int i = 0; i < weaponSlots.Length; i++) {
            if (i < weapons.Count && weapons[i] != null) {
                var sr = weapons[i].GetComponent<SpriteRenderer>();
                if (sr != null && weaponSlots[i] != null) {
                    weaponSlots[i].sprite = sr.sprite;
                    weaponSlots[i].color = Color.white;
                }
            }
            else {
                if (weaponSlots[i] != null) {
                    weaponSlots[i].sprite = null;
                    weaponSlots[i].color = Color.clear;
                }
            }

            // Рамка активного оружия
            if (weaponBorders.Length > i && weaponBorders[i] != null) {
                weaponBorders[i].SetActive(i == currentIndex);
            }
        }

        // Информация об активном оружии
        var current = weaponInventory.GetCurrentWeapon();
        if (current != null && weaponNameText != null) {
            weaponNameText.text = current.name;
        }
    }

    private List<Weapon> GetWeaponsList() {
        // Используем рефлексию для доступа к приватному полю
        var field = typeof(WeaponInventory).GetField("weapons",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null) {
            return field.GetValue(weaponInventory) as List<Weapon>;
        }
        return new List<Weapon>();
    }

    private int GetCurrentIndex() {
        var field = typeof(WeaponInventory).GetField("currentIndex",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null) {
            return (int)field.GetValue(weaponInventory);
        }
        return 0;
    }

    public void OnWeaponSlotClick(int slotIndex) {
        weaponInventory?.EquipWeapon(slotIndex);
        UpdateUI();
    }
}