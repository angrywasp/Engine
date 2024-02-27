using EmulatorCore;
using NesEmulator;

namespace EngineScripting
{
    /// <summary>
    /// Scripts for the emulators
    /// </summary>
    public static class Emu
    {
#if NES
        /// <summary>
        /// Loads a ROM.
        /// </summary>
        /// <param name="path">The relative content path to load a ROM from</param>
        public static void LoadRom(string path)
        {
            ((Emulator)EmulatorGame.Instance).LoadRom(path);
        }
#endif

#if C64
        
#endif
    }
}