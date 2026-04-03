# ♟️ C# Checkers & Minimax AI 

**Status:** Work In Progress / Algorithmic Sandbox  
**Tech Stack:** C#, .NET 10, WPF, MVVM Architecture

## 📖 Overview
This repository is an active sandbox for exploring board state logic, algorithmic decision-making, and eventually implementing a Minimax AI. 

Like my larger engine projects, the architecture strictly follows the **MVVM (Model-View-ViewModel)** design pattern. I have completely decoupled the mathematical game rules (`G00DS0ULCHECKERS.Model`) from the visual interface (`G00DS0ULCHECKERS.WPFUI`).

## 🎯 Current Focus
* Structuring board state evaluation and persistent turn logic.
* Decoupling UI event handlers from backend memory states.
* Researching decision-tree algorithms for the computer AI module.

## 🎮 How to Run
This project relies on a decoupled MVVM architecture, meaning the UI and backend logic are split into separate class libraries.

1. Clone the repository and open `G00DS0ULCHECKERS.slnx` in Visual Studio.
2. Ensure you have the **.NET Desktop Development** workload installed.
3. In the Solution Explorer, right-click the **`WPFUI`** project and select **"Set as Startup Project"**.
4. Press `F5` to build the solution and launch the interactive Checkers board.
