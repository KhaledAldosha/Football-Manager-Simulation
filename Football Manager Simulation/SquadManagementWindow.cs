using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace GUI
{
    public class SquadManagementWindow : Window
    {
        private Club _userTeam;
        private List<Formation> _formations;
        private int _activeFormationIndex;
        private Formation _selectedFormation;
        private Dictionary<int, Player> _positionAssignments;
        private int _activePositionIndex;
        private Player selectedPlayer;

        // Tactic selection fields.
        private List<string> _tactics = new List<string> { "Gegenpress", "Tikitaka", "Park the Bus" };
        private int _selectedTacticIndex = 0;
        private bool _inTacticSelection = false;

        // Mode flags.
        private bool _inFormationSelection = true;
        private bool _inStarting11Selection = false;

        public SquadManagementWindow(Club userTeam, string title, Rectangle rectangle, bool visible)
            : base(title, rectangle, visible)
        {
            _userTeam = userTeam;
            if (string.IsNullOrEmpty(_userTeam.SelectedTactic))
                _userTeam.SelectedTactic = _tactics[_selectedTacticIndex];

            InitializeFormations();
            _activeFormationIndex = 0;
            _selectedFormation = _formations[_activeFormationIndex];
            _positionAssignments = _userTeam.PositionAssignments ?? new Dictionary<int, Player>();
            _activePositionIndex = 0;
            selectedPlayer = null;
        }

        private void InitializeFormations()
        {
            _formations = new List<Formation>
            {
                // "4-3-3": GK, LB, RB, CB, CB, CM, CAM, CM, RW, ST, LW.
                new Formation("4-3-3", new List<FormationPosition>
                {
                    new FormationPosition("GK", 50, 20),
                    new FormationPosition("LB", 10, 40),
                    new FormationPosition("RB", 90, 40),
                    new FormationPosition("CB", 30, 40),
                    new FormationPosition("CB", 70, 40),
                    new FormationPosition("CM", 30, 60),
                    new FormationPosition("CAM", 50, 60),
                    new FormationPosition("CM", 70, 60),
                    new FormationPosition("RW", 90, 60),
                    new FormationPosition("ST", 50, 80),
                    new FormationPosition("LW", 10, 60)
                }),
                // "4-4-2": GK, LB, CB, CB, RB, LM, CM, CM, RM, ST, ST.
                new Formation("4-4-2", new List<FormationPosition>
                {
                    new FormationPosition("GK", 50, 20),
                    new FormationPosition("LB", 10, 40),
                    new FormationPosition("CB", 30, 40),
                    new FormationPosition("CB", 70, 40),
                    new FormationPosition("RB", 90, 40),
                    new FormationPosition("LM", 10, 60),
                    new FormationPosition("CM", 40, 60),
                    new FormationPosition("CM", 60, 60),
                    new FormationPosition("RM", 90, 60),
                    new FormationPosition("ST", 40, 80),
                    new FormationPosition("ST", 60, 80)
                }),
                // "4-3-2-1": GK, LB, CB, CB, RB, CM, CM, CM, CAM, CAM, ST.
                new Formation("4-3-2-1", new List<FormationPosition>
                {
                    new FormationPosition("GK", 50, 20),
                    new FormationPosition("LB", 10, 40),
                    new FormationPosition("CB", 30, 40),
                    new FormationPosition("CB", 70, 40),
                    new FormationPosition("RB", 90, 40),
                    new FormationPosition("CM", 30, 60),
                    new FormationPosition("CM", 50, 60),
                    new FormationPosition("CM", 70, 60),
                    new FormationPosition("CAM", 30, 80),
                    new FormationPosition("CAM", 70, 80),
                    new FormationPosition("ST", 50, 90)
                })
            };
        }

        public override void Update()
        {
            var key = UserInterface.Input.Key;
            if (key == ConsoleKey.Escape)
            {
                _currentAction = InterfaceAction.ReturnToMainMenu;
                return;
            }
            if (key == ConsoleKey.R)
            {
                _inTacticSelection = !_inTacticSelection;
                return;
            }
            if (_inTacticSelection)
            {
                UpdateTacticSelection(key);
                return;
            }
            else
            {
                if (_inFormationSelection)
                    UpdateFormationSelection(key);
                else if (_inStarting11Selection)
                    UpdateStarting11Selection(key);
            }
        }

        private void UpdateTacticSelection(ConsoleKey key)
        {
            if (key == ConsoleKey.UpArrow)
                _selectedTacticIndex = (_selectedTacticIndex - 1 + _tactics.Count) % _tactics.Count;
            else if (key == ConsoleKey.DownArrow)
                _selectedTacticIndex = (_selectedTacticIndex + 1) % _tactics.Count;
            else if (key == ConsoleKey.Enter)
            {
                _userTeam.SelectedTactic = _tactics[_selectedTacticIndex];
                _inTacticSelection = false;
            }
            else if (key == ConsoleKey.LeftArrow)
                _inTacticSelection = false;
        }

        private void UpdateFormationSelection(ConsoleKey key)
        {
            if (key == ConsoleKey.UpArrow)
                _activeFormationIndex = (_activeFormationIndex - 1 + _formations.Count) % _formations.Count;
            else if (key == ConsoleKey.DownArrow)
                _activeFormationIndex = (_activeFormationIndex + 1) % _formations.Count;
            else if (key == ConsoleKey.F)
            {
                _activeFormationIndex = (_activeFormationIndex + 1) % _formations.Count;
                _selectedFormation = _formations[_activeFormationIndex];
                _userTeam.SelectedFormation = _selectedFormation;
            }
            else if (key == ConsoleKey.Enter)
            {
                _selectedFormation = _formations[_activeFormationIndex];
                _userTeam.SelectedFormation = _selectedFormation;
                _positionAssignments = new Dictionary<int, Player>();
                _userTeam.PositionAssignments = _positionAssignments;
                _activePositionIndex = 0;
            }
            else if (key == ConsoleKey.RightArrow)
            {
                if (_selectedFormation.Positions.Count > 0)
                {
                    _inFormationSelection = false;
                    _inStarting11Selection = true;
                    selectedPlayer = _positionAssignments.ContainsKey(_activePositionIndex) ? _positionAssignments[_activePositionIndex] : null;
                }
            }
        }

        private void UpdateStarting11Selection(ConsoleKey key)
        {
            if (key == ConsoleKey.UpArrow)
            {
                _activePositionIndex = (_activePositionIndex - 1 + _selectedFormation.Positions.Count) % _selectedFormation.Positions.Count;
                selectedPlayer = _positionAssignments.ContainsKey(_activePositionIndex) ? _positionAssignments[_activePositionIndex] : null;
            }
            else if (key == ConsoleKey.DownArrow)
            {
                _activePositionIndex = (_activePositionIndex + 1) % _selectedFormation.Positions.Count;
                selectedPlayer = _positionAssignments.ContainsKey(_activePositionIndex) ? _positionAssignments[_activePositionIndex] : null;
            }
            else if (key == ConsoleKey.Enter)
            {
                string pos = _selectedFormation.Positions[_activePositionIndex].PositionName;
                Player chosen = SelectPlayerForPosition(pos);
                if (chosen != null)
                {
                    _positionAssignments[_activePositionIndex] = chosen;
                    _userTeam.PositionAssignments = _positionAssignments;
                    selectedPlayer = chosen;
                }
            }
            else if (key == ConsoleKey.X)
            {
                AutoAssignPlayers();
                _activePositionIndex = 0;
                selectedPlayer = _positionAssignments.ContainsKey(0) ? _positionAssignments[0] : null;
            }
            else if (key == ConsoleKey.LeftArrow)
            {
                _inStarting11Selection = false;
                _inFormationSelection = true;
            }
        }

        public override void Draw(bool active)
        {
            if (!_visible)
                return;
            ClearWindowArea();
            base.Draw(active);
            DrawFormationInfo();
            DrawTacticInfo();
            DrawFormationSelectionSection();
            DrawStarting11Section();
            DrawPlayerInformationSection();
            DrawInstructionsSection();
        }

        private void ClearWindowArea()
        {
            for (int y = _rectangle.Y; y < _rectangle.Y + _rectangle.Height; y++)
            {
                Console.SetCursorPosition(_rectangle.X, y);
                Console.Write(new string(' ', _rectangle.Width));
            }
        }

        private void DrawFormationInfo()
        {
            int x = _rectangle.X + 2, y = _rectangle.Y + 1;
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Formation: {_selectedFormation.Name}");
            Console.ResetColor();
        }

        private void DrawTacticInfo()
        {
            int x = _rectangle.X + _rectangle.Width - 30, y = _rectangle.Y + 1;
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Tactic: {_userTeam.SelectedTactic}");
            Console.ResetColor();
        }

        private void DrawFormationSelectionSection()
        {
            int x = _rectangle.X + 2, y = _rectangle.Y + 3;
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Formations:");
            Console.ResetColor();
            y++;
            foreach (var formation in _formations)
            {
                Console.SetCursorPosition(x, y);
                if (_inFormationSelection && _formations.IndexOf(formation) == _activeFormationIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"> {formation.Name}");
                }
                else
                {
                    Console.WriteLine($"  {formation.Name}");
                }
                Console.ResetColor();
                y++;
            }
        }

        private void DrawStarting11Section()
        {
            int x = _rectangle.X + 2, y = _rectangle.Y + 10;
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Starting 11:");
            Console.ResetColor();
            y++;
            for (int i = 0; i < _selectedFormation.Positions.Count; i++)
            {
                Console.SetCursorPosition(x, y + i);
                string posName = _selectedFormation.Positions[i].PositionName;
                string playerName = _positionAssignments.ContainsKey(i) ? _positionAssignments[i].Name : "Empty";
                string display = $"{posName}: {playerName}";
                if (_inStarting11Selection && i == _activePositionIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"> {display}");
                }
                else
                {
                    Console.WriteLine($"  {display}");
                }
                Console.ResetColor();
            }
        }

        private void DrawPlayerInformationSection()
        {
            if (selectedPlayer == null)
                return;
            int x = _rectangle.X + 40, y = _rectangle.Y + 3;
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Player Information:");
            Console.ResetColor();
            y++;
            Console.SetCursorPosition(x, y++);
            Console.WriteLine($"Name: {selectedPlayer.Name}");
            Console.SetCursorPosition(x, y++);
            Console.WriteLine($"Age: {selectedPlayer.Age}");
            Console.SetCursorPosition(x, y++);
            Console.WriteLine($"Position: {selectedPlayer.Position}");
            Console.SetCursorPosition(x, y++);
            Console.WriteLine($"Rating: {selectedPlayer.Rating}");
            Console.SetCursorPosition(x, y++);
            Console.WriteLine($"Potential: {selectedPlayer.Potential}");
            Console.SetCursorPosition(x, y++);
            Console.WriteLine($"Value: £{selectedPlayer.Value / 1000000}M");
            Console.SetCursorPosition(x, y++);
            Console.WriteLine($"Wage: £{selectedPlayer.Wage}K / Week");
            Console.SetCursorPosition(x, y);
            Console.WriteLine($"Contract: {selectedPlayer.ContractLength} yrs, {selectedPlayer.SquadStatus}");
        }

        private void DrawInstructionsSection()
        {
            int x = _rectangle.X + 2, y = _rectangle.Y + _rectangle.Height - 4;
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Formation Mode: Up/Down = Cycle, F = Next formation, Enter = Confirm, Right = Players");
            Console.SetCursorPosition(x, y + 1);
            Console.WriteLine("Player Assignment: Up/Down = Cycle, Enter = Assign, X = Auto-Assign, Left = Back");
            Console.SetCursorPosition(x, y + 2);
            Console.WriteLine("Tactic Mode: Press R to toggle tactic selection");
            Console.ResetColor();
        }

        private void AutoAssignPlayers()
        {
            for (int i = 0; i < _selectedFormation.Positions.Count; i++)
            {
                if (!_positionAssignments.ContainsKey(i))
                {
                    string pos = _selectedFormation.Positions[i].PositionName;
                    List<Player> available = _userTeam.Players
                        .Where(p => p.Position.Equals(pos, StringComparison.OrdinalIgnoreCase))
                        .Where(p => !_positionAssignments.Values.Contains(p))
                        .ToList();

                    if (available.Count == 0)
                        continue;

                    List<Player> sorted = MergeSortPlayers(available);
                    _positionAssignments[i] = sorted[0];
                }
            }
            _userTeam.PositionAssignments = _positionAssignments;
        }

        private List<Player> MergeSortPlayers(List<Player> players)
        {
            if (players.Count <= 1)
                return players;

            int mid = players.Count / 2;
            List<Player> left = players.GetRange(0, mid);
            List<Player> right = players.GetRange(mid, players.Count - mid);

            left = MergeSortPlayers(left);
            right = MergeSortPlayers(right);

            return Merge(left, right);
        }

        private List<Player> Merge(List<Player> left, List<Player> right)
        {
            List<Player> result = new List<Player>();
            int i = 0, j = 0;
            while (i < left.Count && j < right.Count)
            {
                if (left[i].Rating >= right[j].Rating)
                {
                    result.Add(left[i]);
                    i++;
                }
                else
                {
                    result.Add(right[j]);
                    j++;
                }
            }
            while (i < left.Count)
            {
                result.Add(left[i]);
                i++;
            }
            while (j < right.Count)
            {
                result.Add(right[j]);
                j++;
            }
            return result;
        }

        private Player SelectPlayerForPosition(string position)
        {
            List<Player> available = _userTeam.Players
                .Where(p => p.Position.Equals(position, StringComparison.OrdinalIgnoreCase))
                .Where(p => !_positionAssignments.Values.Contains(p))
                .ToList();

            if (available.Count == 0)
            {
                Console.Clear();
                Console.WriteLine($"No players available for position {position}");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                return null;
            }

            int selIndex = 0;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Select player for position {position}:\n");
                for (int i = 0; i < available.Count; i++)
                {
                    Console.ForegroundColor = (i == selIndex) ? ConsoleColor.Green : ConsoleColor.Gray;
                    Console.WriteLine($"{(i == selIndex ? "> " : "  ")}{available[i].Name} (Rating: {available[i].Rating}, Age: {available[i].Age})");
                }
                Console.ResetColor();
                Console.WriteLine("\nEnter = Select, Up/Down = Navigate, S = Show details, Esc = Cancel");
                var input = Console.ReadKey(true);
                if (input.Key == ConsoleKey.UpArrow)
                    selIndex = (selIndex - 1 + available.Count) % available.Count;
                else if (input.Key == ConsoleKey.DownArrow)
                    selIndex = (selIndex + 1) % available.Count;
                else if (input.Key == ConsoleKey.Enter)
                    return available[selIndex];
                else if (input.Key == ConsoleKey.Escape)
                    return null;
                else if (input.Key == ConsoleKey.S)
                    ShowPlayerDetails(available[selIndex]);
            }
        }

        private void ShowPlayerDetails(Player player)
        {
            Console.Clear();
            Console.WriteLine($"Name: {player.Name}");
            Console.WriteLine($"Age: {player.Age}");
            Console.WriteLine($"Position: {player.Position}");
            Console.WriteLine($"Rating: {player.Rating}");
            Console.WriteLine($"Potential: {player.Potential}");
            Console.WriteLine($"Value: £{player.Value / 1000000}M");
            Console.WriteLine($"Wage: £{player.Wage}K / Week");
            Console.WriteLine($"Contract: {player.ContractLength} yrs, {player.SquadStatus}");
            Console.WriteLine("Statistics:");
            foreach (var stat in player.Statistics)
                Console.WriteLine($"- {stat.Key}: {stat.Value}");
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
        }
    }

    public class Formation
    {
        public string Name { get; }
        public List<FormationPosition> Positions { get; }

        public Formation(string name, List<FormationPosition> positions)
        {
            Name = name;
            Positions = positions;
        }
    }

    public class FormationPosition
    {
        public string PositionName { get; }
        public int XOffset { get; }
        public int YOffset { get; }
        public FormationPosition(string positionName, int xOffset, int yOffset)
        {
            PositionName = positionName;
            XOffset = xOffset;
            YOffset = yOffset;
        }
    }
}
