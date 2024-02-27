using System;
using System.Collections.Generic;
using System.Linq;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio
{
    internal static class ALHelper
    {
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        internal static void CheckError(string message = "", params object[] args)
        {
            ALError error;
            if ((error = AL.GetError()) != ALError.NoError)
            {
                if (args != null && args.Length > 0)
                    message = String.Format(message, args);
                
                throw new InvalidOperationException(message + " (Reason: " + AL.GetErrorString(error) + ")");
            }
        }

        public static bool IsStereoFormat(ALFormat format)
        {
            return (format == ALFormat.Stereo8
                || format == ALFormat.Stereo16
                || format == ALFormat.StereoFloat32
                || format == ALFormat.StereoIma4
                || format == ALFormat.StereoMSAdpcm);
        }
    }

    internal static class AlcHelper
    {
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        internal static void CheckError(string message = "", params object[] args)
        {
            AlcError error;
            if ((error = Alc.GetError()) != AlcError.NoError)
            {
                if (args != null && args.Length > 0)
                    message = String.Format(message, args);

                throw new InvalidOperationException(message + " (Reason: " + error.ToString() + ")");
            }
        }
    }

    internal sealed class OpenALSoundController : IDisposable
    {
        private static OpenALSoundController _instance = null;
        private static EffectsExtension _efx = null;
        private IntPtr _device;
        private IntPtr _context;
        IntPtr NullContext = IntPtr.Zero;
        private int[] allSourcesArray;

        // MacOS & Linux shares a limit of 256.
        internal const int MAX_NUMBER_OF_SOURCES = 256;

        private static OggStreamer _oggstreamer;

        private List<int> availableSourcesCollection;
        private List<int> inUseSourcesCollection;
        bool _isDisposed;
        public bool SupportsIma4 { get; private set; }
        public bool SupportsAdpcm { get; private set; }
        public bool SupportsEfx { get; private set; }
        public bool SupportsIeee { get; private set; }

        /// <summary>
        /// Sets up the hardware resources used by the controller.
        /// </summary>
		private OpenALSoundController()
        {
            if (AL.NativeLibrary == IntPtr.Zero)
                throw new DllNotFoundException("Couldn't initialize OpenAL because the native binaries couldn't be found.");

            if (!OpenSoundController())
            {
                throw new NoAudioHardwareException("OpenAL device could not be initialized, see console output for details.");
            }

            if (Alc.IsExtensionPresent(_device, "ALC_EXT_CAPTURE"))
                Microphone.PopulateCaptureDevices();

            // We have hardware here and it is ready

			allSourcesArray = new int[MAX_NUMBER_OF_SOURCES];
			AL.GenSources(allSourcesArray);
            ALHelper.CheckError("Failed to generate sources.");
            Filter = 0;
            if (Efx.IsInitialized)
            {
                Filter = Efx.GenFilter();
            }
            availableSourcesCollection = new List<int>(allSourcesArray);
			inUseSourcesCollection = new List<int>();
		}

        ~OpenALSoundController()
        {
            Dispose(false);
        }

        /// <summary>
        /// Open the sound device, sets up an audio context, and makes the new context
        /// the current context. Note that this method will stop the playback of
        /// music that was running prior to the game start. If any error occurs, then
        /// the state of the controller is reset.
        /// </summary>
        /// <returns>True if the sound controller was setup, and false if not.</returns>
        private bool OpenSoundController()
        {
            try
            {
                _device = Alc.OpenDevice(string.Empty);
                EffectsExtension.device = _device;
            }
            catch (Exception ex)
            {
                throw new NoAudioHardwareException("OpenAL device could not be initialized.", ex);
            }

            AlcHelper.CheckError("Could not open OpenAL device");

            if (_device != IntPtr.Zero)
            {
                int[] attribute = new int[0];

                _context = Alc.CreateContext(_device, attribute);
                _oggstreamer = new OggStreamer();

                AlcHelper.CheckError("Could not create OpenAL context");

                if (_context != NullContext)
                {
                    Alc.MakeContextCurrent(_context);
                    AlcHelper.CheckError("Could not make OpenAL context current");
                    SupportsIma4 = AL.IsExtensionPresent("AL_EXT_IMA4");
                    SupportsAdpcm = AL.IsExtensionPresent("AL_SOFT_MSADPCM");
                    SupportsEfx = AL.IsExtensionPresent("AL_EXT_EFX");
                    SupportsIeee = AL.IsExtensionPresent("AL_EXT_float32");
                    return true;
                }
            }
            return false;
        }

        public static void EnsureInitialized()
        {
            if (_instance == null)
            {
                try
                {
                    _instance = new OpenALSoundController();
                }
                catch (DllNotFoundException)
                {
                    throw;
                }
                catch (NoAudioHardwareException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw (new NoAudioHardwareException("Failed to init OpenALSoundController", ex));
                }
            }
        }


        public static OpenALSoundController Instance
        {
			get
            {
                if (_instance == null)
                    throw new NoAudioHardwareException("OpenAL context has failed to initialize. Call SoundEffect.Initialize() before sound operation to get more specific errors.");
				return _instance;
			}
		}

        public static EffectsExtension Efx
        {
            get
            {
                if (_efx == null)
                    _efx = new EffectsExtension();
                return _efx;
            }
        }

        public int Filter
        {
            get; private set;
        }

        public static void DestroyInstance()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
        }

        /// <summary>
        /// Destroys the AL context and closes the device, when they exist.
        /// </summary>
        private void CleanUpOpenAL()
        {
            Alc.MakeContextCurrent(NullContext);

            if (_context != NullContext)
            {
                Alc.DestroyContext (_context);
                _context = NullContext;
            }
            if (_device != IntPtr.Zero)
            {
                Alc.CloseDevice (_device);
                _device = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Dispose of the OpenALSoundCOntroller.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the OpenALSoundCOntroller.
        /// </summary>
        /// <param name="disposing">If true, the managed resources are to be disposed.</param>
		void Dispose(bool disposing)
		{
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if(_oggstreamer != null)
                        _oggstreamer.Dispose();
                    for (int i = 0; i < allSourcesArray.Length; i++)
                    {
                        AL.DeleteSource(allSourcesArray[i]);
                        ALHelper.CheckError("Failed to delete source.");
                    }

                    if (Filter != 0 && Efx.IsInitialized)
                        Efx.DeleteFilter(Filter);

                    Microphone.StopMicrophones();
                    CleanUpOpenAL();                    
                }
                _isDisposed = true;
            }
		}

        /// <summary>
        /// Reserves a sound buffer and return its identifier. If there are no available sources
        /// or the controller was not able to setup the hardware then an
        /// <see cref="InstancePlayLimitException"/> is thrown.
        /// </summary>
        /// <returns>The source number of the reserved sound buffer.</returns>
		public int ReserveSource()
		{
            int sourceNumber;

            lock (availableSourcesCollection)
            {                
                if (availableSourcesCollection.Count == 0)
                {
                    throw new InstancePlayLimitException();
                }

                sourceNumber = availableSourcesCollection.Last();
                inUseSourcesCollection.Add(sourceNumber);
                availableSourcesCollection.Remove(sourceNumber);
            }

            return sourceNumber;
		}

        public void RecycleSource(int sourceId)
		{
            lock (availableSourcesCollection)
            {
                inUseSourcesCollection.Remove(sourceId);
                availableSourcesCollection.Add(sourceId);
            }
		}

        public void FreeSource(SoundEffectInstance inst)
        {
            RecycleSource(inst.SourceId);
            inst.SourceId = 0;
            inst.HasSourceId = false;
            inst.SoundState = SoundState.Stopped;
		}

        public int SourceCurrentPosition (int sourceId)
		{
            int pos;
			AL.GetSource (sourceId, ALGetSourcei.SampleOffset, out pos);
            ALHelper.CheckError("Failed to set source offset.");
			return pos;
		}
    }
}
