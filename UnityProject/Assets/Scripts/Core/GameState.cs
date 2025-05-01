using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodexBrokenZodiac
{
    /// <summary>
    /// Defines the possible states of the game
    /// </summary>
    public enum GameState
    {
        MainMenu,       // Main menu screen
        Loading,        // Loading screen
        Playing,        // Active gameplay
        Paused,         // Game paused
        Inventory,      // Inventory/equipment screen
        CardSelection,  // Tarot card selection
        GameOver,       // Player died
        Victory,        // Level completed
        Cutscene,       // Cutscene playing
        Options,        // Options menu
        Credits,        // Credits screen
        Multiplayer     // Multiplayer lobby/connection
    }
    
    /// <summary>
    /// Static class to manage the current game state
    /// </summary>
    public static class GameStateManager
    {
        // Current and previous game states
        private static GameState currentState = GameState.MainMenu;
        private static GameState previousState = GameState.MainMenu;
        
        // Events
        public delegate void GameStateChangedHandler(GameState newState, GameState oldState);
        public static event GameStateChangedHandler OnGameStateChanged;
        
        /// <summary>
        /// Get the current game state
        /// </summary>
        public static GameState GetCurrentState()
        {
            return currentState;
        }
        
        /// <summary>
        /// Get the previous game state
        /// </summary>
        public static GameState GetPreviousState()
        {
            return previousState;
        }
        
        /// <summary>
        /// Set the current game state
        /// </summary>
        public static void SetState(GameState newState)
        {
            // Don't change if the state is the same
            if (newState == currentState)
                return;
            
            // Store previous state
            previousState = currentState;
            
            // Set new state
            currentState = newState;
            
            // Invoke event
            OnGameStateChanged?.Invoke(currentState, previousState);
            
            // Log state change
            Debug.Log($"Game state changed from {previousState} to {currentState}");
        }
        
        /// <summary>
        /// Return to the previous game state
        /// </summary>
        public static void ReturnToPreviousState()
        {
            SetState(previousState);
        }
        
        /// <summary>
        /// Check if the current state is within a list of states
        /// </summary>
        public static bool IsInState(params GameState[] states)
        {
            foreach (GameState state in states)
            {
                if (currentState == state)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if the game is in a playable state (not in a menu or cutscene)
        /// </summary>
        public static bool IsGameplayActive()
        {
            return currentState == GameState.Playing;
        }
        
        /// <summary>
        /// Check if player input should be processed
        /// </summary>
        public static bool ShouldProcessPlayerInput()
        {
            return currentState == GameState.Playing || 
                   currentState == GameState.Inventory || 
                   currentState == GameState.CardSelection;
        }
    }
}