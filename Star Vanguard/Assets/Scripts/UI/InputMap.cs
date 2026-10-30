namespace StarVanguard.UI
{
    /// <summary>
    /// The single place where keys are written down.
    ///
    /// The Controls screen renders straight from this table, so the manual can never
    /// drift away from what the game actually listens to.
    /// </summary>
    public static class InputMap
    {
        public readonly struct Row
        {
            public readonly string Action;
            public readonly string Keyboard;
            public readonly string Mouse;

            public Row(string i_Action, string i_Keyboard, string i_Mouse)
            {
                Action = i_Action;
                Keyboard = i_Keyboard;
                Mouse = i_Mouse;
            }
        }

        public static readonly Row[] Rows =
        {
            new Row("MOVE SHIP", "WASD / ARROWS", "Mouse (follow)"),
            new Row("FIRE CANNON", "SPACE", "Left Click (hold)"),
            new Row("PLASMA BOMB", "LEFT SHIFT", "Right Click"),
            new Row("SWITCH WEAPON", "1  2  3  4", "Collect gift boxes"),
            new Row("PAUSE / RESUME", "ESC", "-"),
            new Row("CONFIRM", "ENTER", "Left Click"),
            new Row("GO BACK", "ESC / BACKSPACE", "-"),
        };
    }
}
