using System;

namespace GUI
{
    public static class FormationHelper
    {
        public static (int, int) GetBasePosition(Player player, int pitchLeft, int pitchRight, int pitchTop, int pitchBottom, bool attackingRight)
        {
            int centerX = (pitchLeft + pitchRight) / 2;
            int centerY = (pitchTop + pitchBottom) / 2;
            if (player.Position.Contains("GK"))
                return (pitchLeft + 1, centerY);
            if (player.Position.Contains("CB") || player.Position.Contains("LB") || player.Position.Contains("RB"))
                return (pitchLeft + (pitchRight - pitchLeft) / 4, centerY);
            if (player.Position.Contains("CM") || player.Position.Contains("CAM") || player.Position.Contains("LM") || player.Position.Contains("RM"))
                return (centerX, centerY);
            if (player.Position.Contains("ST"))
                return (pitchRight - 2, centerY);
            return (centerX, centerY);
        }
    }
}
