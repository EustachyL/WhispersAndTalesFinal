using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhispersAndTales.Services
{
    public interface ISpeechToText
    {
        Task<bool> RequestPermissions();

        Task<string> Listen(CultureInfo culture,
            IProgress<string> recognitionResult,
            CancellationToken cancellationToken);
        Task StartContinuousListeningAsync(CultureInfo culture, IProgress<string> recognitionResult, CancellationToken cancellationToken);
    }
}
