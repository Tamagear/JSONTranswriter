# JSONTranswriter
A JSON transwriter for CHROMA Card Game Prototyping.

Author: [Tim](https://github.com/Tamagear) | January 2023

*©2023 Daggerbunny Studios*


| Features                    									|
| ----------------------------------------------------------------------------------------------|
| Dynamically create, delete and edit cards for the CHROMA database within the GUI   	|
| Generate Sprite Atlas' with placeholder images, ready to be imported into the [Tabletop Simulator](https://store.steampowered.com/app/286160/Tabletop_Simulator/)   	|


## Configuration
1. Download the newest [Release](https://github.com/Tamagear/JSONTranswriter/releases) *(.zip File)*.
2. Extract the .zip File into a directory of your choice.

## Usage
1. Open `GUI/ChromaJSONEditor.exe`
2. Have fun!

## How it works
We use a simple JSON file to store our card information in. By reading from and writing to this file, the user can alter the JSON file.
Upon generation, a template is applied and colorized according to the card information. For each card, a bing search is started where the first fitting image is set as a placeholder image for the specific card. In the end, all cards are replicated according to their quantity on the sprite atlas, which is generated (face and back) within a folder on your desktop.

## Dependencies
### GUI / C#
- `Newtonsoft.Json`

### Image Generation / Python
- `tkinter`
- `shutil`
- `PyTorch`
- `bing_image_downloader`
- `playsound (Version 1.2.2)`

## License
Licensed via [GNU GPL 3.0](https://www.gnu.org/licenses/gpl-3.0), but commercial use is strongly prohibited. Any rights to the names, projects and affiliations of the authors are to be preserved. Images generated or found by this program are probably not owned by the author and belong to their respective owners.

## Potential Future Updates
- Export as .csv / .xlsx
- Online Access

-----

![Official Artwork - Lin & Kris](https://cdn.discordapp.com/attachments/923610318893121606/1068646804285042718/bg.png)
*Lin & Kris - The mascots of the JSONTranswriter.*
