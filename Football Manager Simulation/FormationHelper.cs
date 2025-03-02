using System;

namespace GUI
{
    // Provides helper methods for formation and positioning logic.
    // For example, it can calculate a base position for a player
    // on the pitch based on their role and the pitch dimensions.
    public static class FormationHelper
    {
        // Calculates a base position for a player using their position type and pitch boundaries.
        // This is used as a starting point for movement during a match.
        // For goalkeepers, it can return a fixed position near the goal.
        public static (int, int) GetBasePosition(Player player, int pitchLeft, int pitchRight, int pitchTop, int pitchBottom, bool attackingRight)
        {
            int centerX = (pitchLeft + pitchRight) / 2;
            int centerY = (pitchTop + pitchBottom) / 2;

            // If the player is a goalkeeper, return a fixed position.
            if (player.Position.Contains("GK"))
            {
                // For example, place GK near the left or right goal based on attacking side.
                return attackingRight ? (pitchLeft + 1, centerY) : (pitchRight - 1, centerY);
            }
            // For defenders, position them to the left (if Team A) or right (if Team B) of center.
            if (player.Position.Contains("CB") || player.Position.Contains("LB") || player.Position.Contains("RB"))
            {
                return attackingRight ? (pitchLeft + (pitchRight - pitchLeft) / 4, centerY) : (pitchLeft + 3 * (pitchRight - pitchLeft) / 4, centerY);
            }
            // For midfielders, place at or around center.
            if (player.Position.Contains("CM") || player.Position.Contains("CAM") || player.Position.Contains("LM") || player.Position.Contains("RM"))
            {
                return (centerX, centerY);
            }
            // For strikers, position them near the opponent's goal.
            if (player.Position.Contains("ST"))
            {
                return attackingRight ? (pitchRight - 2, centerY) : (pitchLeft + 2, centerY);
            }
            // Default: return center.
            return (centerX, centerY);
        }
    }
}
