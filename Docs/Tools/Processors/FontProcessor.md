# FontProcessor

FontProcessor converts any installed font to a game ready bitmap font package for the specified font sizes.

## Input File Types

.ttf .otf

*NOTE: Font must be installed on your system first. FontProcessor will query the name of the font from your font cache and build the output.*

## Output File Type

.fontpkg

*NOTE: FontProcessor also outputs intermediate output for each font size selected.*  
*A .png file of the generated bitmap and a .font descriptor file for the bitmap. These files are provided as a convenience and can be deleted before deployment.*

## Program Arguments

Get a full list by running `FontProcessor --help`

## File Structure

```
- .fontpkg header  
    - Number of fonts in package
    - Array of sizes of each .font in the package

- Array of .font fonts in package, ascending order by size
    - .font header
        - Font size
        - Number of characters
        - Letter spacing
        - Texture size
        - Array of per character data
            - Character
            - Size in texture atlas
            - Position in texture atlas
    - font bitmap data (raw 32-bit rgb)
    - ...
```
