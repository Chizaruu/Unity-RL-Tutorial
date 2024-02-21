sealed class Staff : Equippable
{
 public Staff()
 {
  EquipmentType = EquipmentType.Weapon;
  MagicBonus = 3;
 }

 private void OnValidate()
 {
  if (gameObject.transform.parent)
  {
   gameObject.transform.parent.GetComponent<Equipment>().Weapon = this;
  }
 }
}