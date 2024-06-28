

using UnityEngine;

namespace Sprimate.Feedback.Services
{
    /// <summary>
    /// Used to implement some sort of waveform playback such as a tone generator or vibration motor.
    /// </summary>
    public abstract class AWaveformService : MonoBehaviour
    {
        /// <summary>
        /// Plays a waveform. Amplitude is between 0 and 100
        /// </summary>
        /// <param name="durationSeconds"></param>
        /// <param name="frequency"></param>
        /// <param name="amplitudePercent">0 to 100</param>
        public abstract void ExecuteWave(double durationSeconds, double frequency, uint amplitudePercent);
    }
}
