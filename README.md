# auto-click-tool

### Overview

AutoClickTool automates mouse clicks using a recorded list of coordinates and per-item intervals. Use the UI to record points, preview positions, and run or stop the automatic click loop. Save and load coordinate lists as text files.

<img width="516" height="446" alt="image" src="https://github.com/user-attachments/assets/16486ef5-30e4-48b9-823f-76ab34ab1e0a" />

### Controls

The following table lists the main UI controls and a short description of their purpose.

| Control | Description |
|---|---|
| `Default Click Interval` | Default interval in milliseconds used when appending points while not in Record mode. |
| `Open` | Load a saved coordinates text file into the `Coordinates List`. |
| `Save` | Save the contents of the `Coordinates List` to a text file. |
| `Add Point (F8)` | Append the current mouse position to the `Coordinates List`. When recording, the appended interval is the elapsed time since the last recorded point; otherwise the `Default Click Interval` is used. |
| `Record` | Toggle Record mode. In Record mode the app measures elapsed time between recorded points and uses that as each point's interval. The timer shows seconds since the last record. |
| `Start` | Start the automated clicking sequence using the `Coordinates List` (each line must be `X, Y, Interval`). |
| `Stop (Esc)` | Stop the automated clicking sequence or exit Record mode. Equivalent to pressing `Escape`. |
| `Check` | Show a pseudo-cursor at the coordinates on the caret's current line for visual verification. |
| `Reset Check` | Remove the pseudo-cursor preview. |
| `Coordinates List` | Multi-line text area where each line is `X, Y, Interval`. The caret determines which line `Check` previews. |

### Keyboard shortcuts

- `F8`: Append current mouse position to the `Coordinates List`. If in Record mode, the appended interval is time elapsed since the previous recorded point; otherwise the `Default Click Interval` is used.
- `Escape`: Stop the running auto-clicker or exit Record mode.
- `Ctrl+Shift+C`: Equivalent to clicking `Check` (shows pseudo-cursor for the current line).
- `Ctrl+Shift+R`: Equivalent to clicking `Reset Check` (remove pseudo-cursor).

### Notes

- The pseudo-cursor preview is a small visual indicator; it is DPI-aware for high-DPI displays.
- Coordinate lines must contain numeric values separated by commas. Invalid or malformed lines are ignored by the auto-clicker.


