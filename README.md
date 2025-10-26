# auto-click-tool

### Overview

This is a tool to automate clicking. Record mouse coordinates with F8 key and start clicking with Start button.
You can save the recorded cordinates in TXT file by Save button and restore it by Open button.

![image](https://github.com/user-attachments/assets/32c6715f-2395-44ad-bab8-6aa01c9eea9a)


### Controls

The following table lists the main UI controls in the application and a short description of their purpose.

| Control | Description |
|---|---|
| Default Click Interval | Input for the default click interval in milliseconds. Applied when recording coordinates with the record action. |
| Save | Save the recorded coordinates to a text file. |
| Open | Load recorded coordinates from a text file into the coordinates area. |
| Check | Shows a blinking pseudo-cursor at the position specified on the currently selected line in the coordinates area for a visual preview. |
| Reset Check | Removes the pseudo-cursor preview from the screen. |
| Start | Starts the automated click sequence using the list of `X, Y, Interval` lines from the coordinates area. |
| Coordinates List | The main multi-line text area where each line should be formatted as `X, Y, Interval`. The caret position determines which line the Preview uses. Use the record shortcut to append the current mouse position. |

### Keyboard shortcuts

- `F8`: Records the current mouse position and appends a line to the `Coordinates List` using the value from `Default Click Interval` as the interval.
- `Escape`: Stops the running auto-clicker and returns to Edit Mode.

### Notes

- The pseudo-cursor preview is displayed as a small blinking indicator on screen and is DPI-aware to work correctly on high-DPI displays.
- Coordinate lines must be numeric and separated by commas. Invalid lines are ignored by the auto-clicker.

