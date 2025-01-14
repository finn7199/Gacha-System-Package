# Gacha System Unity Package

A customizable gacha system for Unity, perfect for any project needing a gacha mechanic. This system provides flexible pull options, rarity configurations, and planned support for future enhancements.

---

## Features

### Current Features
- **Custom Pull Counts**: Perform pulls of any size, from single pulls to multi pulls (e.g., 1, 2, 10, ... n).
- **Three Rarities**: 
  - **R** (rare)
  - **SR** (super rare)
  - **SSR** (super+ rare)  
- **Configurable Pity System**:
  - SR and SSR pity counters are adjustable via the Unity Inspector:
    - **SR Pity**: Default = every 10 pulls.
    - **SSR Pity**: Default = every 100 pulls.
    - Configure thresholds and drop rates directly from the Inspector.
- **Scriptable Objects for Items**: Add gacha items like characters or weapons with minimal effort.
- Support for SSR rate curves to dynamically adjust drop rates.

---

### Upcoming Features (Planned in Future Updates)
1. **Custom Rarity Support**:
   - Add your own rarity levels beyond R, SR, and SSR with custom drop rates.
2. **Firebase Integration**:
   - Store and manage gacha results, user pulls, and token data securely in the cloud.
3. **Inspector-Friendly Enhancements**:
   - Dedicated Unity Editor tools for setting up gacha configurations faster.

---

## How to Use

### Setup Instructions

### 1. Download the Package
- Find the latest releases under the [Releases](https://github.com/finn7199/Gacha-System-Package/releases) section.
- Import the downloaded package into your Unity project.

### 2. Add the Gacha System Script

-   Create an empty GameObject in your scene and name it **GachaManager**.
-   Attach the `GachaSystem.cs` script to this GameObject.

### 3. Configure the Gacha System

-   Select the **GachaManager** GameObject and set up the following in the Unity Inspector:
    -   **Base Drop Rates**: Define the drop rates for R, SR, and SSR rarities.
    -   **Pity Thresholds**: Configure the SR and SSR pity counts (e.g., 10 pulls for SR and 100 pulls for SSR).
    -   **SSR Rate Curve**: Adjust the curve to set how the SSR drop rate increases as pity progresses.

### 4. Create Gacha Items

-   Use **Scriptable Objects** to define your gacha items:
    1.  Right-click in the Project window.
    2.  Go to **Create > Gacha System > Gacha Item**.
    3.  Fill out the fields (e.g., item name & rarity) in the Inspector.

### 5. Add Gacha Items to the Pool

-   In the **GachaManager**, add all your created Gacha Items to the **Gacha Pool** list in the Inspector.

### 6. Perform Pulls

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
## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.
