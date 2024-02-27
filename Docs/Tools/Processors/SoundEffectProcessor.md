# SoundEffectProcessor

The SoundEffectProcessor converts mono and stereo 8, 16, 24 or 32-bit .wav files to 16-bit audio files in either mono or stereo formats for the audio subsystem.

## Input File Types

.wav

## Output File Type

.sound

## Program Arguments

-c: Force channel count. 1 or 2. Number of channels for the output sound. Use to convert between mono and stereo.  
    If omitted, the channel count will not change and no conversion will be performed  
-x: Compress data. Not implemented. Reserved for future development 

*NOTE: Sounds used as sound effects in game should be converted to mono `-c 1` to be used correctly with the 3d positional audio system.*  
*.sound files used for game music can be either channel layout.*

*NOTE: The sample rate of the input .wav file is preserved in the output.*

### Example Usage

SoundEffectProcessor \<input-file> \<output-file> \<arguments>

The following will convert a .wav file to a mono .sound file

SoundEffectProcessor stereo-input.wav mono-output.sound -c 1

## Output File Format

A .sound file consists of a basic header, followed by 16-bit sample data

### File Structure

- .sound header
    - Sample rate
    - Channel count
    - Bit rate. Reserved for future use. Currently all .sound files are 16-bit
    - Compression flag. Reserved for future use.
    - Null byte. Reserved.

- Array of 16-bit audio sample values