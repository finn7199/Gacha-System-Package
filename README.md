# Gacha System Unity Package

A customizable gacha system for Unity, perfect for any project needing a gacha mechanic. This system provides flexible pull options, rarity configurations, and planned support for future enhancements.

---

## 	Features

### Current Features
- **Custom Pull Counts**: Perform pulls of any size, from single pulls to multi pulls (e.g., 1, 2, 10, ... n).
- **Configurable Pity System**: Configure thresholds and drop rates directly from the Inspector.
- **Scriptable Objects for Items**: Add gacha items like characters or weapons with minimal effort.
- **Custom Rarity Support (New!)**  
  Define your **own rarities** by creating new Rarity ScriptableObjects:
  - Set custom names, drop rates, and order.
  - No need to modify the core code — just create and configure your rarities via the Inspector!
- Support for dynamic curves to adjust drop rates.
- **Live rate preview**: the GachaManager Inspector shows a table of every rarity's actual pull chance, so you can see your real odds without doing any math.
- **Tier assist**: each Rarity asset shows every tier used across the project so you can't accidentally leave two rarities colliding on the same number, plus a one-click "use next free tier" button. The GachaManager Inspector can also renumber tiers to match your list order.

---

## How Drop Rates Work

Each rarity's **Drop Rate** is checked **from rarest to most common**, one coin flip at a time — it is *not* a slice of a 100% pie, and your rarities do **not** need to add up to 100%.

For example, with R = 90%, SR = 9%, SSR = 1%:
1. First, the SSR check runs: 1% chance to stop here.
2. If SSR didn't hit, the SR check runs: 9% chance to stop here.
3. If neither hit, you get R.

Your **most common rarity** (the one with the lowest Tier number) doesn't get its own check at all — it's simply whatever's left after every rarer rarity's check fails. You can leave its Drop Rate slider at any value; it has no effect. The GachaManager Inspector's rate table shows you the *actual* chance each rarity has once this rolls out (SSR 1%, SR 8.91%, R 90.09% in the example above) so you never have to compute it by hand.

---

## Rarity Tiers

Only the **relative order** of `Tier` matters — 1/2/3 behaves identically to 10/20/30, as long as the numbers are in the right order. But leaving two rarities on the same Tier (including the default of 0) isn't harmless: it also makes those two rarities reset each other's Pity every time either one is pulled, and if every rarity is left at the same Tier, Pity never triggers at all.

Each Rarity asset's Inspector shows every Tier already used across your project, so you can spot collisions and pick a free number without guessing. If you'd rather not type numbers at all, drag your rarities into order (most common at the top) in the GachaManager's Available Rarities list, then click **Renumber Tiers from List Order**.

---

### Upcoming Features
1. **Firebase Integration**:
   - Store and manage gacha results, user pulls, and token data securely in the cloud.
2. **Inspector-Friendly Enhancements**:
   - Dedicated Unity Editor tools for setting up gacha configurations faster.

---

## How to Use

### Setup Instructions

### Download the Package
- Find the latest releases under the [Releases](https://github.com/finn7199/Gacha-System-Package/releases) section.
- Import the downloaded package into your Unity project.

### Add the Gacha System Script

-   Create an empty GameObject in your scene and name it **GachaManager**.
-   Attach the `GachaSystem.cs` script to this GameObject.

### Create Gacha Items

-   Use **Scriptable Objects** to define your gacha items:
    1.  Right-click in the Project window.
    2.  Go to **Create > Gacha System > Gacha Item**.
    3.  Fill out the fields (e.g., item name & rarity) in the Inspector.

### (V1.1.0) Create Custom Rarities

- Right-click in the Project window.
- Go to **Create > Gacha System > Gacha Rarity**.
- Define custom rarity names and drop rates.
- Add new rarities to the system without modifying the core code!
  
### Add Items and Rarities to the Pool

-   In the **GachaManager**, add all your created Gacha Items and Rarities to the list in the Inspector.

### Perform Pulls

-   Use the following method to perform gacha pulls:    
    `GachaSystem.Instance.PerformPull(n);` 
    
    -   Replace `n` with the number of pulls (e.g., `1` for a single pull, `10` for a batch pull).
    -   The method returns a list of pulled **GachaItem** objects.

----------

### Example: Button Click

-   To trigger pulls from a button click:
    1.  Add a UI Button to your scene.
    2.  Add a script with a method that calls `GachaSystem.Instance.PerformPull(n)` and link it to the button's **OnClick()** event.
    3.  Set the number of pulls for that button.
 

----------
## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.

----------
## Contributions & Feedback

Found a bug? Have a feature suggestion?  
Feel free to open an [Issue](https://github.com/finn7199/Gacha-System-Package/issues) or submit a [Pull Request](https://github.com/finn7199/Gacha-System-Package/pulls)!
