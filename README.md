# 🎲 Gacha System Unity Package

A customizable gacha system for Unity, perfect for any project needing a gacha mechanic. This system provides flexible pull options, rarity configurations, and planned support for future enhancements.

---

## 	✨ Features

### 🛠️ Current Features
- **Custom Pull Counts**: Perform pulls of any size, from single pulls to multi pulls (e.g., 1, 2, 10, ... n).
- **Configurable Pity System**: Configure thresholds and drop rates directly from the Inspector.
- **Scriptable Objects for Items**: Add gacha items like characters or weapons with minimal effort.
- **Custom Rarity Support (New!)**  
  Define your **own rarities** by creating new Rarity ScriptableObjects:
  - Set custom names, drop rates, and order.
  - No need to modify the core code — just create and configure your rarities via the Inspector!
- Support for dynamic curves to adjust drop rates.

---

### 🔮 Upcoming Features
1. **Firebase Integration**:
   - Store and manage gacha results, user pulls, and token data securely in the cloud.
2. **Inspector-Friendly Enhancements**:
   - Dedicated Unity Editor tools for setting up gacha configurations faster.

---

## 🚀 How to Use

### Setup Instructions

### 📦 Download the Package
- Find the latest releases under the [Releases](https://github.com/finn7199/Gacha-System-Package/releases) section.
- Import the downloaded package into your Unity project.

### 🏗️ Add the Gacha System Script

-   Create an empty GameObject in your scene and name it **GachaManager**.
-   Attach the `GachaSystem.cs` script to this GameObject.

### 🧩 Create Gacha Items

-   Use **Scriptable Objects** to define your gacha items:
    1.  Right-click in the Project window.
    2.  Go to **Create > Gacha System > Gacha Item**.
    3.  Fill out the fields (e.g., item name & rarity) in the Inspector.

### 🎨 (V1.1.0) Create Custom Rarities

- Right-click in the Project window.
- Go to **Create > Gacha System > Gacha Rarity**.
- Define custom rarity names and drop rates.
- Add new rarities to the system without modifying the core code!
  
### 🧺 Add Items and Rarities to the Pool

-   In the **GachaManager**, add all your created Gacha Items and Rarities to the list in the Inspector.

### 🎯 Perform Pulls

-   Use the following method to perform gacha pulls:    
    `GachaSystem.Instance.PerformPull(n);` 
    
    -   Replace `n` with the number of pulls (e.g., `1` for a single pull, `10` for a batch pull).
    -   The method returns a list of pulled **GachaItem** objects.

----------

### Example: Button Click

-   To trigger pulls from a button click:
    1.  Add a UI Button to your scene.
    2.  Attach the `GachaTest.cs` script on to UI Manager.
    3.  Attach the script to the Button and link the `PerformTestPull(int n)` method to the button's **OnClick()** event.
    4.  Set the number on pulls for that button.
 

----------
## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.

----------
## 📢 Contributions & Feedback

Found a bug? Have a feature suggestion?  
Feel free to open an [Issue](https://github.com/finn7199/Gacha-System-Package/issues) or submit a [Pull Request](https://github.com/finn7199/Gacha-System-Package/pulls)!
