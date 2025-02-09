using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhispersAndTales.Services
{
    public class SpeechToTextStub : ISpeechToText
    {
        public Task<bool> RequestPermissions()
        {
            throw new NotSupportedException("Speech-to-text is not supported on this platform.");
        }

        public Task<string> Listen(CultureInfo culture, IProgress<string> recognitionResult, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Speech-to-text is not supported on this platform.");
        }

        public Task StartContinuousListeningAsync(CultureInfo culture, IProgress<string> recognitionResult, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
