using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using MMP;
using NAudio.Dsp;

namespace MMP {
    public class WaveListenerProvider : IWaveProvider {

        private WaveFormat waveFormat;
        private WaveStream sourceStream;
        private SampleAggregator sampleAggregator = new SampleAggregator(8192);

        public float Bass = 0f;

        public WaveListenerProvider(WaveStream sourceStream) {
            this.sourceStream = sourceStream;
            waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sourceStream.WaveFormat.SampleRate, sourceStream.WaveFormat.Channels);

            sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            sampleAggregator.PerformFFT = true;
        }

        private void FftCalculated(object sender, FftEventArgs e) {
            Bass = 0f;
            Complex[] res = e.Result;
            for(int i = 20; i < 60; i++) {
                Bass += res[i].X;
                Bass /= 2;
            }
        }

        public WaveFormat WaveFormat {
            get {
                return waveFormat;
            }
        }

        public int Read(byte[] buffer, int offset, int count) {
            int samples = sourceStream.Read(buffer, offset, count);
            for(int i = 0; i < count; i += offset) {
                float sample = BitConverter.ToSingle(buffer, i);
                sampleAggregator.Add(sample);
            }
            return samples;
        }
    }
}
