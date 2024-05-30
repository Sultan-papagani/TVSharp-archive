using System;
 
namespace TVSharp
{
    public class SettingsMemoryEntry
    {
        private decimal _frequency;
        private int _samplerate;
        private bool _programAgc;
        private bool _rtlAgc;
        private bool _tunerAgc;
        private bool _autoPositionCorrection;
        private float _detectorLevel;
        private int _frequencyCorrection;
        private int _contrast;
        private int _brightnes;
        private long[] _palSecamChannelFrequency;
        private long[] _ntscChannelFequency;
        private bool _inverseVideo;
        private int _dongleNr;
 
        public SettingsMemoryEntry() { }
 
        public SettingsMemoryEntry(SettingsMemoryEntry memoryEntry)
        {
            _programAgc = memoryEntry._programAgc;
            _rtlAgc = memoryEntry._rtlAgc;
            _tunerAgc = memoryEntry._tunerAgc;
            _autoPositionCorrection = memoryEntry._autoPositionCorrection;
            _frequencyCorrection = memoryEntry._frequencyCorrection;
            _samplerate = memoryEntry._samplerate;
            _frequency = memoryEntry._frequency;
            _detectorLevel = memoryEntry._detectorLevel;
            _brightnes = memoryEntry._brightnes;
            _contrast = memoryEntry._contrast;
            _palSecamChannelFrequency = memoryEntry._palSecamChannelFrequency;
            _ntscChannelFequency = memoryEntry._ntscChannelFequency;
            _inverseVideo = memoryEntry._inverseVideo;
            _dongleNr = memoryEntry._dongleNr;
        }
 
        public int Brightnes
        {
            get { return _brightnes; }
            set { _brightnes = value; }
        }
        
        public bool AutoPositionCorrecion
        {
            get { return _autoPositionCorrection; }
            set { _autoPositionCorrection = value; }
        }
 
        public long[] PalSecamChannelFrequency
        {
            get { return _palSecamChannelFrequency; }
            set { _palSecamChannelFrequency = value; }
        }
 
        public long[] ntscChannelFrequency
        {
            get { return _ntscChannelFequency; }
            set { _ntscChannelFequency = value; }
        }
 
        public bool ProgramAgc
        {
            get { return _programAgc; }
            set { _programAgc = value; }
        }
 
        public bool TunerAgc
        {
            get { return _tunerAgc; }
            set { _tunerAgc = value; }
        }
 
        public bool RtlAgc
        {
            get { return _rtlAgc; }
            set { _rtlAgc = value; }
        }
 
        public int FrequencyCorrection
        {
            get { return _frequencyCorrection; }
            set { _frequencyCorrection = value; }
        }
 
        public decimal Frequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }
 
        public int Samplerate
        {
            get { return _samplerate; }
            set { _samplerate = value; }
        }
 
        public float DetectorLevel
        {
            get { return _detectorLevel; }
            set { _detectorLevel = value; }
        }
 
        public int Contrast
        {
            get { return _contrast; }
            set { _contrast = value; }
        }
 
        public bool InverseVideo
        {
            get { return _inverseVideo; }
            set { _inverseVideo = value; }
        }
 
        public int DongleNr
        {
            get { return _dongleNr; }
            set { _dongleNr = value; }
        }
    }
}