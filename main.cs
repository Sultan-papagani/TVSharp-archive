using System;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;
using System.Runtime.InteropServices;
 
namespace TVSharp
{
    public unsafe partial class MainForm : Form
    {
        private readonly RtlSdrIO _rtlDevice = new RtlSdrIO();
        private bool _isDecoding;
        private bool _initialized;
        private byte[] _grayScaleValues;
        private byte[] _videoWindowArray;
        private int _detectLevel;
        private float _detectorLevelCoef;
        private int _detectImpulsePeriod;
        private int _x;
        private int _y;
        private int _pictureWidth;
        private int _pictureHeight;
        private int _correctX;
        private int _correctY;
        private int _autoCorrectX;
        private int _autoCorrectY;
        private int _counterAutoFreqCorrect;
        private bool _autoPositionCorrect;
        private bool _autoFrequencyCorrection;
        private int _fineFrequencyCorrection;
        private int _fineTick;
        private int _pictureCentr;
        private float _countryCoeff;
        private int _lineInFrame;
        private int _pixelCounter;
        private int _maxSignalLevel;
        private int _agcSignalLevel;
        private int _blackLevel;
        private int _bright;
        private float _contrast;
        private float _coeff;
        private double _sampleRate;
        private int _frequencyCorrection;
        private SettingsMemoryEntry _settings;
        private readonly SettingsPersister _settingsPersister = new SettingsPersister();
        private VideoWindow videoWindow;
        private bool _inverseVideo;
        private bool _bufferIsFull;
        static object locker = new object();
 
        public MainForm()
        {
            InitializeComponent();
            videoWindow = new VideoWindow();
            _settings = _settingsPersister.ReadSettings();
            frequencyNumericUpDown_ValueChanged(null, null);
            try
            {
                var devices = DeviceDisplay.GetActiveDevices();
                deviceComboBox.Items.Clear();
                deviceComboBox.Items.AddRange(devices);
 
                _rtlDevice.Open();
 
                //_initialized = true;
                if (_settings.DongleNr != 0)
                {
                    deviceComboBox.SelectedIndex = _settings.DongleNr;
                }
                else
                {
                    deviceComboBox.SelectedIndex = 0;
                }
                deviceComboBox_SelectedIndexChanged(null, null);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
 
        #region GUI Controls
 
        private void startBtn_Click(object sender, EventArgs e)
        {
            if (!_isDecoding)
            {
                StartDecoding();
            }
            else
            {
                StopDecoding();
            }
            startBtn.Text = _isDecoding ? "Stop" : "Start";
            deviceComboBox.Enabled = !_rtlDevice.Device.IsStreaming;
        }
 
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isDecoding)
            {
                StopDecoding();
            }
 
            _settings.Contrast = contrastTrackBar.Value;
            _settings.Brightnes = brightnesTrackBar.Value;
            _settings.Frequency = (decimal)frequencyNumericUpDown.Value;
            _settings.FrequencyCorrection = (int)frequencyCorrectionNumericUpDown.Value;
            _settings.Samplerate = samplerateComboBox.SelectedIndex;
            _settings.ProgramAgc = programAgcCheckBox.Checked;
            _settings.TunerAgc = tunerAgcCheckBox.Checked;
            _settings.RtlAgc = rtlAgcCheckBox.Checked;
            _settings.DetectorLevel = _detectorLevelCoef;
            _settings.AutoPositionCorrecion = _autoPositionCorrect;
            _settings.InverseVideo = _inverseVideo;
            _settings.DongleNr = deviceComboBox.SelectedIndex;
 
            _settingsPersister.PersistSettings(_settings);
        }
 
        private void deviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (!_initialized)
            //{
            //    return;
            //}
            var deviceDisplay = (DeviceDisplay)deviceComboBox.SelectedItem;
            if (deviceDisplay != null)
            {
                try
                {
                    _rtlDevice.SelectDevice(deviceDisplay.Index);
                    _rtlDevice.Frequency = (uint)100000000;
                    _rtlDevice.Device.Samplerate = 2000000;
                    _rtlDevice.Device.UseRtlAGC = false;
                    _rtlDevice.Device.UseTunerAGC = false;
                    _initialized = true;
                }
                catch (Exception ex)
                {
                    deviceComboBox.SelectedIndex = -1;
                    _initialized = false;
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                ConfigureDevice();
                ConfigureGUI();
            }
        }
 
        private void tunerGainTrackBar_Scroll(object sender, EventArgs e)
        {
            if (!_initialized)
            {
                return;
            }
            var gain = _rtlDevice.Device.SupportedGains[tunerGainTrackBar.Value];
            _rtlDevice.Device.TunerGain = gain;
            gainLabel.Text = gain / 10.0 + " dB";
        }
 
        private void frequencyCorrectionNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (!_initialized)
            {
                return;
            }
            _rtlDevice.Device.FrequencyCorrection = (int)frequencyCorrectionNumericUpDown.Value;
            frequencyCorrectionTrackBar.Value = (int)frequencyCorrectionNumericUpDown.Value;
        }
 
        #endregion
 
        #region Private Methods
 
        private void ConfigureGUI()
        {
            startBtn.Enabled = _initialized;
            if (!_initialized)
            {
                return;
            }
            videoWindow.Visible = true;
            
            tunerTypeLabel.Text = _rtlDevice.Device.TunerType.ToString();
            tunerGainTrackBar.Maximum = _rtlDevice.Device.SupportedGains.Length - 1;
            tunerGainTrackBar.Value = tunerGainTrackBar.Maximum;
            if (_settings.Samplerate < 0) _settings.Samplerate = 2;
            samplerateComboBox.SelectedIndex = _settings.Samplerate;
            samplerateComboBox_SelectedIndexChanged(null, null);
            brightnesTrackBar.Value = _settings.Brightnes;
            brightnesTrackBar_Scroll(null, null);
            contrastTrackBar.Value = _settings.Contrast;
            contrastTrackBar_Scroll(null, null);
            frequencyCorrectionNumericUpDown.Value = _settings.FrequencyCorrection;
            frequencyCorrectionNumericUpDown_ValueChanged(null, null);
            frequencyNumericUpDown.Value = _settings.Frequency;
            frequencyNumericUpDown_ValueChanged(null, null);
            rtlAgcCheckBox.Checked = _settings.RtlAgc;
            rtlAgcCheckBox_CheckedChanged(null, null);
            tunerAgcCheckBox.Checked = _settings.TunerAgc;
            tunerAgcCheckBox_CheckedChanged(null, null);
            programAgcCheckBox.Checked = _settings.ProgramAgc;
            programAgcCheckBox_CheckedChanged(null, null);
            autoSincCheckBox.Checked = _settings.AutoPositionCorrecion;
            autoSincCheckBox_CheckedChanged(null, null);
            inverseCheckBox.Checked = _settings.InverseVideo;
            inverseCheckBox_CheckedChanged(null, null);
 
            for (var i = 0; i < deviceComboBox.Items.Count; i++)
            {
                var deviceDisplay = (DeviceDisplay)deviceComboBox.Items[i];
                if (deviceDisplay.Index == _rtlDevice.Device.Index)
                {
                    deviceComboBox.SelectedIndex = i;
                    break;
                }
            }
        }
 
        private void ConfigureDevice()
        {
            frequencyCorrectionNumericUpDown_ValueChanged(null, null);
            tunerGainTrackBar_Scroll(null, null);
        }
 
        private void StartDecoding()
        {
            _detectorLevelCoef = _settings.DetectorLevel;
            if (_detectorLevelCoef == 0) _detectorLevelCoef = 0.77f;
 
            _pictureHeight = (int)(_lineInFrame);
 
            _pictureWidth = (int)(_sampleRate / _countryCoeff);
 
            _pictureCentr = (int)(_pictureWidth / 2);
 
            _detectImpulsePeriod = (int)(_pictureWidth / 4);
 
            int lengthOfGrayScaleArray = (_pictureWidth * _pictureHeight);
            _grayScaleValues = new byte[lengthOfGrayScaleArray];
            _videoWindowArray = new byte[lengthOfGrayScaleArray];
 
            try
            {
                _rtlDevice.Start(rtl_SamplesAvailable);
            }
            catch (Exception e)
            {
                StopDecoding();
                MessageBox.Show("Unable to start RTL device\n" + e.Message);
                return;
            }
            _isDecoding = true;
        }
 
        private void StopDecoding()
        {
            _rtlDevice.Stop();
            _isDecoding = false;
            _grayScaleValues = null;
        }
 
        #endregion
 
        #region Samples Callback
 
        private void rtl_SamplesAvailable(object sender, Complex* buf, int length)
        {
            var agcMaxLevel = (int)0;
            var maxSignalLevel = (int) 0;
 
            for (var i = 0; i < length; i++)
            {
                var real = buf[i].Real;
                var imag = buf[i].Imag;
                agcMaxLevel = Math.Max(real, agcMaxLevel);
                var mag = (int)(Math.Sqrt(real * real + imag * imag));
                maxSignalLevel = Math.Max(mag, maxSignalLevel);
                if (_inverseVideo) mag = _maxSignalLevel - mag;
                DrawPixel(mag);
            }
            _maxSignalLevel = (int)(_maxSignalLevel * 0.9 + maxSignalLevel * 0.1);
            _detectLevel = Convert.ToInt32(_maxSignalLevel * _detectorLevelCoef);
            _blackLevel = Convert.ToInt32(_maxSignalLevel * 0.7f);
            _coeff = 255.0f / _blackLevel;
            _agcSignalLevel = agcMaxLevel;
            if (_bufferIsFull)
            {
                lock (locker)
                {
                    Array.Copy(_grayScaleValues, _videoWindowArray, _grayScaleValues.Length);
                    _bufferIsFull = false;
                }
            }
        }
 
 
 
        private void DrawPixel(int mag)
        {
 
            if (mag > _detectLevel)
            {
                _pixelCounter++;
            }
            else
            {
                if (_pixelCounter > 5)
                {
                    if (_pixelCounter > _detectImpulsePeriod)
                    {
                        _autoCorrectY = _y;
                        
                    }
                    else if (_pixelCounter < _detectImpulsePeriod)
                    {
                        _autoCorrectX = _x - _pixelCounter;
                    }
                }
                _pixelCounter = 0;
            }
            
            if (_x >= _pictureWidth)
            {
                _y += 2;
                _x = 0;
            }
            if (_y == _pictureHeight)
            {
                _y = 0;
                FineFrequencyCorrection_Tick();
            }
            if (_y > _pictureHeight)
            {
                _y = 1;
                FineFrequencyCorrection_Tick();
                _bufferIsFull = true;
            }
            
            var pixelAdress = (((_y + _correctY) % _pictureHeight) * _pictureWidth) + ((_x + _correctX) % _pictureWidth);
            var brightnes = ((_blackLevel - mag) * _coeff * _contrast);
            brightnes = brightnes + _bright;
            if (brightnes > 255) brightnes = 255;
            if (brightnes < 0) brightnes = 0;
            _grayScaleValues[pixelAdress] = (byte)brightnes;
            _x++;
        }
 
        private void FineFrequencyCorrection_Tick()
        {
            _fineTick = _fineTick + _fineFrequencyCorrection;
            if (_fineTick > 200)
            {
                _correctX = (_correctX + 1) % _pictureWidth;
                _fineTick = 0;
            }
            else if (_fineTick < -200)
            {
                _correctX = _correctX - 1;
                if (_correctX < 0) _correctX = _pictureWidth;
                _fineTick = 0;
            }
        }
            
            
 
 
        #endregion
 
        private void fpsTimer_Tick(object sender, EventArgs e)
        {
            if (!_isDecoding) return;
            lock (locker)
            {
                videoWindow.DrawPictures(_videoWindowArray, _pictureWidth, _pictureHeight);
            }
        }
 
        private void frequencyNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            {
                if (!_initialized)
                {
                    return;
                }
                _rtlDevice.Device.Frequency = (uint)(frequencyNumericUpDown.Value * 1000000);
                videoWindow.Text = Convert.ToString(frequencyNumericUpDown.Value);
            }
        }
 
        private void brightnesTrackBar_Scroll(object sender, EventArgs e)
        {
            _bright = brightnesTrackBar.Value;
        }
 
        private void contrastTrackBar_Scroll(object sender, EventArgs e)
        {
            _contrast = (contrastTrackBar.Value) / 10.0f;
        }
 
        private void samplerateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_initialized)
            {
                return;
            }
            var isPlay = _isDecoding;
            if (isPlay)
            {
                StopDecoding();
            }
            var samplerateString = samplerateComboBox.Items[samplerateComboBox.SelectedIndex].ToString().Split(' ')[0];
            _sampleRate = (double.Parse(samplerateString, CultureInfo.InvariantCulture)) * 1000000.0;
            _rtlDevice.Device.Samplerate = (uint)(_sampleRate);
            if (samplerateComboBox.SelectedIndex > 4)
            {
                _countryCoeff = 15734.25f;
                _lineInFrame = 525;
            }
            else
            {
                _countryCoeff = 15625.0f;
                _lineInFrame = 625;
            }
            if (isPlay)
            {
                StartDecoding();
            }
        }
 
        private void tunerAgcCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _rtlDevice.Device.UseTunerAGC = tunerAgcCheckBox.Checked;
            tunerGainTrackBar.Enabled = !tunerAgcCheckBox.Checked;
        }
 
        private void rtlAgcCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _rtlDevice.Device.UseRtlAGC = rtlAgcCheckBox.Checked;
        }
 
        private void programAgcCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            rtlAgcCheckBox.Enabled = !programAgcCheckBox.Checked;
            tunerAgcCheckBox.Enabled = !programAgcCheckBox.Checked;
            tunerGainTrackBar.Enabled = programAgcCheckBox.Checked;
            if (programAgcCheckBox.Checked)
            {
                tunerGainTrackBar.Enabled = true;
                _rtlDevice.Device.UseTunerAGC = false;
                _rtlDevice.Device.UseRtlAGC = false;
            }
            else
            {
                tunerAgcCheckBox_CheckedChanged(null, null);
                rtlAgcCheckBox_CheckedChanged(null, null);
            }
        }
 
        private void frequencyCorrectionTimer_Tick(object sender, EventArgs e)
        {
            if (!_isDecoding) return;
            if (_autoFrequencyCorrection)
            {
                if (_frequencyCorrection < _autoCorrectX)
                {
                    frequencyCorrectionNumericUpDown.Value = (frequencyCorrectionNumericUpDown.Value + 1) % 200;
                    _counterAutoFreqCorrect = 0;
                }
                else if (_frequencyCorrection > _autoCorrectX)
                {
                    frequencyCorrectionNumericUpDown.Value = (frequencyCorrectionNumericUpDown.Value - 1) % 200;
                    _counterAutoFreqCorrect = 0;
                }
                else if (_frequencyCorrection == _autoCorrectX)
                {
                    _counterAutoFreqCorrect++;
                    if (_counterAutoFreqCorrect == 3)
                    {
                        autoFrequencyCorrectionButton_Click(null, null);
                    }
                }
                _frequencyCorrection = _autoCorrectX;
            }
            if (_autoPositionCorrect)
            {
                _correctX = _pictureWidth - _autoCorrectX;
                _correctY = _pictureHeight - _autoCorrectY;
            }
            if (programAgcCheckBox.Checked)
            {
                if (_agcSignalLevel > 125 && _isDecoding)
                {
                    if (tunerGainTrackBar.Value > tunerGainTrackBar.Minimum)
                    {
                        tunerGainTrackBar.Value = tunerGainTrackBar.Value - 1;
                        tunerGainTrackBar_Scroll(null, null);
                    }
                }
                else if (_agcSignalLevel < 90 && _isDecoding && programAgcCheckBox.Checked)
                {
                    if (tunerGainTrackBar.Value < tunerGainTrackBar.Maximum)
                    {
                        tunerGainTrackBar.Value = tunerGainTrackBar.Value + 1;
                        tunerGainTrackBar_Scroll(null, null);
                    }
                }
            }
            if (_agcSignalLevel > 125)
            {
                label2.Text = "Gain Overload";
            }
            else
            {
                label2.Text = "Gain";
            }
            
        }
 
        private void xCorrectionTrackBar_Scroll(object sender, EventArgs e)
        {
            _correctX = xCorrectionTrackBar.Value;
        }
 
        private void yCorrectionTrackBar_Scroll(object sender, EventArgs e)
        {
            _correctY = yCorrectionTrackBar.Value * 2;
        }
 
        private void frequencyCorrectionTrackBar_Scroll(object sender, EventArgs e)
        {
            frequencyCorrectionNumericUpDown.Value = frequencyCorrectionTrackBar.Value;
            frequencyCorrectionNumericUpDown_ValueChanged(null, null);
        }
 
        private void autoSincCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _autoPositionCorrect = autoSincCheckBox.Checked;
            xCorrectionTrackBar.Enabled = !_autoPositionCorrect;
            yCorrectionTrackBar.Enabled = !_autoPositionCorrect;
            if (!_autoPositionCorrect)
            {
                xCorrectionTrackBar.Value = _correctX;
                yCorrectionTrackBar.Value = _correctY / 2;
            }
        }
 
        private void autoFrequencyCorrectionButton_Click(object sender, EventArgs e)
        {
            _autoFrequencyCorrection = !_autoFrequencyCorrection;
            if (_autoFrequencyCorrection)
            {
                autoFrequencyCorrectionButton.Text = "Stop correction";
            }
            else
            {
                autoFrequencyCorrectionButton.Text = "Auto correction";
            }
        }
 
        private void fineFrequencyCorrectTrackBar_Scroll(object sender, EventArgs e)
        {
            _fineFrequencyCorrection = fineFrequencyCorrectTrackBar.Value;
        }
 
        private void inverseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _inverseVideo = inverseCheckBox.Checked;
        }
    }
 
    public class DeviceDisplay
    {
        public uint Index { get; private set; }
        public string Name { get; set; }
 
        public static DeviceDisplay[] GetActiveDevices()
        {
            var count = NativeMethods.rtlsdr_get_device_count();
            var result = new DeviceDisplay[count];
 
            for (var i = 0u; i < count; i++)
            {
                var name = NativeMethods.rtlsdr_get_device_name(i);
                result[i] = new DeviceDisplay { Index = i, Name = name };
            }
 
            return result;
        }
 
        public override string ToString()
        {
            return Name;
        }
    }
}