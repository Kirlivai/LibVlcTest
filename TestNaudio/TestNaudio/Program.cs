using LibVLCSharp.Shared;
using NAudio.Wave;
using System.Runtime.InteropServices;
using System.Threading.Channels;

bool _threadInprogress = true;
WaveFormat _sourceFormat;
WaveFormat _targetFormat;
WaveStream _sourceStream;
WaveStream _targetStream;
LibVLC _libVLC;
string _url = ""; // Coming from config


_libVLC = new LibVLC(enableDebugLogs: false);
_sourceFormat = new WaveFormat(48000, 16, 1);
_targetFormat = new WaveFormat(8000, 16, 1);

using (var media = new Media(_libVLC, new Uri($"rtp://{_url}"), ":no-video"))
{
    using (var mediaPlayer = new MediaPlayer(media))
    {
        mediaPlayer.SetAudioFormatCallback(AudioSetup, AudioCleanup);
        mediaPlayer.SetAudioCallbacks(PlayAudio, null, null, FlushAudio, null);

        mediaPlayer.Play();

        while (_threadInprogress)
        {
            Console.WriteLine("In progress");
            Thread.Sleep(60000);
        }
    }
}

void PlayAudio(IntPtr data, IntPtr samples, uint count, long pts)
{
    try
    {

        var buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame


        int bytes = (int)count * 2; // (16 bit, 1 channel)
                                    //var buffer = new byte[bytes];
        Marshal.Copy(samples, buffer, 0, bytes);

        if (_sourceFormat.SampleRate == _targetFormat.SampleRate)
        {
            //_waveProvider.AddSamples(buffer, 0, bytes);
        }
        else
        {
            var memoryStream = new MemoryStream(buffer, 0, bytes);
            _sourceStream = new RawSourceWaveStream(memoryStream, _sourceFormat);
            _targetStream = new WaveFormatConversionStream(_targetFormat, _sourceStream);

            byte[] bufferAudioConverted = new byte[_targetStream.Length];
            _targetStream.Read(bufferAudioConverted, 0, bufferAudioConverted.Length);

            //_waveProvider.AddSamples(bufferAudioConverted, 0, bufferAudioConverted.Length);
        }

    }
    catch (Exception ex)
    {

    }
}

void FlushAudio(IntPtr data, long pts)
{
    //_waveProvider.ClearBuffer();
}
int AudioSetup(ref IntPtr opaque, ref IntPtr format, ref uint rate, ref uint channels)
{
    return 0;
}
void AudioCleanup(IntPtr opaque) { }

 WaveFormat ReturnWaveFormat(Mp3Frame frame)
{
    WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
        frame.FrameLength, frame.BitRate);

    return waveFormat;
}
