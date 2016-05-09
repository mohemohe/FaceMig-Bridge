using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DxMath;
using MikuMikuPlugin;

namespace FaceMig
{
    public class FaceMig : IResidentPlugin
    {
        public string Description => @"FaceMig, inspired by FaceRig";
        public Guid GUID => new Guid(@"BC7D5699-06B9-467B-BDA5-77CC638445F4");
        public string Text => @"FaceMig";
        public string EnglishText => Text;
        public Image Image => null;
        public Image SmallImage => null;
        public IWin32Window ApplicationForm { get; set; }
        public Scene Scene { get; set; }

        private ConfigForm _configForm;

        public Stabilizer eyeL = new Stabilizer(2);
        public Stabilizer eyeR = new Stabilizer(2);
        public Stabilizer mouth = new Stabilizer(3);
        public Stabilizer leanX = new Stabilizer(10);
        public Stabilizer leanY = new Stabilizer(10);
        public Stabilizer leanZ = new Stabilizer(10);

        private bool _isTracking;
        private float _time = 0;
        private NativeBridge.StatusUnsafe _status;

        private Model _model;
        private Morph _leftEye;
        private Morph _rightEye;
        private Morph _a;
        private Bone _atama;
        private Bone _kubi;

        public void Initialize()
        {
            NativeBridge.Initialize();
        }

        public void Enabled()
        {
            NativeBridge.Enabled();
#if !DEBUG
            _configForm = new ConfigForm();
#endif
            _configForm?.Show(ApplicationForm);
            _time = 0;
            _isTracking = true;
            Track();
        }

        public void Disabled()
        {
            _isTracking = false;
            _configForm?.Close();
            NativeBridge.Disabled();
        }

        private void Track()
        {
            Task.Run(() => { 
                while (_isTracking)
                {
                    if (_time > 1000 / (float)_configForm.numericUpDown2.Value)
                    {
                        _time = float.MinValue;
                        NativeBridge.Track();
                        _status = NativeBridge.GetStatus();
                        _time = 0;
                        eyeL.Add(_status.EyeL);
                        eyeR.Add(_status.EyeR);
                        mouth.Add(_status.Mouth);
                        leanX.Add(_status.leanX);
                        leanY.Add(_status.leanY);
                        leanZ.Add(-_status.leanZ);
                    }
                    Thread.Sleep(1);
                }
            });
        }

        // ReSharper disable InconsistentNaming
        public void Update(float Frame, float ElapsedTime)
            // ReSharper restore InconsistentNaming
        {
            _time += ElapsedTime*1000;
            if (_time < 1000/(float) _configForm.numericUpDown2.Value)
            {
                return;
            }

            if (Scene.Models.Count == 0)
            {
                return;
            }
            if (_model == null || _model != Scene.ActiveModel)
            {
                _model = Scene.ActiveModel;
                if (_model.Morphs["ウィンク左"] != null)
                {
                    _leftEye = _model.Morphs["ウィンク左"];
                    if (_model.Morphs["ウィンク"] != null)
                    {
                        _rightEye = _model.Morphs["ウィンク"];
                    }
                }
                else if (_model.Morphs["ウィンク右"] != null)
                {
                    _rightEye = _model.Morphs["ウィンク右"];
                    if (_model.Morphs["ウィンク"] != null)
                    {
                        _leftEye = _model.Morphs["ウィンク"];
                    }
                }
                else if (_model.Morphs["まばたき"] != null)
                {
                    _leftEye = _rightEye = _model.Morphs["まばたき"];
                }
                else return;

                if (_model.Morphs["あ"] != null)
                {
                    _a = _model.Morphs["あ"];
                }
                else return;

                if (_model.Bones["頭"] != null)
                {
                    _atama = _model.Bones["頭"];
                }
                else return;

                if (_model.Bones["首"] != null)
                {
                    _kubi = _model.Bones["首"];
                }
                else return;
            }

            float nextWeight;
            if (_configForm.checkBox3.Checked)
            {
                nextWeight = 1.0F - eyeL.Average();
            }
            else
            {
                nextWeight = 1.0F - eyeL.Many();
            }
            _leftEye.CurrentWeight = nextWeight;

            if (_configForm.checkBox2.Checked)
            {
                if (_configForm.checkBox3.Checked)
                {
                    nextWeight = 1.0F - eyeR.Average();
                }
                else
                {
                    nextWeight = 1.0F - eyeR.Many();
                }
            }
            else
            {
                if (_configForm.checkBox3.Checked)
                {
                    nextWeight = 1.0F - eyeL.Average();
                }
                else
                {
                    nextWeight = 1.0F - eyeL.Many();
                }
            }
            _rightEye.CurrentWeight = nextWeight;

            _a.CurrentWeight = Limit(mouth.MovingAverage(), 0, 1, 0.5F);

            var xAverage = Limit(leanY.MovingAverage(), -45, 45, 0);
            var yAverage = Limit(leanX.MovingAverage(), -45, 45, 0);
            var zAverage = Limit(leanZ.MovingAverage(), -45, 45, 0);

            var atamaMotion = _atama.CurrentLocalMotion;
            atamaMotion.Rotation = Quaternion.RotationYawPitchRoll(xAverage, yAverage, zAverage);
            _atama.CurrentLocalMotion = atamaMotion;

            var kubiMotion = _kubi.CurrentLocalMotion;
            kubiMotion.Rotation = Quaternion.RotationYawPitchRoll(xAverage/1.4142F, yAverage/1.4142F, zAverage/1.4142F);
            _kubi.CurrentLocalMotion = kubiMotion;
        }

        public void Dispose()
        {
            Disabled();
            NativeBridge.Dispose();
        }

        public float Limit(float value, float min, float max, float NaN)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            if (float.IsNaN(value))
            {
                return NaN;
            }
            return value;
        }
    }
}
