
using Crossmatch.BioBaseApi;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;

namespace FPandSign
{
    public partial class Form1 : Form
    {

        private delegate void EnableControlDelegate(object data, bool val);
        private void EnableControl(object data, bool val)
        {
            System.Windows.Forms.Control ctrl = (System.Windows.Forms.Control)data;
            ctrl.Enabled = val;
        }
        // Delegate for Checked controls that setup that needs to be done after openDevice
        private delegate void CheckedControlDelegate(object data, bool val);
        private void CheckedControl(object data, bool val)
        {
            System.Windows.Forms.CheckBox ctrl = (System.Windows.Forms.CheckBox)data;
            ctrl.Checked = val;
        }

        /*!
         * \enum public enum comboBoxMethod
         * \brief Describes function to be done on the conboBox during device initialization.
         */
        public enum comboBoxMethod
        {
            Clear = 0,
            ItemsAdd,
            SelectedIndex
        };
        // Delegate for comboBox setup that needs to be done after openDevice
        // Code uses getproperty to add positions and impressions supported by the newly opened device
        private delegate void UpdatecomboBoxDelegate(object data, comboBoxMethod method, string val);
        private void UpdatecomboBox(object data, comboBoxMethod method, string val)
        {
            System.Windows.Forms.ComboBox cb = (System.Windows.Forms.ComboBox)data;
            switch (method)
            {
                case comboBoxMethod.Clear:
                    cb.Items.Clear();
                    break;
                case comboBoxMethod.ItemsAdd:
                    cb.Items.Add(val);
                    break;
                case comboBoxMethod.SelectedIndex:
                    int nIndex = Int32.Parse(val);
                    cb.SelectedIndex = nIndex;
                    break;
            }
        }
        public enum DeviceState
        {
            device_not_connected,
            device_connected_and_not_opened,
            device_opened_and_not_live,
            device_opened_and_live,
            device_opened_and_image_captured,
            device_opened_and_capture_cancelled
        };
        public static class LSEConst
        {
            public const string DEVICE_TYPE_LSCAN_1000 = "L Scan 1000";
            public const string DEVICE_TYPE_LSCAN_1000P = "L SCAN 1000P";
            public const string DEVICE_TYPE_LSCAN_1000PX = "L SCAN 1000PX";
            public const string DEVICE_TYPE_LSCAN_1000T = "L SCAN 1000T";
            public const string DEVICE_TYPE_LSCAN_500 = "L Scan 500";
            public const string DEVICE_TYPE_LSCAN_500P = "L SCAN 500P";
            public const string DEVICE_TYPE_LSCAN_500PJ = "L SCAN 500PJ";
            public const string DEVICE_TYPE_LSCAN_GUARDIAN_FW = "L SCAN GUARDIAN";
            public const string DEVICE_TYPE_LSCAN_GUARDIAN_USB = "L SCAN GUARDIAN USB";
            public const string DEVICE_TYPE_LSCAN_GUARDIAN_F = "L SCAN GUARDIAN F";
            public const string DEVICE_TYPE_LSCAN_GUARDIAN_R2 = "L SCAN GUARDIAN R2";
            public const string DEVICE_TYPE_LSCAN_GUARDIAN_T = "L SCAN GUARDIAN T";
            public const string DEVICE_TYPE_LSCAN_GUARDIAN_L = "L SCAN GUARDIAN L";
            public const string DEVICE_TYPE_LSCAN_PATROL = "L SCAN PATROL";
            public const string DEVICE_TYPE_PATROL = "PATROL";
            public const string DEVICE_TYPE_PATROL_ID = "Patrol ID";
            public const string DEVICE_TYPE_GUARDIAN = "GUARDIAN";
            public const string DEVICE_TYPE_GUARDIAN_MODULE = "GUARDIAN Module";
            public const string DEVICE_TYPE_GUARDIAN_100 = "GUARDIAN 100";
            public const string DEVICE_TYPE_GUARDIAN_200 = "GUARDIAN 200";
            public const string DEVICE_TYPE_GUARDIAN_300 = "GUARDIAN 300";
            public const string DEVICE_TYPE_GUARDIAN_45 = "GUARDIAN 45";
            public const string DEVICE_TYPE_VERIFIER_320LC = "VERIFIER 320LC";
            public const string DEVICE_TYPE_VERIFIER_320S = "VERIFIER 320S";
        }

        public void RegisterEventHandlers()
        {
            if (this.InvokeRequired)
            {
                // Invoke when called from events outside of GUI thread
                Invoke((Action)RegisterEventHandlers);
            }
            else
            {
                AddMessage("registering event 'IBioBaseDevice.Preview'");
                _biobaseDevice.Preview += new EventHandler<BioBasePreviewEventArgs>(_biobaseDevice_Preview);

                AddMessage("registering event 'IBioBaseDevice.ObjectQuality'");
                _biobaseDevice.ObjectQuality += new EventHandler<BioBaseObjectQualityEventArgs>(_biobaseDevice_ObjectQuality);

                AddMessage("registering event 'IBioBaseDevice.ObjectCount'");
                _biobaseDevice.ObjectCount += new EventHandler<BioBaseObjectCountEventArgs>(_biobaseDevice_ObjectCount);

                AddMessage("registering event 'IBioBaseDevice.ScannerUserInput'");
                _biobaseDevice.ScannerUserInput += new EventHandler<BioBaseUserInputEventArgs>(_biobaseDevice_ScannerUserInput);

                //TODO: Add support for ScannerUserOutput once we know why there is an error when unregistering this event.
                //        AddMessage("registering event 'IBioBaseDevice.ScannerUserOutput'");
                //        _biobaseDevice.ScannerUserOutput += new EventHandler<BioBaseUserOutputEventArgs>(_biobaseDevice_ScannerUserOutput);

                AddMessage("registering event 'IBioBaseDevice.AcquisitionStart'");
                _biobaseDevice.AcquisitionStart += new EventHandler<BioBaseAcquisitionStartEventArgs>(_biobaseDevice_AcquisitionStart);

                AddMessage("registering event 'IBioBaseDevice.AcquisitionComplete'");
                _biobaseDevice.AcquisitionComplete += new EventHandler<BioBaseAcquisitionCompleteEventArgs>(_biobaseDevice_AcquisitionComplete);

                AddMessage("registering event 'IBioBaseDevice.DataAvailable'");
                _biobaseDevice.DataAvailable += new EventHandler<BioBaseDataAvailableEventArgs>(_biobaseDevice_DataAvailable);

                AddMessage("registering event 'IBioBaseDevice.DetectedObject'");
                _biobaseDevice.DetectedObject += new EventHandler<BioBaseDetectObjectEventArgs>(_biobaseDevice_DetectedObject);
            }
        }
        #region BioB Device Events

        /*!
         * \fn void _biobaseDevice_Init()
         * \brief New event triggered during device open to give status of initialization process
         * Note that all updates to the UI from this event must be done via Invoke calls.
         * This event is called from a thread created in the low level SDK to notify the application 
         * of the initialization process.
         * This event updates the UI with message box but could also control a progress bar.
         */
        void _biobaseDevice_Init(object sender, BioBaseInitProgressEventArgs e)
        {
            float progress = e.ProgressValue;
            AddMessage(string.Format("event: Initializing device... {0}%", progress));
        }

        /*!
         * \fn void _biobaseDevice_Preview()
         * \brief New event triggered during capture with each new image
         * Note that all updates to the UI from this event must be done via Invoke calls.
         * This event is called from a thread created in the low level SDK with decimated preview
         * iamges at a rate between 10 and 30 per second.
         * This event updates the UI with UI ImageBox.
         * NOTE: When using multiple devices it is a good idea to add check for valid e.DeviceID
         * NOTE: Use this event to flag any additional overlay information on top of new preview image.
         * Other options to this event is the have the LSE SDK draw to the ImageBox. While not shown
         * in this sample, the BioB_SetVisualizationWindow() method supports LSE drawing the preview image.
         */
        void _biobaseDevice_Preview(object sender, BioBasePreviewEventArgs e)
        {
            if (checkBoxVisualization.Checked == false)
            {
                // Visualization is not being used so we must display image data in preview event 
                Bitmap ImageData = e.ImageData;
                ImageBox.Image = ImageData;
            }


        }

        /*!
         * \fn void _biobaseDevice_ObjectQuality()
         * \brief New event triggered during capture when the quality of each detected finger objects change.
         * During a 4 finger flat capture the image quality array will have 4 entries while during 
         * a fingerprint roll, the quality array will only have one entry.
         * Note that all updates to the UI from this event must be done via Invoke calls.
         * This event is called saves the image quality array to the global _biobaseDevice.mostRecentqualities variable.
         * The _biobaseDevice.mostRecentqualities.Length is then used to update the UI LED light panel and the capture status on each device.
         * Catch BioBaseException and Exception and log errors
         */
        void _biobaseDevice_ObjectQuality(object sender, BioBaseObjectQualityEventArgs e)
        {
            try
            {
                AddMessage("event BIOB_OBJECT_QUALITY: object[] quality");
                if (e is BioBaseObjectQualityEventArgs)
                {
                    _biobaseDevice.mostRecentqualities = ((BioBaseObjectQualityEventArgs)e).QualStateArray;
                    switch (_biobaseDevice.mostRecentqualities.Length)
                    {
                        case 1:
                            SetUILedColors(ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[0]),
                              ActiveColor.gray, ActiveColor.gray, ActiveColor.gray, ActiveColor.gray);
                            break;
                        case 2:
                            SetUILedColors(ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[0]),
                              ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[1]),
                              ActiveColor.gray, ActiveColor.gray, ActiveColor.gray);
                            break;
                        case 3:
                            SetUILedColors(ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[0]),
                              ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[1]),
                              ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[2]),
                              ActiveColor.gray, ActiveColor.gray);
                            break;
                        case 4:
                            SetUILedColors(ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[0]),
                              ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[1]),
                              ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[2]),
                              ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[3]), ActiveColor.gray);
                            break;
                        case 5:
                            // support 5th (upper palm) status "LED" for LScan palm scanners
                            SetUILedColors(ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[0]),
                              ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[1]),
                              ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[2]),
                              ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[3]),
                              ConvertQualityToIndicatorColor(_biobaseDevice.mostRecentqualities[4]));
                            break;
                    }

                    _SetStatusElements();  // Update status on device with mostRecentqualities and mostResentKey
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("event: BIOB_OBJECT_QUALITY error {0}%", ex.Message));
            }
        }

        /*!
         * \fn void _biobaseDevice_ObjectCount()
         * \brief New event triggered during capture when a new finger objects is detected.
         * During a 4 finger flat capture the e.ObjectCountState will return BIOB_TOO_FEW_OBJECTS 
         * when less than 4 figners are detectd. BIOB_OBJECT_COUNT_OK is returned when
         * the expected number of finger objects is detected.
         * Note that all updates to the UI from this event must be done via Invoke calls.
         * NOTE: Use this event to flag display hints and prompt operator when the number of fingers on the platen in not correct
         */
        void _biobaseDevice_ObjectCount(object sender, BioBaseObjectCountEventArgs e)
        {
            BioBObjectCountState state = e.ObjectCountState;
            switch (state)
            {
                case BioBObjectCountState.BIOB_OBJECT_COUNT_OK:
                    AddMessage("event BIOB_OBJECT_COUNT: Object count (OK)");
                    break;
                case BioBObjectCountState.BIOB_TOO_FEW_OBJECTS:
                    AddMessage("event BIOB_OBJECT_COUNT: Object count (Too few objects)");
                    break;
                case BioBObjectCountState.BIOB_TOO_MANY_OBJECTS:
                    AddMessage("event BIOB_OBJECT_COUNT: Object count (Too many objects)");
                    break;
            }
        }

        /*!
         * \fn void _biobaseDevice_ScannerUserInput()
         * \brief New event triggered during capture when a key is pressed on the device.
         * Note that all updates to the UI from this event must be done via Invoke calls.
         * This event can be expaned to suppport more key types. This event only demonstrates some 
         * of the key types availble from the devices.
         * NOTE: "IDButtonConfirmActive" and "IDButtonRetryActive" defined in Templates\imgs\*.svg files
         * \param  global input m_bAskRecapture used to determine capture state.  m_bAskRecapture is false 
         * during capture. The _biobaseDevice_DataAvailable event will set m_bAskRecapture to true.
         * During capture, the keys can cancel or adjust the catpure process.
         * After catpure, the keys can accept an image or request a rescan.
         */
        void _biobaseDevice_ScannerUserInput(object sender, BioBaseUserInputEventArgs e)
        {
            AddMessage("event BIOB_SCANNER_USERINPUT: Pressed key...");

            switch (e.PressedKeys)
            {
                case (PropertyConstants.OUT_DATA_KEY_FOOTSWITCH):
                case (PropertyConstants.OUT_DATA_KEY_RIGHT):
                case (PropertyConstants.OUT_DATA_KEY_OK):
                case ("IDButtonConfirmActive"):
                    if (m_bAskRecapture == false)
                        _biobaseDevice.AdjustAcquisitionProcess(PropertyConstants.PROC_ADJUST_TYPE_OPTIMIZE_CONTRAST, null);
                    else
                    {
                        string msg = string.Format("Capture warning...");
                        //TODO: Add prompt here to accept image with warning to replace prompt in _biobaseDevice_DataAvailable dlg
                        // Current applcation does not properly process this device key because of MessageBox in the _biobaseDevice_DataAvailable event
                        _BeepError();
                    }
                    break;

                case (PropertyConstants.OUT_DATA_KEY_CANCEL):
                case (PropertyConstants.OUT_DATA_KEY_LEFT):
                case ("IDButtonRetryActive"):
                    if (m_bAskRecapture == false)
                    { // capture in progress
                        _biobaseDevice.CancelAcquisition();
                        SetDeviceState(DeviceState.device_opened_and_capture_cancelled);

                        //Reset device's LEDs, TFT display or Touch display here
                        _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
                        _ResetStatusElements();
                        _ResetGuidanceElements();
                    }
                    else
                    {
                        string msg = string.Format("capture warning...");
                        //TODO: Add prompt here to recapture image with warning to replace prompt in _biobaseDevice_DataAvailable dlg
                        // Current applcation logic will not properly process RescanImage call because of MessageBox in the _biobaseDevice_DataAvailable event
                        RescanImage();
                        _BeepError();
                    }
                    break;
            }
        }

        /*!
         * \fn void _biobaseDevice_ScannerUserOutput()
         * \brief New event triggered when data is sent to the scanner devices.
         * Note that all updates to the UI from this event must be done via Invoke calls.
         *
         * This event is not enabled.
         *
         */
        void _biobaseDevice_ScannerUserOutput(object sender, BioBaseUserOutputEventArgs e)
        {
            AddMessage(string.Format("event BIOB_SCANNER_USEROUTPUT: User Output to scanner acknowledged transactionID:{0}", e.TransactionID));

            // Option to confirm XML data sent to device.
            //if (e.FormatType == BioBOutputDataFormat.BIOB_OUT_XML)
            //{

            //}

            //Display a copy of Guardian 300 User Guidance image in UI
            // When e.FormatType is an image, then e.SetOutputData can be displayed in the UI.
            // This will only come from the Guardian, Guardian 300, Guardian 200, Guardian 100 and Guardian Module
            if (e.FormatType == BioBOutputDataFormat.BIOB_OUT_BMP)
            {
                System.IO.MemoryStream bytes = new System.IO.MemoryStream(e.SetOutputData);
                Bitmap bmp = new Bitmap(bytes);
            }
        }

        /*!
         * \fn void _biobaseDevice_AcquisitionStart()
         * \brief New event triggered during capture when the auto capture starts.
         * Note that all updates to the UI from this event must be done via Invoke calls.
         * 
         * When performing capture, this is triggered when all the autocapture threashold have been met.
         * With flat capture, the _biobaseDevice_AcquisitionComplete will be triggered next.
         * With roll capture, the rolled image is still being stitched together and the 
         * _biobaseDevice_AcquisitionComplete event won't be triggered until the finger is lifted off the platen.
         * 
         * Optionally update screen message that all the autocapture threashold have been met.
         */
        void _biobaseDevice_AcquisitionStart(object sender, BioBaseAcquisitionStartEventArgs e)
        {
            AddMessage("event BIOB_ACQUISITION_STARTED: acquisition start");
        }

        /*!
         * \fn void _biobaseDevice_AcquisitionComplete()
         * \brief New event triggered during capture when the capture is complete
         * There can be a delay between this event and _biobaseDevice_DataAvailable event that contains 
         * the final image. This delay varies based on how long it takes to transfer the final image
         * from the device to the PC. This is typically longer on the LScan palm devices so this event
         * is used to display an hour glass on the device during this delay.
         */
        void _biobaseDevice_AcquisitionComplete(object sender, BioBaseAcquisitionCompleteEventArgs e)
        {
            AddMessage("event BIOB_ACQUISITION_COMPLETED: acquisition complete");
            // nothing more to do right now. Wait for Data Available event...

            switch (_biobaseDevice.deviceGuidanceType)
            {
                case guidanceType.guidanceTypeTFT:
                case guidanceType.guidanceTypeTFT_1000:
                    TFT_ObjectCaptureDictionary obj = new TFT_ObjectCaptureDictionary();
                    obj[PropertyConstants.OUT_DATA_TFT_STAT_TOP] = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ERASE;
                    obj[PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM] = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_HOURGLASS_ANIMATED;
                    _TftShowFingerCaptureScreen(false, obj);

                    break;
            }
        }


        /*!
         * \fn void _biobaseDevice_DataAvailable()
         * \brief New event triggered when capture is complete and final image is ready.
         * Note that all updates to the UI from this event must be done via Invoke calls.
         * global input _biobaseDevice use to send OK beep to device when capture is complete
         * global input _biobaseDevice.mostRecentqualities and _biobaseDevice.mostResentKey to control keys on status elements on device
         * global input _biobaseDevice.mostResentKey set to disable keys on device if capture is successful
         * \param global m_bAskRecapture set to true to flag accept or re-capture status because of warning
         * \param global m_bAskRecapture set to true to flag accept or re-capture status because of warning
         */
        void _biobaseDevice_DataAvailable(object sender, BioBaseDataAvailableEventArgs e)
        {
            AddMessage("event BIOB_DATA_AVAILABLE: final image ready");
            string devID = e.DeviceID;
            if ((e.IsFinal == true) && (e.DataStatus >= (int)BioBReturnCode.BIOB_SUCCESS))
            {
                // Display final full resolution image data!!!!!!!!!!!!!!!!!
                Bitmap ImageData = e.ImageData;
                ImageBox.Image = ImageData;

                AddMessage("event: Final image displayed");

                if (devID == _biobaseDevice.DeviceInfo.DeviceId)
                    _BeepOK();
                else
                    System.Media.SystemSounds.Beep.Play();

                if (e.DataStatus == (int)BioBReturnCode.BIOB_SUCCESS)
                    _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
                else
                {
                    m_bAskRecapture = true;     // Used to OK button for accept captured image
                    _biobaseDevice.mostResentKey = ActiveKeys.KEYS_ACCEPT_RECAPTURE;
                }
                _SetFinalStatusElements((BioBReturnCode)e.DataStatus);  // Update status on device with _e.DataStatus and _biobaseDevice.mostResentKey

                if (e.IsPADScoresValid == true)
                {
                    AddMessage("PAD data for captured fingerprints");
                    string msg = string.Format("   PAD invalid score value is {0:0.0000}", e.PADScoreInvalid);
                    AddMessage(msg);
                    msg = string.Format("   PAD minimum score value is {0:0.0000}", e.PADScoreMinimum);
                    AddMessage(msg);
                    msg = string.Format("   PAD maximum score value is {0:0.0000}", e.PADScoreMaximum);
                    AddMessage(msg);
                    msg = string.Format("   PAD threshold value is {0:0.0000}", e.PADThresold);
                    AddMessage(msg);
                    foreach (double score in e.PADScore)
                    {
                        msg = string.Format("   PAD Score is {0:0.0000}", score);
                        AddMessage(msg);
                    }
                }


                // Capture complete, now check for warnings on rolled fingers and PAD (spoof).
                // Display roll warnings (may have multiple warnings) and prompt operator to accept or re-capture.
                bool promptRescan = false;
                bool promptRescan2 = false;
                string strRescan = "";
                //check capture status for roll messages
                if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY) == (int)BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY)
                {
                    promptRescan = true;
                    strRescan += " SHIFTED HORIZONTALLY";
                }
                if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY) == (int)BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY)
                {
                    promptRescan = true;
                    strRescan += " SHIFTED VERTICALLY";
                }
                if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_LIFTED_TIP) == (int)BioBReturnCode.BIOB_ROLL_LIFTED_TIP)
                {
                    promptRescan = true;
                    strRescan += " LIFTED_TIP";
                }
                if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_ON_BORDER) == (int)BioBReturnCode.BIOB_ROLL_ON_BORDER)
                {
                    promptRescan = true;
                    strRescan += " ROLL_ON_BORDER";
                }
                if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_PAUSED) == (int)BioBReturnCode.BIOB_ROLL_PAUSED)
                {
                    promptRescan = true;
                    strRescan += " ROLL_PAUSED";
                }
                if ((e.DataStatus & (int)BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW) == (int)BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW)
                {
                    promptRescan = true;
                    strRescan += " ROLL_TOO_NARROW";
                }

                if ((e.DataStatus & (int)BioBReturnCode.BIOB_SPOOF_DETECTED) == (int)BioBReturnCode.BIOB_SPOOF_DETECTED)
                {
                    promptRescan2 = true;
                    strRescan += " SPOOF_DETECTED";
                }
                if ((e.DataStatus & (int)BioBReturnCode.BIOB_SPOOF_DETECTOR_FAIL) == (int)BioBReturnCode.BIOB_SPOOF_DETECTOR_FAIL)
                {
                    promptRescan2 = true;
                    strRescan += " SPOOF_DETECTOR_FAIL";
                }



                //If option to not use MessageBox, exit this event and wait for keys in _biobaseDevice_ScannerUserInput event to accept warning or rescan
                if (promptRescan)
                {
                    string strRollMessage = "Detected the following roll warnings:" + strRescan + ". Do you want to accept the image?";
                    if (MessageBox.Show(strRollMessage, "Roll Warning", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        RescanImage();
                        return;
                    }
                }
                if (promptRescan2)
                {
                    string strRollMessage = "Detected PAD warnings:" + strRescan + ". Do you want to accept the image?";
                    if (MessageBox.Show(strRollMessage, "PAD Warning", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        RescanImage();
                        return;
                    }
                }
            }
            else
            {
                string msg = string.Format("event: Final image not available; error {0}", e.DataStatus);
                AddMessage(msg);

                if (devID == _biobaseDevice.DeviceInfo.DeviceId)
                    _BeepError();
                else
                    System.Media.SystemSounds.Exclamation.Play();
                _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
                _SetFinalStatusElements((BioBReturnCode)e.DataStatus);  // Update status on device with _e.DataStatus and _biobaseDevice.mostResentKey
            }


            SetDeviceState(DeviceState.device_opened_and_image_captured);


            //May want to reset device's LEDs, TFT display or Touch display here
            //      or wait until operator closes device and or starts new acquisition.
            _ResetStatusElements();
            _ResetGuidanceElements();
        }

        /*!
         * \fn void _biobaseDevice_DetectedObject()
         * \brief New event triggered if object was UNEXPECTEDLY detected on the platen
         * Note that all updates to the UI from this event must be done via Invoke calls.
         * NOTE: Use this event to display hints and prompt operator when something is on the platen
         */
        void _biobaseDevice_DetectedObject(object sender, BioBaseDetectObjectEventArgs e)
        {
            AddMessage("event BIOB_OBJECT_DETECTED: object on platen");
            BioBDeviceDectionAreaState state = e.DetectionAreaState;
            switch (state)
            {
                case BioBDeviceDectionAreaState.BIOB_CLEAR_OBJECT_FROM_DETECTION_AREA:
                    AddMessage("Remove object from platen before continuing.");
                    break;
                case BioBDeviceDectionAreaState.BIOB_DETECTION_AREA_CLEAR:
                    AddMessage("Detected object has been remvoed from platen.");
                    break;
            }
        }
        #endregion

        LseBioBase _biobase = null;
        IBioBaseDevice _biobaseDevice = null;   // Object for the one device that this application will have open at a given time.
        BioBaseDeviceInfo[] _biobaseDevices;

        DeviceState _deviceState = DeviceState.device_not_connected;
        string _deviceType = null;
        private bool _imageCaptured = false;    // Final image available when flag is true
        bool m_scannerOpen = false;
        bool m_bAskRecapture = false;     // Used to confirm if Scanner OK button is for contrast adjustment or accept captured image

        bool m_bMsgBoxOpened = false;

        bool m_ImpressionModeRoll = false;  //no adjust on the fly for roll

        string m_CurrentTFTstatus = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ERASE; // quality status prevously sent to TFT display
        ActiveKeys m_CurrentTFTKey = ActiveKeys.KEYS_NONE;                                // keys prevously sent to TFT display


        string m_TouchDisplayTemplatePath;  //< Touch display template root path

        public Form1()
        {
            InitializeComponent();

            m_TouchDisplayTemplatePath = AppDomain.CurrentDomain.BaseDirectory + "Templates\\";
            m_TouchDisplayTemplatePath = m_TouchDisplayTemplatePath.Replace("\\", "/");
            m_TouchDisplayTemplatePath = "file:///" + m_TouchDisplayTemplatePath;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenAPI();
            OpenDevice();

            //****** Signature *******//
            CheckForIllegalCrossThreadCalls = false;

            endPos = new PointF(-1F, -1f);
            beginPos = new PointF(-1F, -1f);
            frontPos = new PointF(-1F, -1f);

            lastpointx[0] = -1;
            lastpointy[0] = -1;
            lastpointx[1] = -1;
            lastpointy[1] = -1;
            lastpointx[2] = -1;
            lastpointy[2] = -1;
            xypointcount = 0;

            g = pictureBox1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //mGraphicsBuffer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //-------------------------//
        }
        private void InitializeBioBase()
        {
            try
            {

                AddMessage("BioBase object.");
                _biobase = new LseBioBase();

                AddMessage("BioBase API Register DeviceCount Change callback event.");
                _biobase.DeviceCountChanged += new EventHandler<BioBaseDeviceCountEventArgs>(_biobase_DeviceCount);

                AddMessage("BioBase API Open");
                _biobase.Open();

                // Optional logging of BioBase API version.
                BioBaseInterfaceVersion ver = _biobase.InterfaceVersion;
                AddMessage(string.Format("BioBase API version {0}.{1}", ver.Major, ver.Minor));

                // Optional logging of this API version.
                BioBaseApiProperties prop = _biobase.ApiProperties;
                AddMessage(string.Format("{0} SDK version {1}.", prop.Api, prop.Product));

            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("InitializeBioBase BioBase object error {0}", ex.Message));
            }
            catch (DllNotFoundException ex)
            {
                string msg = string.Format("LSEBioBase Open method failed - {0}.  \n\n Copy native dlls to development folder?", ex.Message);
                DialogResult result = MessageBox.Show(msg, "Missing native dlls", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("InitializeBioBase object error {0}", ex.Message));
            }
        }


        private String GetOSVersion()
        {
            //  Per Microsoft - Applications not manifested for Windows 8.1 or Windows 10 will return the Windows 8 OS version value (6.2). This is for support of legacy apps.
            // i.e. GetVersionEx does not set dwMajorVersion & dwMinorVersion after Windows 8.0.
            // Also, RtlGetVersion() does not set wProductType which is properly set by the GetVersionEx() function.
            // Thus for C++ call GetVersionEx() to get wProductType and then RtlGetVersion() for dwMajorVersion & dwMinorVersion. Must be in order...
            // For C#, we can read the registry...

            //For NT Platform...
            Int32 OSMajorVersion = (Int32)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentMajorVersionNumber", -1);
            Int32 OSMinorVersion = (Int32)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentMinorVersionNumber", -1);
            String OSCurrentVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", string.Empty).ToString();
            String OSProductName = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", string.Empty).ToString();
            String OSBuildVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", string.Empty).ToString();
            String OSVersion;

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                    return "Win 3.1";
                case PlatformID.Win32Windows:
                    switch (Environment.OSVersion.Version.Minor)
                    {
                        case 0: return "Windows 95";
                        case 10: return "Windows 98";
                        case 90: return "Windows ME";
                        default: return "Widnows 9x";
                    }

                case PlatformID.Win32NT:
                    switch (OSMajorVersion)
                    {
                        case 3: return "Widnows NT 3.51";
                        case 4: return "Widnows NT 4.0";
                        case 5:
                            switch (OSMinorVersion)
                            {
                                case 0: return "Widnows 2000";
                                case 1: return "Widnows XP";
                                case 2: return "Widnows 2003";
                            }
                            break;

                        case 6:
                            switch (OSMinorVersion)
                            {
                                case 0: return "Widnows Vista";
                                case 1: return "Widnows 7";
                                case 2: return "Widnows 8";
                                case 3: return "Widnows 8.1";
                            }
                            break;
                        case 10:
                            OSVersion = String.Format("{0} ({1}.{2}.{3})", OSProductName, OSMajorVersion, OSMinorVersion, OSBuildVersion);
                            return OSVersion;
                        case -1:
                            // no OSMajorVersion or OSMinorVersion entry on some Windows 7 systems.
                            OSVersion = String.Format("{0} {1}.{2}", OSProductName, OSCurrentVersion, OSBuildVersion);
                            return OSVersion;
                        default:
                            OSVersion = String.Format("Unknown OS ({0} {1} {2}.{3}.{4})", Environment.OSVersion.Platform, Environment.OSVersion.Version.Minor, OSMajorVersion, OSMinorVersion, OSBuildVersion);
                            return OSVersion;
                    }
                    break;

                case PlatformID.WinCE:
                    return "Windows CE";
            }

            return "Unknown";
        }
        public void AddMessage(string eventName)
        {
            if (textBoxLog.InvokeRequired)
            {
                // Invoke when called from events outside of UI thread
                Invoke((Action<string>)AddMessage, eventName);
            }
            else
            {
                if (textBoxLog.TextLength + eventName.Length > textBoxLog.MaxLength)
                {
                    const int truncatingSize = 10000;
                    string textTB = textBoxLog.Text;
                    textTB = textTB.Remove(0, truncatingSize);
                    textBoxLog.Text = textTB;
                }
                this.textBoxLog.Text += eventName + "\r\n";
                this.textBoxLog.SelectionStart = textBoxLog.TextLength;
                this.textBoxLog.ScrollToCaret();
                this.Refresh();
            }
        }

        void _biobase_DeviceCount(object sender, BioBaseDeviceCountEventArgs e)
        {
            try
            {
                AddMessage("event BIOB_DEVICE_COUNT_CHANGED: Checking number of attached devices");

                if (e.DeviceCount > 0)
                {
                    if (m_scannerOpen == false)
                        SetDeviceState(DeviceState.device_connected_and_not_opened);
                }
                else
                {
                    //TODO: If device was open and now disconnected, we need to call CloseDevice()
                    SetDeviceState(DeviceState.device_not_connected);
                }

                // Update list of connected devices
                _biobaseDevices = _biobase.ConnectedDevices;
                FillDeviceListBox();
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBaseDeviceCountEventArgs thread BioBase error {0}", ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("BioBaseDeviceCountEventArgs thread error {0}", ex.Message));
            }
        }
        private void FillDeviceListBox()
        {
            try
            {
                if (DeviceInfoBox.InvokeRequired)
                {
                    Invoke((Action)FillDeviceListBox);
                }
                else
                {
                    bool foundDev = false;
                    string SelectedindexDevID = "";

                    // Remember Device ID so we can re-select device when re-populate DeviceInfoBox
                    if (DeviceInfoBox.Items.Count > 0)
                        SelectedindexDevID = DeviceInfoBox.SelectedItem.ToString();

                    // If device was open and now disconnected, we need to call CloseDevice()
                    // This logic can be reduced if you want to assume you will only have one device.
                    // Get name of open device and assume it is no longer attached until proven otherwise
                    string openDevice = "";
                    bool bOpenDeviceAttached = false;
                    if (_biobaseDevice != null)
                        openDevice = _biobaseDevice.DeviceInfo.DeviceId;

                    // clear out old device information
                    while (DeviceInfoBox.Items.Count != 0)
                    {
                        DeviceInfoBox.Items.Clear();
                        DeviceInfoBox.ClearSelected();
                    }

                    if ((_biobaseDevices == null) || (_biobaseDevices.Length == 0))
                    {
                        return; // Return if no devices are attached....
                    }

                    if (_biobase != null)
                    {
                        // loop through attached devices and add all of the DeviceID to the DeviceInfoBox list box
                        AddMessage(" Filling list box with list of attached devices");
                        foreach (BioBaseDeviceInfo device in _biobaseDevices)
                        {
                            if (device.DeviceId.Length > 0)
                            {
                                DeviceInfoBox.Items.Add(device.DeviceId);

                                if (device.DeviceId == openDevice)
                                    bOpenDeviceAttached = true; // We found open device is still attached. So, we won't call CloseDevice after this foreach loop

                                // First, always select first item in list by default
                                if (DeviceInfoBox.Items.Count == 1)
                                    DeviceInfoBox.SelectedIndex = DeviceInfoBox.TopIndex;
                                // Second, always select item in list if it was prevously selected.
                                if (SelectedindexDevID == device.DeviceId)
                                {
                                    // If previously selelected item is found, make sure it is still selected
                                    // Previously selelected item may still be open...
                                    foundDev = true;
                                    DeviceInfoBox.SelectedIndex = DeviceInfoBox.Items.Count - 1;
                                }
                            }
                        }

                        //If a device was open and is now disconnected, it must be closed
                        if ((_biobaseDevice != null) && (bOpenDeviceAttached == false))
                            CloseDevice();

                        // If not able to re-select any prevously selected device, enable DeviceInfoBox
                        if (foundDev == false)
                        {
                            // if new device is selected, it will not be open yet
                            SetDeviceState(DeviceState.device_connected_and_not_opened);
                            DeviceInfoBox.Enabled = true;
                        }
                    }
                }
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("FillDeviceListBox BioBase error {0}", ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("FillDeviceListBox error {0}", ex.Message));
            }
        }
        void CloseDevice()
        {
            try
            {
                if (_biobaseDevice == null)
                    return;

                // Remove any _biobaseDevice_DataAvailable image. 1. won't conflict with visualization image. 2. ensure security of personal data (GDPR)!!!!
                if (_biobaseDevice.mostRecentImpression != "")
                {
                    ImageBox.Image = null;
                    ImageBox.Update();
                }

                if (_biobaseDevice != null)
                {
                    //reset device's LEDs, TFT display or Touch display here
                    _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
                    _ResetStatusElements();
                    _ResetGuidanceElements();

                    _biobaseDevice.DetectedObject -= _biobaseDevice_DetectedObject;
                    _biobaseDevice.DataAvailable -= _biobaseDevice_DataAvailable;
                    _biobaseDevice.AcquisitionComplete -= _biobaseDevice_AcquisitionComplete;
                    _biobaseDevice.AcquisitionStart -= _biobaseDevice_AcquisitionStart;
                    _biobaseDevice.ScannerUserOutput -= _biobaseDevice_ScannerUserOutput;
                    _biobaseDevice.ScannerUserInput -= _biobaseDevice_ScannerUserInput;
                    _biobaseDevice.ObjectCount -= _biobaseDevice_ObjectCount;
                    _biobaseDevice.ObjectQuality -= _biobaseDevice_ObjectQuality;
                    _biobaseDevice.Preview -= _biobaseDevice_Preview;

                    _biobase.InitProgress -= _biobaseDevice_Init;

                    AddMessage("calling method 'IBioBaseDevice.Dispose'");
                    _biobaseDevice.Dispose();
                    _biobaseDevice = null;
                }
            }
            catch (Exception ex)
            {
                // log but ignore errors on closing
                AddMessage(string.Format("CloseDevice error {0}", ex.Message));
            }
            finally
            {
                if (DeviceInfoBox.InvokeRequired)
                    DeviceInfoBox.Invoke(new EnableControlDelegate(EnableControl), new object[] { DeviceInfoBox, true });
                else
                    EnableControl(DeviceInfoBox, true);

                SetDeviceState(DeviceState.device_connected_and_not_opened);
                m_scannerOpen = false;
            }
        }
        public void SetDeviceState(DeviceState deviceState)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    // Invoke when called from events outside of UI thread
                    Invoke((Action<DeviceState>)SetDeviceState, deviceState);
                }
                else
                {
                    _deviceState = deviceState;
                    switch (_deviceState)
                    {
                        case DeviceState.device_not_connected:
                            DeviceStatus.Text = "Device Not Connected";
                            DeviceStatus.BackColor = Color.Red;
                            DeviceStatus.ForeColor = Color.White;

                            comboBox_NumObjCapture.Enabled = false;
                            comboBox_Position.Enabled = false;
                            comboBox_Impression.Enabled = false;
                            btnOpenDevice.Enabled = false;
                            btnCloseDevice.Enabled = false;
                            //btnProperties.Enabled = false;
                            btnSave.Enabled = false;
                            btnAcquire.Enabled = false;
                            //btnForce.Enabled = false;
                            btnCancelAcquire.Enabled = false;
                            //btnAdjust.Enabled = false;
                            checkBoxAltTrigger.Enabled = false;
                            radioButtonInsufficientQuality.Enabled = false;
                            radioButtonInsufficientObjectCount.Enabled = false;
                            checkBoxAutocontrast.Enabled = false;
                            checkBoxPAD.Enabled = false;
                            checkBox1000dpi.Enabled = false;
                            checkBoxFlexRollCapture.Enabled = false;
                            checkBoxFlexFlatCapture.Enabled = false;
                            checkBoxVisualization.Enabled = false;

                            //btnUserControls.Enabled = false;
                            _imageCaptured = false;
                            break;

                        case DeviceState.device_connected_and_not_opened:
                            DeviceStatus.Text = "Device Connected";
                            DeviceStatus.BackColor = Color.Orange;
                            DeviceStatus.ForeColor = Color.White;

                            comboBox_NumObjCapture.Enabled = false;
                            comboBox_Position.Enabled = false;
                            comboBox_Impression.Enabled = false;
                            btnOpenDevice.Enabled = true;
                            btnCloseDevice.Enabled = false;
                            //btnProperties.Enabled = false;
                            btnSave.Enabled = false;
                            btnAcquire.Enabled = false;
                            //btnForce.Enabled = false;
                            //btnAdjust.Enabled = false;
                            btnCancelAcquire.Enabled = false;
                            checkBoxAltTrigger.Enabled = false;
                            radioButtonInsufficientQuality.Enabled = false;
                            radioButtonInsufficientObjectCount.Enabled = false;
                            checkBoxAutocontrast.Enabled = false;
                            checkBoxPAD.Enabled = false;
                            checkBox1000dpi.Enabled = false;
                            checkBoxFlexRollCapture.Enabled = false;
                            checkBoxFlexFlatCapture.Enabled = false;
                            checkBoxVisualization.Enabled = false;

                            //btnUserControls.Enabled = false;
                            _imageCaptured = false;
                            break;

                        case DeviceState.device_opened_and_not_live:
                            DeviceStatus.Text = "Device Open";
                            DeviceStatus.BackColor = Color.Green;
                            DeviceStatus.ForeColor = Color.White;

                            comboBox_NumObjCapture.Enabled = true;
                            comboBox_Position.Enabled = true;
                            comboBox_Impression.Enabled = true;
                            btnOpenDevice.Enabled = false;
                            btnCloseDevice.Enabled = true;
                            //btnProperties.Enabled = true;
                            btnSave.Enabled = false;
                            btnAcquire.Enabled = true;
                            //btnForce.Enabled = false;
                            //btnAdjust.Enabled = false;
                            btnCancelAcquire.Enabled = false;
                            checkBoxAltTrigger.Enabled = true;
                            radioButtonInsufficientQuality.Enabled = true;
                            radioButtonInsufficientObjectCount.Enabled = true;
                            checkBoxAutocontrast.Enabled = true;
                            checkBoxPAD.Enabled = true;
                            checkBox1000dpi.Enabled = true;
                            checkBoxFlexRollCapture.Enabled = true;
                            checkBoxFlexFlatCapture.Enabled = true;
                            checkBoxVisualization.Enabled = true;

                            //btnUserControls.Enabled = true;
                            _imageCaptured = false;
                            break;

                        case DeviceState.device_opened_and_live:
                            DeviceStatus.Text = "Acquiring";
                            DeviceStatus.BackColor = Color.LightPink;
                            DeviceStatus.ForeColor = Color.Black;

                            comboBox_Position.Enabled = false;
                            comboBox_Impression.Enabled = false;
                            btnOpenDevice.Enabled = false;
                            btnCloseDevice.Enabled = true;
                            // btnProperties.Enabled = false;
                            btnSave.Enabled = false;
                            btnAcquire.Enabled = false;
                            //btnForce.Enabled = true;
                            //btnAdjust.Enabled = !m_ImpressionModeRoll; // true;
                            btnCancelAcquire.Enabled = true;

                            //btnUserControls.Enabled = false;
                            _imageCaptured = false;
                            break;

                        case DeviceState.device_opened_and_image_captured:
                            DeviceStatus.Text = "Fingerprint Captured";
                            DeviceStatus.BackColor = Color.Green;
                            DeviceStatus.ForeColor = Color.White;

                            comboBox_NumObjCapture.Enabled = true;
                            comboBox_Position.Enabled = true;
                            comboBox_Impression.Enabled = true;
                            btnOpenDevice.Enabled = false;
                            btnCloseDevice.Enabled = true;
                            //btnProperties.Enabled = true;
                            btnSave.Enabled = true;
                            btnAcquire.Enabled = true;
                            //btnForce.Enabled = false;
                            //btnAdjust.Enabled = false;
                            btnCancelAcquire.Enabled = false;
                            checkBoxAltTrigger.Enabled = true;
                            radioButtonInsufficientQuality.Enabled = true;
                            radioButtonInsufficientObjectCount.Enabled = true;
                            checkBoxAutocontrast.Enabled = true;
                            checkBoxPAD.Enabled = true;
                            checkBox1000dpi.Enabled = true;
                            checkBoxFlexRollCapture.Enabled = true;
                            checkBoxFlexFlatCapture.Enabled = true;
                            checkBoxVisualization.Enabled = true;

                            //btnUserControls.Enabled = true;
                            _imageCaptured = true;    // allow image to be saved

                            _ResetStatusElements();
                            _ResetGuidanceElements();
                            break;

                        case DeviceState.device_opened_and_capture_cancelled:
                            DeviceStatus.Text = "Cancelled";
                            DeviceStatus.BackColor = Color.Green;
                            DeviceStatus.ForeColor = Color.White;

                            comboBox_NumObjCapture.Enabled = true;
                            comboBox_Position.Enabled = true;
                            comboBox_Impression.Enabled = true;
                            btnOpenDevice.Enabled = false;
                            btnCloseDevice.Enabled = true;
                            //btnProperties.Enabled = true;
                            btnSave.Enabled = false;
                            btnAcquire.Enabled = true;
                            //btnForce.Enabled = false;
                            //btnAdjust.Enabled = false;
                            btnCancelAcquire.Enabled = false;
                            checkBoxAltTrigger.Enabled = true;
                            radioButtonInsufficientQuality.Enabled = true;
                            radioButtonInsufficientObjectCount.Enabled = true;
                            checkBoxAutocontrast.Enabled = true;
                            checkBoxPAD.Enabled = true;
                            checkBox1000dpi.Enabled = true;
                            checkBoxFlexRollCapture.Enabled = true;
                            checkBoxFlexFlatCapture.Enabled = true;
                            checkBoxVisualization.Enabled = true;

                            //btnUserControls.Enabled = true;
                            _imageCaptured = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("SetDeviceState update of UI error {0}", ex.Message));
                return;
            }
        }


        private void OpenAPI()
        {
            try
            {
                if (_biobase != null) _biobase.Close(); // Close before re-open

                InitializeBioBase();
                AddMessage("Open BioBase API");

                // Now, it is best to wait for OnDeviceCountChanged event before continuing.
                // OPTION: Could check _biobase.NumberOfDevices in a loop unit a device is attached.
                // Another bad option would be to just *hope* all the devices are ready and get list of device here.
                //_biobaseDevices = _biobase.ConnectedDevices;
                //FillDeviceListBox();
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioB_Open BioBase error {0}", ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("BioB_Open error {0}", ex.Message));
            }
        }

        private void OpenDevice()
        {
            try
            {
                if (_biobase == null)
                {
                    AddMessage("Error, BioBase BioBase API not opened!");
                }
                else
                {
                    if (_biobaseDevice != null)
                    {
                        // Close device before it is Re-opened
                        AddMessage("calling method 'IBioBaseDevice.Dispose'");
                        _biobaseDevice.Dispose();
                        _biobaseDevice = null;

                    }

                    btnOpenDevice.Enabled = false;  // Disable Open device button so only one CreateBioBaseDevice thread is created

                    // Get the selected device that will be opened.
                    // Start a new thread to call to open device!
                    // Don't wait for it to finish because it can take up to 30 seconds for LScan 1000
                    // The thread will enable UI if OpenDevice is successful.
                    DeviceInfoBox.Enabled = false;  // Don't allow change to Device ID when device is open.
                    string selectedDevice = DeviceInfoBox.SelectedItem.ToString();
                    Thread myThread = new Thread(delegate ()
                    {
                        CreateBioBaseDevice(selectedDevice);
                    });
                    myThread.Start();
                    //        myThread.Join();  // Don't wait for thread to exit. Thread will enable UI so we know when to continue.
                    // The _biobaseDevice_Init event will also be called with 100% when device is open and ready
                    // Another less than ideal option would be to call _biobaseDevice.IsDeviceOpen() in a loop.
                }
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioB_OpenDevice BioBase error {0}", ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("BioB_OpenDevice error {0}", ex.Message));
            }
        }

        public void CreateBioBaseDevice(string selectDevice)
        {
            try
            {
                // Check if there is a device already opened
                if (_biobaseDevice == null)
                {

                    if (selectDevice != null)
                    {
                        AddMessage(string.Format("Opening device {0}", selectDevice));

                        // Find selected device in our devices list
                        // Then open the device
                        for (int cnt = 0; cnt < _biobaseDevices.Length; cnt++)
                        {
                            if (_biobaseDevices[cnt].DeviceId == selectDevice)
                            {
                                // InitProgress event is defined in the iBioBase class because the event is triggered before the IBioBaseDevice object is created
                                AddMessage("registering event 'IBioBaseDevice.InitProgress'");
                                _biobase.InitProgress += new EventHandler<BioBaseInitProgressEventArgs>(_biobaseDevice_Init);

                                _deviceType = _biobaseDevices[cnt].ModelName;
                                AddMessage(string.Format("Opening device type {0}", _deviceType));

                                while (true)
                                {
                                    try
                                    {
                                        // Must have try/catch to pick up warning and errors returned by low level BioB_OpenDevice
                                        _biobase.OpenDevice(_biobaseDevices[cnt], out _biobaseDevice);
                                        //NOTE: _biobase.OpenDevice can return with exception because 
                                        //      devImpl.Initialize can return with exception because 
                                        //      Interop.OpenDevice will throw a exception on warnings or errors.
                                        //      catch exception here so UI can notify operator and prompted for action.
                                        break;
                                    }
                                    catch (BioBaseException ex)
                                    {
                                        if (ex.ReturnCode > BioBReturnCode.BIOB_SUCCESS)
                                        {
                                            // device is OPENED but with warning.
                                            // positive return code is a warning, prompt to continue or fix
                                            AddMessage(string.Format("BioB_OpenDevice warning {0}", ex.Message));


                                            if (ex.ReturnCode == BioBReturnCode.BIOB_OPTICS_SURFACE_DIRTY)
                                            {
                                                // with BIOB_OPTICS_SURFACE_DIRTY return code, there are three options.
                                                // 1. Ignore the dirty platen; continue with open (break)
                                                // 2. abort open device (return)
                                                // 3. Have operator clean the device platen and re-open device to re-check cleanliness (continue)
                                                m_bMsgBoxOpened = true;
                                                string msg = string.Format("BioB_OpenDevice warning {0}.   Clean Device and retry?", ex.Message);
                                                DialogResult result = MessageBox.Show(msg, "BioB_OpenDevice", MessageBoxButtons.AbortRetryIgnore);
                                                m_bMsgBoxOpened = false;
                                                if (result == System.Windows.Forms.DialogResult.Ignore)
                                                    break;  // 1. Ignore the dirty platen; continue with open (break)
                                                else if (result == System.Windows.Forms.DialogResult.Abort)
                                                { // 2. abort open device (return)
                                                    CloseDevice();
                                                    return;     //Close device and return without initializing UI
                                                }
                                                else
                                                { // 3. Have operator clean the device platen and re-open device to re-check cleanliness (continue)
                                                    CloseDevice();
                                                    continue;     // Close device and retry openning device after the platen was cleaned
                                                }
                                            }
                                            else if (ex.ReturnCode == BioBReturnCode.BIOB_REPLACE_PAD)
                                            {
                                                // with BIOB_REPLACE_PAD return code, there are three options.
                                                // 1. Ignore the warning to replace silicone membrane; continue with open (break)
                                                // 2. abort open device (return)
                                                // 3. Have operator replace the silicone membrane, reset the membrane usage and re-open device to check cleanliness of new membrane (continue)
                                                string msg = string.Format("BioB_OpenDevice warning {0}.   Replace silicone membrane and retry?", ex.Message);
                                                DialogResult result = MessageBox.Show(msg, "BioB_OpenDevice", MessageBoxButtons.YesNoCancel);
                                                if (result == System.Windows.Forms.DialogResult.No)
                                                    break;  // 1. Ignore the warning to replace silicone membrane; continue with open (break)
                                                else if (result == System.Windows.Forms.DialogResult.Cancel)
                                                { // 2. abort open device (return)
                                                    CloseDevice();
                                                    return; //Close device and return without initializing UI
                                                }
                                                else
                                                {
                                                    // 3. Have operator replace the silicone membrane, reset the membrane usage and re-open device to check cleanliness of new membrane (continue)

                                                    //OPTIONAL get properties of silicone membrane life expectancy with four GetProperty calls.
                                                    string usageMax = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_SILICONE_MEMBRANE_MAX_USAGE_COUNT);
                                                    string usageCurrent = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_SILICONE_MEMBRANE_CURRENT_USAGE_COUNT);
                                                    string daysMax = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_SILICONE_MEMBRANE_MAX_LIFE_DAYS);
                                                    string daysCurrent = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_SILICONE_MEMBRANE_REPLACEMENT_DATE);

                                                    // Prompt the operator to replace the silicone memberane and then clicked OK button
                                                    // IF the OK button is pressed, the silicone membrane counter will be reset for another 8000 captures
                                                    // IF the OK button was not pressed, the silicone membrane counter warning message will be displayed again on the next attempt to start capture.
                                                    if (MessageBox.Show("Replace silicone membrane then click OK,", "Replace silicone membrane", MessageBoxButtons.OK) == DialogResult.OK)
                                                        _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_SILICONE_MEMBRANE_REPLACE, PropertyConstants.DEV_PROP_TRUE);
                                                    CloseDevice();
                                                    continue;     // Close device and retry openning device after the platen was cleaned
                                                                  // break;     // we don't want to continue with open because we need check cleanliness of new silicone membrane
                                                }
                                            }
                                            else
                                            {
                                                // with unknown warning return code, there are three options.
                                                // 1. Ignore the warning (break)
                                                // 2. abort open device (return)
                                                string msg = string.Format("BioB_OpenDevice warning {0} - Continue?", ex.Message);
                                                DialogResult result = MessageBox.Show(msg, "BioB_OpenDevice", MessageBoxButtons.YesNo);
                                                if (result == System.Windows.Forms.DialogResult.Yes)
                                                    break;  // 1. Ignore the warning; continue with open (break)
                                                if (result == System.Windows.Forms.DialogResult.No)
                                                {
                                                    CloseDevice();
                                                    return; // 2. abort open device (return)
                                                }
                                            }
                                        }


                                        else if (ex.ReturnCode < BioBReturnCode.BIOB_SUCCESS)
                                        {
                                            if (ex.ReturnCode == BioBReturnCode.BIOB_OBJECT_ON_PLATEN_DURING_CALIBRATION)
                                            {
                                                // device open failed with BIOB_OBJECT_ON_PLATEN_DURING_CALIBRATION error.
                                                AddMessage(string.Format("BioB_OpenDevice Error {0}. Object or excessive dirt detected on the platen during the collection of background images.", ex.Message));
                                                // with BIOB_OBJECT_ON_PLATEN_DURING_CALIBRATION return code, there are two options.
                                                // 1. Have operator clean the device platen and re-open device to re-check cleanliness (continue
                                                // 2. abort open device (return)
                                                string msg = string.Format("BioB_OpenDevice Error {0}. Retry by removing object from platen, cleaning and clicking Yes. Click No to quit", ex.Message);
                                                DialogResult result = MessageBox.Show(msg, "BioB_OpenDevice", MessageBoxButtons.YesNo);
                                                if (result == System.Windows.Forms.DialogResult.Yes)
                                                {
                                                    //1. Have operator clean the device platen and re-open device to re-check cleanliness (continue)
                                                    continue;
                                                }
                                            }
                                            {
                                                // device is NOT OPENED because of error.
                                                // If negative error, Close device to clean up and return without initializing UI
                                                CloseDevice();
                                                if (ex.ReturnCode != BioBReturnCode.BIOB_OBJECT_ON_PLATEN_DURING_CALIBRATION)
                                                {
                                                    string msg = string.Format("Device closed. Fix BioB_OpenDevice error {0} and try again", ex.Message);
                                                    AddMessage(msg);
                                                    MessageBox.Show(msg, "BioBase4 Open Device", MessageBoxButtons.OK);
                                                }
                                                return;
                                            }
                                        }
                                    }
                                } //while(true)
                                break;
                            }
                        }

                        if (_biobaseDevice != null)
                        {
                            //OpenDevice was successful, now update the UI
                            RegisterEventHandlers();

                            if (DeviceInfoBox.InvokeRequired)
                                DeviceInfoBox.Invoke(new EnableControlDelegate(EnableControl), new object[] { DeviceInfoBox, false });
                            else
                                EnableControl(DeviceInfoBox, false);

                            //////////////////////////////////////////
                            // Fill Position combo box for this device
                            string supportedPositions = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_AVAILABLE_POSITION_TYPES);
                            string[] positionArray = supportedPositions.Split(' ');

                            //comboBox_Position.Items.Clear();
                            if (comboBox_Position.InvokeRequired)
                                comboBox_Position.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Position, comboBoxMethod.Clear, null });
                            else
                                UpdatecomboBox(comboBox_Position, comboBoxMethod.Clear, null);

                            foreach (string pos in positionArray)
                            {
                                //comboBox_Position.Items.Add(pos);
                                if (comboBox_Position.InvokeRequired)
                                    comboBox_Position.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Position, comboBoxMethod.ItemsAdd, pos });
                                else
                                    UpdatecomboBox(comboBox_Position, comboBoxMethod.ItemsAdd, pos);
                            }

                            ////////////////////////////////////////////
                            // Fill Impression combo box for this device
                            string supportedImpressions = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_AVAILABLE_IMPRESSION_TYPES);
                            string[] impressionsArray = supportedImpressions.Split(' ');

                            //comboBox_Impression.Items.Clear();
                            if (comboBox_Impression.InvokeRequired)
                                comboBox_Impression.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Impression, comboBoxMethod.Clear, null });
                            else
                                UpdatecomboBox(comboBox_Impression, comboBoxMethod.Clear, null);

                            foreach (string impress in impressionsArray)
                            {
                                //comboBox_Impression.Items.Add(impress);
                                if (comboBox_Impression.InvokeRequired)
                                    comboBox_Impression.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Impression, comboBoxMethod.ItemsAdd, impress });
                                else
                                    UpdatecomboBox(comboBox_Impression, comboBoxMethod.ItemsAdd, impress);
                            }

                            // Option to select the first valid entry in each of the combo boxes so we can save time and quickly click Acquire
                            //select first valid position in list
                            //comboBox_Position.SelectedIndex = 1;
                            if (comboBox_Position.InvokeRequired)
                                comboBox_Position.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Position, comboBoxMethod.SelectedIndex, "1" });
                            else
                                UpdatecomboBox(comboBox_Position, comboBoxMethod.SelectedIndex, "1");

                            //comboBox_Impression.SelectedIndex = 1;
                            if (comboBox_Impression.InvokeRequired)
                                comboBox_Impression.Invoke(new UpdatecomboBoxDelegate(UpdatecomboBox), new object[] { comboBox_Impression, comboBoxMethod.SelectedIndex, "1" });
                            else
                                UpdatecomboBox(comboBox_Impression, comboBoxMethod.SelectedIndex, "1");

                            //Set option to check for spoof detection AKA presentation attack detection (PAD) 
                            //Only allo option to be set if supported by device else BeginAcquire will return an error
                            bool bspoof;
                            if (_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_SPOOF_DETECTION_SUPPORTED) == PropertyConstants.DEV_PROP_TRUE)
                                bspoof = true;
                            else
                                bspoof = false;
                            if (checkBoxPAD.InvokeRequired)
                            {
                                checkBoxPAD.Invoke(new EnableControlDelegate(EnableControl), new object[] { checkBoxPAD, bspoof });
                                checkBoxPAD.Invoke(new CheckedControlDelegate(CheckedControl), new object[] { checkBoxPAD, bspoof });
                            }
                            else
                            {
                                EnableControl(checkBoxPAD, bspoof);
                                CheckedControl(checkBoxPAD, bspoof);
                            }

                            //Set option to allow the image resolution to be changed
                            // Checked if device supports setting else the SetProperty will throw exception
                            string resolution = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_AVAILABLE_IMAGE_RESOLUTIONS);
                            bool bdpi;
                            if (resolution.Contains(PropertyConstants.DEV_PROP_RESOLUTION_1000))
                                bdpi = true;
                            else
                                bdpi = false;
                            if (checkBox1000dpi.InvokeRequired)
                            {
                                checkBox1000dpi.Invoke(new EnableControlDelegate(EnableControl), new object[] { checkBox1000dpi, bdpi });
                                checkBox1000dpi.Invoke(new CheckedControlDelegate(CheckedControl), new object[] { checkBox1000dpi, bdpi });
                            }
                            else
                            {
                                EnableControl(checkBox1000dpi, bdpi);
                                CheckedControl(checkBox1000dpi, bdpi);
                            }

                            //Reset device's LEDs, TFT display or Touch display here at start if previous cycle didn't turn everything off
                            _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
                            _ResetStatusElements();
                            _ResetGuidanceElements();

                            m_scannerOpen = true;
                            AddMessage("OpenDevice succeeded");
                            SetDeviceState(DeviceState.device_opened_and_not_live);
                        }
                    }
                }
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("InitializeBioBase IBioBaseDevice error {0}", ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("InitializeBioBase IBioBaseDevice error {0}", ex.Message));
            }
            finally
            {
            }
        }

        //*****************************************************//
        public void SetUILedColors(ActiveColor led1, ActiveColor led2, ActiveColor led3, ActiveColor led4, ActiveColor led5)
        {
            if (LEDlightPanel.InvokeRequired)
                // Invoke when called from events outside of GUI thread
                Invoke((Action<ActiveColor, ActiveColor, ActiveColor, ActiveColor, ActiveColor>)SetUILedColors, led1, led2, led3, led4, led5);
            else
            {
                LEDlightPanel.SetUILedColors(led1, led2, led3, led4, led5);
            }
        }

        /*!
         * \fn public void SetUILedColors()
         * \brief Set the number of status LEDs being displayed.
         * The number of LEDs can be between 0 and 5.
         */
        public void SetUILedColors(int count)
        {
            LEDlightPanel.LedCount = count;
        }
        /*!
         * \fn public void ConvertQualityToIndicatorColor()
         * \brief Determine the color of the UI status LEDs based on quality from _biobaseDevice_ObjectQuality event.
         */
        static public ActiveColor ConvertQualityToIndicatorColor(BioBObjectQualityState qualityCode)
        {
            ActiveColor retval = ActiveColor.gray;
            switch (qualityCode)
            {
                case BioBObjectQualityState.BIOB_OBJECT_NOT_PRESENT:
                    retval = ActiveColor.gray;
                    break;
                case BioBObjectQualityState.BIOB_OBJECT_TOO_LIGHT:
                case BioBObjectQualityState.BIOB_OBJECT_TOO_DARK:
                case BioBObjectQualityState.BIOB_OBJECT_BAD_SHAPE:
                    retval = ActiveColor.red;
                    break;
                case BioBObjectQualityState.BIOB_OBJECT_GOOD:
                    retval = ActiveColor.green;
                    break;
                case BioBObjectQualityState.BIOB_OBJECT_POSITION_NOT_OK:
                case BioBObjectQualityState.BIOB_OBJECT_CORE_NOT_PRESENT:
                case BioBObjectQualityState.BIOB_OBJECT_TRACKING_NOT_OK:
                case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_HIGH:
                case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_LEFT:
                case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_RIGHT:
                case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_LOW:
                case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_HIGH:
                case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_LEFT:
                case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_RIGHT:
                case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_LOW:
                case BioBObjectQualityState.BIOB_OBJECT_OCCLUSION:
                case BioBObjectQualityState.BIOB_OBJECT_CONFUSION:
                case BioBObjectQualityState.BIOB_OBJECT_ROTATED_CLOCKWISE:
                case BioBObjectQualityState.BIOB_OBJECT_ROTATED_COUNTERCLOCKWISE:
                    retval = ActiveColor.yellow;
                    break;
                default:
                    retval = ActiveColor.red;
                    break;
            }
            return retval;
        }

        private void _ResetStatusElements()
        {
            switch (_biobaseDevice.deviceGuidanceType)
            {
                case guidanceType.guidanceTypeNone:
                    break;
                case guidanceType.guidanceTypeLScan:
                    _ResetLScanLEDs();
                    break;
                case guidanceType.guidanceTypeStatusLED:
                    _ResetStatusLEDs();
                    break;
                case guidanceType.guidanceTypeTFT:
                case guidanceType.guidanceTypeTFT_1000:
                    _TftShowCompanyLogo();
                    break;
                case guidanceType.guidanceTypeTouchDisplay:
                    _ResetTouchDisplay();
                    break;
            }
        }

        /*!
         * \fn private void _ResetGuidanceElements()
         * \brief Reset any guidance/icon elements on the fingerprint device
         * Determine the type of device guidance/icon elements and call the appropriate method.
         */
        private void _ResetGuidanceElements()
        {
            switch (_biobaseDevice.deviceGuidanceType)
            {
                case guidanceType.guidanceTypeNone:
                    break;
                case guidanceType.guidanceTypeLScan:
                    _ResetLScanLEDs();
                    break;
                case guidanceType.guidanceTypeStatusLED:
                    // Not required. Both status and icon LEDs are reset by _ResetStatusLEDs();   //a.k.a. _ResetIconLEDs();
                    break;
                case guidanceType.guidanceTypeTFT:
                case guidanceType.guidanceTypeTFT_1000:
                    // Not required. Done in _ResetStatusElements by _TftShowCompanyLogo();
                    break;
                case guidanceType.guidanceTypeTouchDisplay:
                    // Not required. Done in _ResetStatusElements by _ResetTouchDisplay();
                    break;
            }
        }

        //******************** SetOutputData***************************//
        #region DeviceSounds


        /*!
         * \fn private void _BeepOK()
         * \brief Send beep pattern #3 to the device at full volume
         */
        private void _BeepOK()
        {
            _Beep("3", "100");
        }

        /*!
         * \fn private void _BeepError()
         * \brief Send beep pattern #1 to the device  at full volume
         * NOTE: if the developer wishes to view the XML forma structure, the xmlDoc.OuterXml string can be passed to the AddMessage method 
         * Catch BioBaseException and Exception and log errors
         */
        private void _BeepError()
        {
            _Beep("1", "100");
        }

        /*!
         * \fn private void _Beep()
         * \brief Send beep pattern to the device
         * NOTE: if the developer wishes to view the XML forma structure, the xmlDoc.OuterXml string can be passed to the AddMessage method 
         * 
         * Typical XML for beep command:
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         * -<OutputData>
         *   <Beeper Volume="100" Pattern="3"/>
         * </OutputData>
         * </BioBase>
         * 
         * Catch BioBaseException and Exception and log errors
         */
        private void _Beep(string pattern, string volume)
        {
            try
            {
                // create xml tree
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                XmlElement beeperElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_BEEPER);
                beeperElem.SetAttribute(PropertyConstants.OUT_DATA_BEEP_PATTERN, pattern);
                beeperElem.SetAttribute(PropertyConstants.OUT_DATA_BEEP_VOLUME, volume);

                xmlDoc.DocumentElement.AppendChild(outputDataElem);
                outputDataElem.AppendChild(beeperElem);

                AddMessage(xmlDoc.OuterXml);  //option to log the formated XML string that is sent to the devices.

                _PerformUserOutput(xmlDoc.OuterXml);        //Send XML to device

            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_BEEPER, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_BEEPER, ex.Message));
            }
        }

        #endregion




        /*!
         * \fn private void _SetFinalStatusElements()
         * \brief Set any status on device with _e.DataStatus and _biobaseDevice.mostResentKey
         * \param DataStatus final image status from _biobaseDevice_DataAvailable
         * gloabal input _biobaseDevice.mostResentKey
         * Called from the _biobaseDevice_DataAvailable event handler
         * Determine the type of device status elements and call the appropriate method.
         */
        private void _SetFinalStatusElements(BioBReturnCode DataStatus)
        {
            switch (_biobaseDevice.deviceGuidanceType)
            {
                case guidanceType.guidanceTypeNone:
                    break;
                case guidanceType.guidanceTypeLScan:
                    _SetLScanStatusLEDs(DataStatus);
                    break;
                case guidanceType.guidanceTypeStatusLED:
                    _SetStatusLEDs(DataStatus);
                    break;
                case guidanceType.guidanceTypeTFT:
                case guidanceType.guidanceTypeTFT_1000:
                    _SetTftStatus(DataStatus);
                    break;
                case guidanceType.guidanceTypeTouchDisplay:
                    _SetTouchStatus(DataStatus);
                    break;
            }
        }


        /*!
         * \fn private void _SetStatusElements()
         * \brief Set any status elements on the fingerprint device with mostRecentqualities and mostResentKey
         * Called from the _biobaseDevice_ObjectQuality event handler
         * Determine the type of device status elements and call the appropriate method.
         */
        private void _SetStatusElements()
        {
            switch (_biobaseDevice.deviceGuidanceType)
            {
                case guidanceType.guidanceTypeNone:
                    break;
                case guidanceType.guidanceTypeLScan:
                    _SetLScanStatusLEDs();
                    break;
                case guidanceType.guidanceTypeStatusLED:
                    _SetStatusLEDs();
                    break;
                case guidanceType.guidanceTypeTFT:
                case guidanceType.guidanceTypeTFT_1000:
                    _SetTftStatus();
                    break;
                case guidanceType.guidanceTypeTouchDisplay:
                    _SetTouchStatus();
                    break;
            }
        }

        /*!
         * \fn private void _SetGuidanceElements()
         * \brief Set any guidance/icon elements on the fingerprint device 
         * Typically called from Begin Acquisition Process with position and impression being captured.
         * Determine the type of device guidance/icon elements and call the appropriate method.
         */
        private void _SetGuidanceElements()
        {
            switch (_biobaseDevice.deviceGuidanceType)
            {
                case guidanceType.guidanceTypeNone:
                    break;
                case guidanceType.guidanceTypeLScan:
                    _SetLScanLEDs();
                    break;
                case guidanceType.guidanceTypeStatusLED:
                    _SetIconLEDs();
                    break;
                case guidanceType.guidanceTypeTFT:
                case guidanceType.guidanceTypeTFT_1000:
                    _SetTftGuidance();
                    break;
                case guidanceType.guidanceTypeTouchDisplay:
                    _SetTouchGuidance();
                    break;
            }
        }


        #region LScanLED
        /*!
         * \fn private void _ResetLScanLEDs()
         * \brief place holder to turn off the status and LED on legacy LScan 1000P and LScan 1000T
         * The end of support for the LScan 1000T was 31, March 2018
         * The end of support for the LScan 1000P was 31, December 2016
         */
        private void _ResetLScanLEDs()
        {
            //TODO: turn off the status and LED on legacy LScan 1000P and LScan 1000T
        }

        /*!
         * \fn private void _SetLScanStatusLEDs()
         * \brief place holder to control status on legacy LScan 1000P and LScan 1000T based on _biobaseDevice.mostRecentqualities
         * The end of support for the LScan 1000T was 31, March 2018
         * The end of support for the LScan 1000P was 31, December 2016
         */
        private void _SetLScanStatusLEDs()
        {
            //TODO:control status on legacy LScan 1000P and LScan 1000T 
        }
        /*!
         * \fn private void _SetLScanStatusLEDs()
         * \brief place holder to control FINAL status on legacy LScan 1000P and LScan 1000T based on e.DataStatus
         * The end of support for the LScan 1000T was 31, March 2018
         * The end of support for the LScan 1000P was 31, December 2016
         */
        private void _SetLScanStatusLEDs(BioBReturnCode DataStatus)
        {
            //TODO: control FINAL status on legacy LScan 1000P and LScan 1000T
        }

        /*!
         * \fn private void _SetIconLEDs()
         * \brief place holder to contorl LED based on position in legacy LScan 1000P and LScan 1000T.
         * Settings based on _biobaseDevicemostRecentPosition and _biobaseDevice.mostRecentImpression.
         * The end of support for the LScan 1000T was 31, March 2018
         * The end of support for the LScan 1000P was 31, December 2016
         */
        private void _SetLScanLEDs()
        {
            //TOD: contorl LED based on position in legacy LScan 1000P and LScan 1000T.
        }
        #endregion


        #region StatusAndIconLEDs
        // StatusAndIconLEDs - Status and Icon LED on LScan Guardian USB, Guardian F, Guardian L, Guardian R2, Patrol, Patrol ID, etc.

        /*!
         * \fn private void _SetStatusLEDs()
         * \brief Turn on the Device's status LEDs based on changes to the BioBObjectQualityState object
         * Then, the BioBase API requires that we reset Icon LEDs based on position and impression.
         * This method will each LED to use  non blinking LED colors!
         * global input _biobaseDevice.mostRecentqualities[] used to update status LEDs on device. 
         * global input _biobaseDevice.mostRecentPosition, _biobaseDevice.mostRecentImpression used to set icon LEDs on device.
         * These devices don't have keys to program with _mostResentKey
         * NOTE:  setting PropertyConstants.OUT_DATA_LED_S1_RED_B1 will give red slow blinking,
         *        setting PropertyConstants.OUT_DATA_LED_S1_RED_B2 will give red flash blinking, and
         *        setting both PropertyConstants.OUT_DATA_LED_S1_RED_B1 and PropertyConstants.OUT_DATA_LED_S1_RED_B2 will give red non-blinking
         *        
         * Typical XML format to Right thumb roll capture with solid green LEDs and red S1 status LED
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<StatusLeds>
         *    <Led>NONE</Led>
         *
         *    <Led>S1_RED_B1</Led>
         *    <Led>S1_RED_B2</Led>
         *
         *    <Led>I4_GREEN_B1</Led>
         *    <Led>I4_GREEN_B2</Led>
         *   </StatusLeds>
         *  </OutputData>
         * </BioBase>
         *
         * Catch BioBaseException and Exception and log errors
         */
        private void _SetStatusLEDs()
        {
            try
            {
                // create xml tree based on _mostRecentqualities 
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);

                XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_STATUSLEDS);
                outputDataElem.AppendChild(statusDataElem);

                {
                    // turn off all status LEDs
                    _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_NONE);

                    // Turn on first status LED bases on BioBObjectQualityState qualities[0]
                    if (_biobaseDevice.mostRecentqualities.Length > 0)
                    { // Set first status led
                        _AddUserOutputStatusLed(xmlDoc, statusDataElem, _biobaseDevice.mostRecentqualities[0],
                                                null, null, //unknown
                                                PropertyConstants.OUT_DATA_LED_S1_RED_B1, PropertyConstants.OUT_DATA_LED_S1_RED_B2,  //Error color - non blinking Red - too dark, too light, bad shape, bad position, etc.
                                                PropertyConstants.OUT_DATA_LED_S1_RED_B1, PropertyConstants.OUT_DATA_LED_S1_RED_B2, PropertyConstants.OUT_DATA_LED_S1_GREEN_B1, PropertyConstants.OUT_DATA_LED_S1_GREEN_B2, //Not OK - non blinking Yellow - Not tracking
                                                PropertyConstants.OUT_DATA_LED_S1_GREEN_B1, PropertyConstants.OUT_DATA_LED_S1_GREEN_B2  //OK - non blinking Green - object good / correct pressure
                                                );
                    }
                    if (_biobaseDevice.mostRecentqualities.Length > 1)
                    { // set second status led
                        _AddUserOutputStatusLed(xmlDoc, statusDataElem, _biobaseDevice.mostRecentqualities[1],
                                                null, null, //unknown
                                                PropertyConstants.OUT_DATA_LED_S2_RED_B1, PropertyConstants.OUT_DATA_LED_S2_RED_B2,
                                                PropertyConstants.OUT_DATA_LED_S2_RED_B1, PropertyConstants.OUT_DATA_LED_S2_RED_B2, PropertyConstants.OUT_DATA_LED_S2_GREEN_B1, PropertyConstants.OUT_DATA_LED_S2_GREEN_B2,
                                                PropertyConstants.OUT_DATA_LED_S2_GREEN_B1, PropertyConstants.OUT_DATA_LED_S2_GREEN_B2);
                    }
                    if (_biobaseDevice.mostRecentqualities.Length > 2)
                    { // set third status led
                        _AddUserOutputStatusLed(xmlDoc, statusDataElem, _biobaseDevice.mostRecentqualities[2],
                                                null, null, //unknown
                                                PropertyConstants.OUT_DATA_LED_S3_RED_B1, PropertyConstants.OUT_DATA_LED_S3_RED_B2,
                                                PropertyConstants.OUT_DATA_LED_S3_RED_B1, PropertyConstants.OUT_DATA_LED_S3_RED_B2, PropertyConstants.OUT_DATA_LED_S3_GREEN_B1, PropertyConstants.OUT_DATA_LED_S3_GREEN_B2,
                                                PropertyConstants.OUT_DATA_LED_S3_GREEN_B1, PropertyConstants.OUT_DATA_LED_S3_GREEN_B2);
                    }
                    if (_biobaseDevice.mostRecentqualities.Length > 3)
                    { // set fourth status led
                        _AddUserOutputStatusLed(xmlDoc, statusDataElem, _biobaseDevice.mostRecentqualities[3],
                                                null, null, //unknown
                                                PropertyConstants.OUT_DATA_LED_S4_RED_B1, PropertyConstants.OUT_DATA_LED_S4_RED_B2,
                                                PropertyConstants.OUT_DATA_LED_S4_RED_B1, PropertyConstants.OUT_DATA_LED_S4_RED_B2, PropertyConstants.OUT_DATA_LED_S4_GREEN_B1, PropertyConstants.OUT_DATA_LED_S4_GREEN_B2,
                                                PropertyConstants.OUT_DATA_LED_S4_GREEN_B1, PropertyConstants.OUT_DATA_LED_S4_GREEN_B2);
                    }

                    // Must also set icon LEDs again!!!!!
                    _SetIcon(xmlDoc, statusDataElem, _biobaseDevice.mostRecentPosition, _biobaseDevice.mostRecentImpression);
                }

                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
            }
        }

        /*!
         * \fn private void _SetStatusLEDs()
         * \brief Turn on the Device's status LEDs based on changes to the e.DataStatus
         * Then, the BioBase API requires that we reset Icon LEDs based on position and impression.
         * This method will each LED to use  non blinking LED colors!
         * \param DataStatus final image status from _biobaseDevice_DataAvailable
         * global input _biobaseDevice.mostRecentPosition and _biobaseDevice.mostRecentImpression used to set icon LEDs on device.
         * These devices don't have keys to program with _biobaseDevice.mostResentKey
         * NOTE:  setting PropertyConstants.OUT_DATA_LED_S1_RED_B1 will give red slow blinking,
         *        setting PropertyConstants.OUT_DATA_LED_S1_RED_B2 will give red flash blinking, and
         *        setting both PropertyConstants.OUT_DATA_LED_S1_RED_B1 and PropertyConstants.OUT_DATA_LED_S1_RED_B2 will give red non-blinking
         *
         * Typical XML output for right thumb roll captured print roll warning flashing all status LEDs
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<StatusLeds>
         *    <Led>NONE</Led>
         *    <Led>S1_RED_B2</Led>
         *    <Led>S1_GREEN_B2</Led>
         *    <Led>S2_RED_B2</Led>
         *    <Led>S2_GREEN_B2</Led>
         *    <Led>S3_RED_B2</Led>
         *    <Led>S3_GREEN_B2</Led>
         *    <Led>S4_RED_B2</Led>
         *    <Led>S4_GREEN_B2</Led>
         *
         *    <Led>I4_GREEN_B1</Led>
         *    <Led>I4_GREEN_B2</Led>
         *   </StatusLeds>
         *  </OutputData>
         * </BioBase>
         *
         * Typical XML output for right thumb roll captured print roll succeful with all status LEDs green
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<StatusLeds>
         *    <Led>NONE</Led>
         *    <Led>S1_GREEN_B1</Led>
         *    <Led>S2_GREEN_B1</Led>
         *    <Led>S3_GREEN_B1</Led>
         *    <Led>S4_GREEN_B1</Led>
         *
         *    <Led>I4_GREEN_B1</Led>
         *    <Led>I4_GREEN_B2</Led>
         *    </StatusLeds>
         *    </OutputData>
         *    </BioBase>
         *    
         * Catch BioBaseException and Exception and log errors
         */
        private void _SetStatusLEDs(BioBReturnCode DataStatus)
        {
            try
            {
                // create xml tree based on mostRecentqualities 
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);

                XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_STATUSLEDS);
                outputDataElem.AppendChild(statusDataElem);

                {
                    // turn off all status LEDs
                    _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_NONE);

                    if (DataStatus > 0)
                    { // warning flash yellow
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S1_RED_B2);  // flash first LED
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S1_GREEN_B2);  // flash first LED
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S2_RED_B2);  // set second status led
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S2_GREEN_B2);  // set second status led
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S3_RED_B2);  // set third status led
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S3_GREEN_B2);  // set third status led
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S4_RED_B2);  // set fourth status led
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S4_GREEN_B2);  // set fourth status led
                    }
                    else if (DataStatus == BioBReturnCode.BIOB_SUCCESS)
                    { // successfull - blink green
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S1_GREEN_B1);  // Set first status led
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S2_GREEN_B1);  // set second status led
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S3_GREEN_B1);  // set third status led
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S4_GREEN_B1);  // set fourth status led
                    }
                    if (DataStatus < 0)
                    { // warning flash red error
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S1_RED_B2);  // flash first LED
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S2_RED_B2);  // set second status led
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S3_RED_B2);  // set third status led
                        _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_S4_RED_B2);  // set fourth status led
                    }


                    // Must also set icon LEDs again!!!!!
                    _SetIcon(xmlDoc, statusDataElem, _biobaseDevice.mostRecentPosition, _biobaseDevice.mostRecentImpression);
                }

                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
            }
        }

        /*!
         * \fn private void _ResetStatusLEDs()
         * \brief Turn off the status and Icon LEDs.
         * 
         * Typical XML format to reset LEDs on device
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<StatusLeds>
         *    <Led>NONE</Led>
         *   </StatusLeds>
         *  </OutputData>
         *  </BioBase>
         * 
         * Catch BioBaseException and Exception and log errors
         */
        private void _ResetStatusLEDs()
        {
            try
            {
                // create xml tree
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);

                XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_STATUSLEDS);
                outputDataElem.AppendChild(statusDataElem);

                // turn off ALL status AND icon LEDs
                _AddUserOutputElement(xmlDoc, statusDataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_NONE);

                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
            }
        }

        /*!
         * \fn private void _AddUserOutputStatusLed()
         * \brief use LED inputs strings to set approprate status based on quality input
         * Matches the quality value with the LED string to send to the device.
         * Method called once for each status LED on device
         */
        private void _AddUserOutputStatusLed(
                                  XmlDocument xmlDoc,  ///< [in]  top level XML document for elements
                              XmlElement Parent,   ///< [in]  XML element to add element to
                              BioBObjectQualityState quality,
                                  string ledUnknown1, string ledUnknown2,
                                  string ledFingerError1, string ledFingerError2,    //Error color - non blinking Red - too dark, too light, bad shape, bad position, etc.
                                  string ledTrackingNotOk1, string ledTrackingNotOk2, string ledTrackingNotOk3, string ledTrackingNotOk4,   //Not OK - non blinking Yellow - Not tracking
                                  string ledOk1, string ledOk2 //OK - non blinking Green - object good / correct pressure
                                  )
        {
            string[] pLed = { null, null, null, null };  // each led can have upto 4 different color and flashing rate

            switch (quality)
            {
                case BioBObjectQualityState.BIOB_OBJECT_TOO_DARK:
                case BioBObjectQualityState.BIOB_OBJECT_TOO_LIGHT:
                case BioBObjectQualityState.BIOB_OBJECT_BAD_SHAPE:
                case BioBObjectQualityState.BIOB_OBJECT_POSITION_NOT_OK:
                case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_HIGH:
                case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_LEFT:
                case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_RIGHT:
                case BioBObjectQualityState.BIOB_OBJECT_CORE_NOT_PRESENT:
                case BioBObjectQualityState.BIOB_OBJECT_CONFUSION:
                case BioBObjectQualityState.BIOB_OBJECT_ROTATED_CLOCKWISE:
                case BioBObjectQualityState.BIOB_OBJECT_ROTATED_COUNTERCLOCKWISE:
                    // Set ERROR LED color and phase
                    pLed[0] = ledFingerError1;
                    pLed[1] = ledFingerError2;
                    break;
                case BioBObjectQualityState.BIOB_OBJECT_TRACKING_NOT_OK:
                    // Set NOT OK LED color and phase
                    pLed[0] = ledTrackingNotOk1;
                    pLed[1] = ledTrackingNotOk2;
                    pLed[2] = ledTrackingNotOk3;
                    pLed[3] = ledTrackingNotOk4;
                    break;
                case BioBObjectQualityState.BIOB_OBJECT_GOOD:
                    // Set OK LED color and phase
                    pLed[0] = ledOk1;
                    pLed[1] = ledOk2;
                    break;
                case BioBObjectQualityState.BIOB_OBJECT_NOT_PRESENT:
                    // Set unknown LED color and phase (often none/LED off)
                    pLed[0] = ledUnknown1;
                    pLed[1] = ledUnknown2;
                    break;
                default:
                    break;
            }

            for (int i = 0; i < 4; i++)
            {
                if (pLed[i] != null)
                    _AddUserOutputElement(xmlDoc, Parent, PropertyConstants.OUT_DATA_LED, pLed[i]);
            }
        }

        /*!
         * \fn private void _SetIconLEDs()
         * \brief Turn on Icon LED based on position.
         * Icon LEDs will be non blinking Green
         * global input _biobaseDevice.mostRecentPosition and _biobaseDevice.mostRecentImpression used to set icon LEDs on device.
         * 
         * Typical XML format to Right thumb capture with solid green LEDs
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<StatusLeds>
         *    <Led>NONE</Led>
         *    <Led>I4_GREEN_B1</Led>
         *    <Led>I4_GREEN_B2</Led>
         *   </StatusLeds>
         *  </OutputData>
         * </BioBase>
         * 
         * Catch BioBaseException and Exception and log errors
         */
        private void _SetIconLEDs()
        {
            try
            {
                // create xml tree
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);

                XmlElement DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_STATUSLEDS);
                outputDataElem.AppendChild(DataElem);

                // turn off ALL Status and Icon LEDs
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_NONE);

                _SetIcon(xmlDoc, DataElem, _biobaseDevice.mostRecentPosition, _biobaseDevice.mostRecentImpression);

                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_STATUSLEDS, ex.Message));
            }
        }

        /*!
         * \fn private void _SetIcon()
         * \brief set individual XML for icon LED based on position.
         * This method does not include the XML header so it can be used to set initial icon LED and also with status LEDs.
         * Icon LEDs will be non blinking Green
         * 
         * Exceptions passed to calling method
         */
        private void _SetIcon(XmlDocument xmlDoc, XmlElement DataElem, string postion, string impression)
        {
            // Turn on first status LED bases on BioBObjectQualityState qualities[0]
            if (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB)
            {
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I4_GREEN_B1);
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I4_GREEN_B2);
            }
            else if ((postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS)   // Generic two fingers so in this application it will assume right and left index 
              )
            {
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I3_GREEN_B1);
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I3_GREEN_B2);
            }

            else if (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB)
            {
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I2_GREEN_B1);
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I2_GREEN_B2);
            }
            else if ((postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE) ||
                (postion == PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE))
            {
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I1_GREEN_B1);
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I1_GREEN_B2);
            }

            else if (postion == PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS)
            { //PreTriggerMessage = "Place 2 thumbs!";
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I2_GREEN_B1);
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I2_GREEN_B2);
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I4_GREEN_B1);
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I4_GREEN_B2);
            }
            else if (postion == PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES)
            { //PreTriggerMessage = "Place Flat 4: Left Middle + Left Index + Right Index + Right Middle!";
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I1_GREEN_B1);
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I1_GREEN_B2);
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I3_GREEN_B1);
                _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_LED, PropertyConstants.OUT_DATA_LED_I3_GREEN_B2);
            }
        }
        #endregion


        #region TFTDisplay
        //TFTDisplay - control display on LScan 500P, LScan 500, and LScan 1000PX, LScan 1000, LScan 500, etc

        /// <summary>
        /// Class TFT_ObjectDictionary is the L SCAN palm scanner TFT segment object definitions.
        /// Each segment of the TFT display is controlled independently.
        /// This class defines each of the segments that are available in the FingerSelectionScreen formated screen.
        ///
        /// Base TFT_ObjectDictionary class used for the FingerSelectionScreen formated screen.
        /// NOTE: This sample application only uses the CaptureProgressScreen. Minor changes need to be done to use the FingerSelectionScreen formated screen.
        ///
        /// ***NOTE***: Each key is initialized to PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE but most other values could be used.
        ///             However, you must not use keys set to PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED when first switching to these screens.
        ///             ***BUT for the sake of speed, this application, will default to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED and the in 
        ///             the initialization function, set all the keys to OUT_DATA_DISPLAY_OBJECT_INACTIVE.***
        /// ***NOTE***: Errors in XML formated data for the TFT are often related to using OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED without 
        ///             first setting the element to a valid value.
        /// </summary>
        protected class TFT_ObjectDictionary : Dictionary<string, string>
        {
            public TFT_ObjectDictionary()
            {
                this.Add(PropertyConstants.OUT_DATA_TFT_CTRL_LEFT, PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE);
                this.Add(PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT, PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_ERASE);

                this.Add(PropertyConstants.OUT_DATA_TFT_L_PALM, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_L_THENAR, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_L_LOWERT, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_L_INTER, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_L_THUMB, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_L_INDEX, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_L_MIDDLE, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_L_RING, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_L_SMALL, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);

                this.Add(PropertyConstants.OUT_DATA_TFT_R_PALM, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_R_THENAR, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_R_LOWERT, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_R_INTER, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_R_THUMB, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_R_INDEX, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_R_MIDDLE, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_R_RING, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
                this.Add(PropertyConstants.OUT_DATA_TFT_R_SMALL, PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED);
            }
            ~TFT_ObjectDictionary()
            {
                this.Clear();
            }
        }

        /// <summary>
        /// Class TFT_ObjectCaptureDictionary is the L SCAN palm scanner TFT segment object definitions.
        /// Derived from the TFT_ObjectDictionary to add additional segments for the CaptureProgressScreen
        /// Each segment of the TFT display is controlled independently.
        /// This class defines each of the segments that are available in the CaptureProgressScreen formated screen.
        ///
        /// The TFT_ObjectCaptureDictionary class used for the CaptureProgressScreen formated screen.
        /// NOTE: This sample application only uses the CaptureProgressScreen. Minor changes need to be done to use the FingerSelectionScreen formated screen.
        ///
        /// ***NOTE***: Each key is initialized to PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE but most other values could be used.
        ///             However, you must not use keys set to PropertyConstants.OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED when first switching to these screens.
        ///             ***BUT for the sake of speed, this application, will default to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED and the in 
        ///             the initialization function, set all the keys to OUT_DATA_DISPLAY_OBJECT_INACTIVE.***
        /// </summary>
        protected class TFT_ObjectCaptureDictionary : TFT_ObjectDictionary
        {

            public TFT_ObjectCaptureDictionary()
            {
                this[PropertyConstants.OUT_DATA_TFT_CTRL_LEFT] = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_ERASE;

                this.Add(PropertyConstants.OUT_DATA_TFT_STAT_TOP, PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ERASE);
                this.Add(PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM, PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ERASE);

            }
            ~TFT_ObjectCaptureDictionary()
            {
                this.Clear();

            }
        }

        /*!
         * \fn private void _SetTftStatus()
         * \brief Display positioning status on TFT display based on BioBObjectQualityState object
         * global inputs: _biobaseDevice.mostRecentqualities[] used to update status LEDs on device
         * global inputs: _biobaseDevice.mostResentKey program with active keys
         * 
         * NOTE: This function assumes that the FingerSelectionScreen or CaptureProgressScreen formated 
         * screen has been initialized with the guidance elements via the _SetTftGuidance() method.
         * 
         * Catch BioBaseException and Exception and log errors
         */
        private void _SetTftStatus()
        {
            try
            {
                bool tooHigh = false;
                bool tooLeft = false;
                bool tooRight = false;
                bool flexToo = false;

                // Consolidate status of each individual finger to create one status
                for (int i = 0; i < _biobaseDevice.mostRecentqualities.Length; i++)
                {
                    switch (_biobaseDevice.mostRecentqualities[i])
                    {
                        case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_HIGH: tooHigh = true; break;
                        case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_LEFT: tooLeft = true; break;
                        case BioBObjectQualityState.BIOB_OBJECT_POSITION_TOO_RIGHT: tooRight = true; break;

                        case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_HIGH:
                        case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_LEFT:
                        case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_RIGHT:
                        case BioBObjectQualityState.BIOB_OBJECT_FLEX_POSITION_TOO_LOW: flexToo = true; break;
                    }
                }

                string status;
                if ((tooHigh && tooLeft && tooRight) || flexToo)
                    status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN_LEFT_RIGHT_UP;
                else if (tooHigh && tooLeft)
                    status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN_RIGHT;
                else if (tooHigh && tooRight)
                    status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN_LEFT;
                else if (tooRight && tooLeft)
                    status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_LEFT_RIGHT;
                else if (tooRight)
                    status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_LEFT;
                else if (tooLeft)
                    status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_RIGHT;
                else if (tooHigh)
                    status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_POSITION_DOWN;
                else
                    status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ERASE;


                string ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED;
                string ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED;
                switch (_biobaseDevice.mostResentKey)
                {
                    case (ActiveKeys.KEYS_NONE):
                        ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE;
                        ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE;
                        break;
                    case (ActiveKeys.KEYS_OK_CONTRAST):
                        // update buttons on first status change after start of catpure
                        ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_CONTRAST;
                        ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_OK;
                        break;
                    case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
                        // update buttons and status when DataAvailable has error
                        ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_REPEAT;
                        ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_OK;
                        status = (_biobaseDevice.mostRecentImpression == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL) ?
                                PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLL_ERROR : PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CAPTURE_ERROR;
                        break;
                }


                if ((status == m_CurrentTFTstatus) && (_biobaseDevice.mostResentKey == m_CurrentTFTKey))
                    return; // nothing to update

                m_CurrentTFTstatus = status;
                m_CurrentTFTKey = _biobaseDevice.mostResentKey;
                TFT_ObjectCaptureDictionary obj = new TFT_ObjectCaptureDictionary();

                obj[PropertyConstants.OUT_DATA_TFT_CTRL_LEFT] = ctrlLeft;
                obj[PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT] = ctrlRight;
                obj[PropertyConstants.OUT_DATA_TFT_STAT_TOP] = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_LEAVE_UNCHANGED;
                obj[PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM] = m_CurrentTFTstatus;
                // NOTE: TFT_ObjectCaptureDictionary constructor sets values to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED
                _TftShowFingerCaptureScreen(false, obj);


                //////////////////////////////////////////////////////
                //Outputdata to notify LSE to monitor inputs from keys
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement outputDataElem;
                XmlElement DataElem;
                switch (_biobaseDevice.mostResentKey)
                {
                    case (ActiveKeys.KEYS_NONE):
                        _SetupXMLOutputHeader(xmlDoc);

                        outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                        xmlDoc.DocumentElement.AppendChild(outputDataElem);

                        DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
                        outputDataElem.AppendChild(DataElem);
                        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_NONE);
                        _PerformUserOutput(xmlDoc.OuterXml);
                        break;
                    case (ActiveKeys.KEYS_OK_CONTRAST):
                    case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
                        _SetupXMLOutputHeader(xmlDoc);

                        outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                        xmlDoc.DocumentElement.AppendChild(outputDataElem);

                        DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
                        outputDataElem.AppendChild(DataElem);
                        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_OK);
                        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_CANCEL);
                        _PerformUserOutput(xmlDoc.OuterXml);
                        break;
                }
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT, ex.Message));
            }
        }

        /*!
         * \fn private void _SetTftStatus()
         * \brief Display positioning status on TFT display based on e.DataStatus
         * \param DataStatus final image status from _biobaseDevice_DataAvailable
         * global inputs: _biobaseDevice.mostResentKey program with active keys
         * 
         * NOTE: This function assumes that the FingerSelectionScreen or CaptureProgressScreen formated 
         * screen has been initialized with the guidance elements via the _SetTftGuidance() method.
         * 
         * Catch BioBaseException and Exception and log errors
         */
        private void _SetTftStatus(BioBReturnCode DataStatus)
        {
            try
            {
                string status;

                // check status 
                switch ((BioBReturnCode)DataStatus)
                {
                    case BioBReturnCode.BIOB_SUCCESS:
                        status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_OK;
                        break;

                    case BioBReturnCode.BIOB_OPTICS_SURFACE_DIRTY:
                        status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SURFACE_IS_DIRTY;
                        break;

                    case BioBReturnCode.BIOB_REPLACE_PAD:
                        status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SURFACE_IS_DIRTY_ALT_1;
                        break;

                    case BioBReturnCode.BIOB_BAD_SCAN:
                    case BioBReturnCode.BIOB_NO_CAPTURE_ACTIVE:
                        status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CAPTURE_ERROR;
                        break;

                    case BioBReturnCode.BIOB_AUTOCAPTURE_SEGMENTATION:
                        status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_QUALITY_CHECK_ERROR;
                        break;

                    case BioBReturnCode.BIOB_NO_OBJECT:
                    case BioBReturnCode.BIOB_SPOOF_DETECTED:
                    case BioBReturnCode.BIOB_SPOOF_DETECTOR_FAIL:
                        status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_SEQUENCE_CHECK_ERROR_ALT_1;
                        break;

                    case BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY:
                    case BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY:
                    case BioBReturnCode.BIOB_ROLL_LIFTED_TIP:
                    case BioBReturnCode.BIOB_ROLL_ON_BORDER:
                    case BioBReturnCode.BIOB_ROLL_PAUSED:
                    case BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW:
                        status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLL_ERROR;
                        break;

                    default:
                        status = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_COMMON_ERROR;
                        break;
                }


                string ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED;
                string ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED;
                switch (_biobaseDevice.mostResentKey)
                {
                    case (ActiveKeys.KEYS_NONE):
                        ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE;
                        ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_ERASE;
                        break;
                    case (ActiveKeys.KEYS_OK_CONTRAST):
                        // update buttons for OK to accept image captured with warning 
                        ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_CONTRAST;
                        ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_OK;
                        break;
                    case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
                        // update buttons to retry capture when DataAvailable has warngin 
                        ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_REPEAT;
                        ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_OK;
                        break;
                }

                if ((status == m_CurrentTFTstatus) && (_biobaseDevice.mostResentKey == m_CurrentTFTKey))
                    return; // nothing to update

                m_CurrentTFTKey = _biobaseDevice.mostResentKey;

                TFT_ObjectCaptureDictionary obj = new TFT_ObjectCaptureDictionary();
                obj[PropertyConstants.OUT_DATA_TFT_CTRL_LEFT] = ctrlLeft;
                obj[PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT] = ctrlRight;
                obj[PropertyConstants.OUT_DATA_TFT_STAT_TOP] = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_LEAVE_UNCHANGED;
                obj[PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM] = m_CurrentTFTstatus = status;
                // NOTE: TFT_ObjectCaptureDictionary constructor sets values to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED
                _TftShowFingerCaptureScreen(false, obj);

                //////////////////////////////////////////////////////
                //Outputdata to notify LSE to monitor inputs from keys
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement outputDataElem;
                XmlElement DataElem;
                switch (_biobaseDevice.mostResentKey)
                {
                    case (ActiveKeys.KEYS_NONE):
                        _SetupXMLOutputHeader(xmlDoc);

                        outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                        xmlDoc.DocumentElement.AppendChild(outputDataElem);

                        DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
                        outputDataElem.AppendChild(DataElem);
                        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_NONE);
                        _PerformUserOutput(xmlDoc.OuterXml);
                        break;
                    case (ActiveKeys.KEYS_OK_CONTRAST):
                    case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
                        _SetupXMLOutputHeader(xmlDoc);

                        outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                        xmlDoc.DocumentElement.AppendChild(outputDataElem);

                        DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
                        outputDataElem.AppendChild(DataElem);
                        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_OK);
                        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_CANCEL);
                        _PerformUserOutput(xmlDoc.OuterXml);
                        break;
                }

            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT, ex.Message));
            }
        }

        /*!
         * \fn private void _TftShowCompanyLogo()
         * \brief Display company logo to reset TFT screen to default.
         * 
         * Typical XML format to display logo on TFT display
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<Tft>
         *    -<LogoScreen>
         *     <Option>SHOW_FW_VERSION</Option>
         *     <ProgressBarPercent>0</ProgressBarPercent>
         *    </LogoScreen>
         *   </Tft>
         *  </OutputData>
         * </BioBase>
         * 
         * Catch BioBaseException and Exception and log errors
         */

        private void _TftShowCompanyLogo()
        {
            try
            {
                // create xml tree
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);

                XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT);
                outputDataElem.AppendChild(statusDataElem);

                XmlElement LogElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT_LOG_SCREEN);
                statusDataElem.AppendChild(LogElem);

                _AddUserOutputElement(xmlDoc, LogElem, PropertyConstants.OUT_DATA_TFT_LOG_SCR_OPTION, PropertyConstants.OUT_DATA_TFT_LOG_SCR_SHOW_FW_VERSION);
                _AddUserOutputElement(xmlDoc, LogElem, PropertyConstants.OUT_DATA_TFT_LOG_SCR_PROGRESS, "0");

                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_LOG_SCREEN, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_LOG_SCREEN, ex.Message));
            }
        }

        /*!
         * \fn private void _SetTftGuidance()
         * \brief Display guidance based on position and impression to the TFT screen.
         * global inputs: _biobaseDevice.mostRecentPosition and _biobaseDevice.mostRecentImpression
         * global inputs: _biobaseDevice.mostResentKey
         * 
         * NOTE: When application adds support for finger annotation, this methods logic must add support for annotated fingers.
         * 
         * NOTE: Because this is the function that will change to the FingerSelectionScreen or CaptureProgressScreen, all
         *       ALL TFT_ObjectCaptureDictionary key values will be changed to OUT_DATA_DISPLAY_OBJECT_INACTIVE.
         * NOTE: When changing to the FingerSelectionScreen or CaptureProgressScreen formated screen, none of the 
         *       and TFT_ObjectDictionary key value can be set to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED!
         *
         * Catch BioBaseException and Exception and log errors
         */
        private void _SetTftGuidance()
        {
            try
            {
                TFT_ObjectCaptureDictionary obj = new TFT_ObjectCaptureDictionary();

                // Initialize array to inactive. Must make sure no values are set to OUT_DATA_DISPLAY_OBJECT_LEAVE_UNCHANGED when switching screens.
                var keys = new List<string>(obj.Keys);
                foreach (string key in keys)
                {
                    obj[key] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_INACTIVE;
                }


                switch (_biobaseDevice.mostRecentPosition)
                {
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB): //PreTriggerMessage = "Impression?(Place):(roll) right thumb!";
                        obj[PropertyConstants.OUT_DATA_TFT_R_THUMB] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX):  //PreTriggerMessage = "Impression?(Place):(roll) right index finger!";
                        obj[PropertyConstants.OUT_DATA_TFT_R_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE):
                        obj[PropertyConstants.OUT_DATA_TFT_R_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING):
                        obj[PropertyConstants.OUT_DATA_TFT_R_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE):
                        obj[PropertyConstants.OUT_DATA_TFT_R_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB): //"Place left thumb!";
                        obj[PropertyConstants.OUT_DATA_TFT_R_THUMB] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX):
                        obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE):
                        obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING):
                        obj[PropertyConstants.OUT_DATA_TFT_L_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE):
                        obj[PropertyConstants.OUT_DATA_TFT_L_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS):
                        // PreTriggerMessage = "Place right 4 flat fingers!";
                        obj[PropertyConstants.OUT_DATA_TFT_R_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;

                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS):
                        //PreTriggerMessage = "Place left 4 flat fingers!";
                        obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS):
                        //PreTriggerMessage = "Place 2 thumbs!";
                        obj[PropertyConstants.OUT_DATA_TFT_R_THUMB] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_THUMB] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES):
                        //PreTriggerMessage = "Place Flat 4: Left Middle + Left Index + Right Index + Right Middle!";
                        // case is not supported directly by LScan palm device but software can be setup as an option.
                        obj[PropertyConstants.OUT_DATA_TFT_R_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS):
                        // Generic two fingers so in this application it will assume right and left index on TFT display
                        obj[PropertyConstants.OUT_DATA_TFT_R_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE):
                        // case is not supported directly by LScan palm device but software can be setup as an option.
                        obj[PropertyConstants.OUT_DATA_TFT_R_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE):
                        // case is not supported directly by LScan palm device but software can be setup as an option.
                        obj[PropertyConstants.OUT_DATA_TFT_R_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE):
                        // case is not supported directly by LScan palm device but software can be setup as an option.
                        obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE):
                        // case is not supported directly by LScan palm device but software can be setup as an option.
                        obj[PropertyConstants.OUT_DATA_TFT_L_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;

                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FULL_PALM):
                        //PreTriggerMessage = "Place right upper palm!";
                        // case is not supported directly by LScan palm device but software can be setup as an option.
                        obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_PALM] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_INTER] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        SetUILedColors(5); // Display five UI status LEDs for fingers and palm.
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_WRITERS_PALM):
                        //PreTriggerMessage = "Place right writers palm!";
                        obj[PropertyConstants.OUT_DATA_TFT_R_THENAR] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LOWER_PALM):
                        //PreTriggerMessage = "Place right lower palm!";
                        // One part larger lower palm
                        obj[PropertyConstants.OUT_DATA_TFT_R_PALM] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_LOWERT] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_MISSING;

                        /*OR two part lower palm...
                        obj[PropertyConstants.OUT_DATA_TFT_R_PALM]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_LOWERT] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        */
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_UPPER_PALM):
                        //PreTriggerMessage = "Place right upper palm!";
                        obj[PropertyConstants.OUT_DATA_TFT_R_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_R_INTER] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        SetUILedColors(5); // Display five UI status LEDs for fingers and palm.
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FULL_PALM):
                        //PreTriggerMessage = "Place left upper palm!";
                        // case is not supported directly by LScan palm device but software can be setup as an option.
                        obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_PALM] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_INTER] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        SetUILedColors(5); // Display five UI status LEDs for fingers and palm.
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_WRITERS_PALM):
                        //PreTriggerMessage = "Place left writers palm!";
                        obj[PropertyConstants.OUT_DATA_TFT_L_THENAR] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LOWER_PALM):
                        //PreTriggerMessage = "Place left lower palm!";
                        // One part larger lower palm
                        obj[PropertyConstants.OUT_DATA_TFT_L_PALM] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_LOWERT] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_MISSING;

                        /*OR two part lower palm...
                        obj[PropertyConstants.OUT_DATA_TFT_L_PALM]   = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_LOWERT] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        */
                        break;
                    case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_UPPER_PALM):
                        //PreTriggerMessage = "Place left upper palm!";
                        obj[PropertyConstants.OUT_DATA_TFT_L_INDEX] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_MIDDLE] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_RING] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_SMALL] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        obj[PropertyConstants.OUT_DATA_TFT_L_INTER] = PropertyConstants.OUT_DATA_DISPLAY_OBJECT_AUTOCAPTURE_OK;
                        SetUILedColors(5); // Display five UI status LEDs for fingers and palm.
                        break;
                }

                string statusTop = "";
                switch (_biobaseDevice.mostRecentImpression)
                {
                    case (PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL):
                        switch (_biobaseDevice.mostRecentPosition)
                        {
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB):
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX):
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE):
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING):
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE):
                                statusTop = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_HORIZONTAL_RIGHT;
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB):
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX):
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE):
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING):
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE):
                                statusTop = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_HORIZONTAL_LEFT;
                                break;
                            default:
                                statusTop = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_HORIZONTAL;
                                break;
                        }
                        break;
                    case (PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL_VERTICAL):
                        statusTop = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ROLL_VERTICAL;
                        break;
                    default:
                        statusTop = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_CAPTURE_FLAT;
                        break;
                }


                string statusBottom = PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ERASE;

                string ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_LEAVE_UNCHANGED;
                string ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_SELECTION_CTRL_LEAVE_UNCHANGED;
                switch (_biobaseDevice.mostResentKey)
                {
                    case (ActiveKeys.KEYS_NONE):
                        ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_ERASE;
                        ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_STAT_TOP_ERASE;
                        break;
                    case (ActiveKeys.KEYS_OK_CONTRAST):
                        // update buttons on first status change after start of catpure
                        ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_CONTRAST;
                        ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_GREEN_OK;
                        break;
                    case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
                        // update buttons and status when DataAvailable has error
                        ctrlLeft = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_REPEAT;
                        ctrlRight = PropertyConstants.OUT_DATA_DISPLAY_COMMON_CTRL_YELLOW_OK;
                        statusBottom = (_biobaseDevice.mostRecentImpression == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL) ?
                                PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_ROLL_ERROR : PropertyConstants.OUT_DATA_DISPLAY_STAT_BOTTOM_CAPTURE_ERROR;
                        break;
                }


                obj[PropertyConstants.OUT_DATA_TFT_CTRL_LEFT] = ctrlLeft;
                obj[PropertyConstants.OUT_DATA_TFT_CTRL_RIGHT] = ctrlRight;
                obj[PropertyConstants.OUT_DATA_TFT_STAT_TOP] = statusTop;
                obj[PropertyConstants.OUT_DATA_TFT_STAT_BOTTOM] = statusBottom;
                _TftShowFingerCaptureScreen(false, obj);


                //////////////////////////////////////////////////////
                /*Outputdata to notify LSE to monitor inputs from keys
                 * 
                 * Typical XML output for Active keys:
                 * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
                 * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
                 *  -<OutputData>
                 *   -<ActiveDeviceButtons>
                 *    <Key>OK</Key>
                 *    <Key>CANCEL</Key>
                 *   </ActiveDeviceButtons>
                 *  </OutputData>
                 * </BioBase>
                 */
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement outputDataElem;
                XmlElement DataElem;
                switch (_biobaseDevice.mostResentKey)
                {
                    case (ActiveKeys.KEYS_NONE):
                        _SetupXMLOutputHeader(xmlDoc);

                        outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                        xmlDoc.DocumentElement.AppendChild(outputDataElem);

                        DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
                        outputDataElem.AppendChild(DataElem);
                        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_NONE);
                        _PerformUserOutput(xmlDoc.OuterXml);
                        break;
                    case (ActiveKeys.KEYS_OK_CONTRAST):
                    case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
                        _SetupXMLOutputHeader(xmlDoc);

                        outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                        xmlDoc.DocumentElement.AppendChild(outputDataElem);

                        DataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_ACTIVEBUTTONS);
                        outputDataElem.AppendChild(DataElem);
                        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_OK);
                        _AddUserOutputElement(xmlDoc, DataElem, PropertyConstants.OUT_DATA_KEY, PropertyConstants.OUT_DATA_KEY_CANCEL);
                        _PerformUserOutput(xmlDoc.OuterXml);
                        break;
                }
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_CAP_SCREEN, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TFT_CAP_SCREEN, ex.Message));
            }
        }


        /*!
         * \fn private void _TftShowCaptureProgress()
         * \brief Format XML and send output for status and guidance to the TFT OUT_DATA_TFT_FIN_SCREEN and OUT_DATA_TFT_CAP_SCREEN
         * input: finger true for OUT_DATA_TFT_FIN_SCREEN and false for OUT_DATA_TFT_CAP_SCREEN
         * input: TFT_ObjectDictionary defines how each segment on the TFT is displayed.
         * 
         * No try/catch as the BioBaseException and Exception are passed up to the calling function
         * 
         * Typical XML format for TFT display for the CaptureProgressScreen formated screen:
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<Tft>
         *    -<CaptureProgressScreen>
         *      <LeftButton>YELLOW_CONTRAST</LeftButton>
         *      <RightButton>GREEN_OK</RightButton>
         *      <StatTop>CAPTURE_FLAT</StatTop>
         *      <StatBottom>ERASE</StatBottom>
         *      <ColorLeftPalm>INACTIVE</ColorLeftPalm>
         *      <ColorLeftThenar>INACTIVE</ColorLeftThenar>
         *      <ColorLeftLowerThenar>INACTIVE</ColorLeftLowerThenar>
         *      <ColorLeftInterDigital>INACTIVE</ColorLeftInterDigital>
         *      <ColorLeftThumb>INACTIVE</ColorLeftThumb>
         *      <ColorLeftIndex>INACTIVE</ColorLeftIndex>
         *      <ColorLeftMiddle>INACTIVE</ColorLeftMiddle>
         *      <ColorLeftRing>INACTIVE</ColorLeftRing>
         *      <ColorLeftSmall>INACTIVE</ColorLeftSmall>
         *      <ColorRightPalm>INACTIVE</ColorRightPalm>
         *      <ColorRightThenar>INACTIVE</ColorRightThenar>
         *      <ColorRightLowerThenar>INACTIVE</ColorRightLowerThenar>
         *      <ColorRightInterDigital>INACTIVE</ColorRightInterDigital>
         *      <ColorRightThumb>INACTIVE</ColorRightThumb>
         *      <ColorRightIndex>CURRENT_SELECTION</ColorRightIndex>
         *      <ColorRightMiddle>CURRENT_SELECTION</ColorRightMiddle>
         *      <ColorRightRing>CURRENT_SELECTION</ColorRightRing>
         *      <ColorRightSmall>CURRENT_SELECTION</ColorRightSmall>
         *     </CaptureProgressScreen>
         *    </Tft>
         *   </OutputData>
         *  </BioBase>
         */
        private void _TftShowFingerCaptureScreen(bool finger, TFT_ObjectDictionary obj)
        {
            // create xml tree
            XmlDocument xmlDoc = new XmlDocument();
            _SetupXMLOutputHeader(xmlDoc);

            XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
            xmlDoc.DocumentElement.AppendChild(outputDataElem);

            XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT);
            outputDataElem.AppendChild(statusDataElem);

            XmlElement Elem;
            if (finger)
                Elem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT_FIN_SCREEN);
            else
                Elem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TFT_CAP_SCREEN);

            statusDataElem.AppendChild(Elem);

            foreach (KeyValuePair<string, string> property in obj)
            {
                _AddUserOutputElement(xmlDoc, Elem, property.Key, property.Value);
            }
            _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
        }

        #endregion


        #region TouchDisplay
        // TouchDisplay - Control Touch screen and LED display on Guardian, Guardian 300, Guardain 200 and Guardian 100


        /*!
         * \fn private void _ResetTouchDisplay()
         * \brief Display the comany log on the Touch Screen or turn off all the LEDs on the LED display
         * 
         * Typical XML format to display default touch screen:
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<TouchDisplay>
         *    -<DesignTemplate>
         *     <URI>file:///E:/Templates/index_standby.html</URI>
         *    </DesignTemplate>
         *   </TouchDisplay>
         *  </OutputData>
         * </BioBase>
         * Catch BioBaseException and Exception and log errors
         */
        private void _ResetTouchDisplay()
        {
            try
            {
                // Dispaly index_standby.html on Guardian 300
                string template = m_TouchDisplayTemplatePath + "index_standby.html";

                // create xml tree
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);

                XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY);
                outputDataElem.AppendChild(statusDataElem);

                XmlElement LogElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_DESIGNTEMPLATE);
                statusDataElem.AppendChild(LogElem);

                _AddUserOutputElement(xmlDoc, LogElem, PropertyConstants.OUT_DATA_TOUCHDISPLAY_URI, template);

                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
            }
        }

        /*!
         * \fn private void _SetTouchStatus()
         * \brief The TouchDisplayStandardTemplate template and ExternalParameters are already set in _SetTouchGuidance!
         * This method doeesn't need _biobaseDevice.mostRecentqualities because this is automatically process by the template
         * global inputs: _biobaseDevice.mostResentKey 
         * 
         * Typical XML output for status update of flat capture of right thumb:
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         * -<OutputData>
         * -<TouchDisplay>
         * -<DesignTemplate>
         * <URI>file:///E:/temp/Templates/index_standard_thumbs.html</URI>
         * <ExternalParameter Value="2" Key="ButtonRetry"/>
         * <ExternalParameter Value="1" Key="ButtonConfirm"/>
         * <ExternalParameter Value="1" Key="FP1"/>
         * <ExternalParameter Value="0" Key="FP6"/>
         * </DesignTemplate>
         * </TouchDisplay>
         * </OutputData>
         * </BioBase>
         * 
         * Catch BioBaseException and Exception and log errors
         */
        private void _SetTouchStatus()
        {
            try
            {
                // Update TouchDisplayStandardExternalParameters with new _biobaseDevice.mostResentKey
                string Retry = "2"; //hidden
                string Confirm = "2"; //hidden
                switch (_biobaseDevice.mostResentKey)
                {
                    case (ActiveKeys.KEYS_NONE):
                        Retry = "2"; //hidden
                        Confirm = "2"; //hidden
                        break;
                    case (ActiveKeys.KEYS_OK_CONTRAST):
                        Retry = "2"; //hidden
                        Confirm = "1"; //active  - enable AdjustAcquisitionProcess
                        break;
                    case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
                        Retry = "1";  // button active  - enable RescanImage
                        Confirm = "1";  // button active - enable accept image captured with warning
                        break;
                }
                _biobaseDevice.TouchDisplayStandardExternalParameters["ButtonRetry"] = Retry;  // update button 
                _biobaseDevice.TouchDisplayStandardExternalParameters["ButtonConfirm"] = Confirm;  // update button 

                // create xml tree
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);

                XmlElement TDElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY);
                outputDataElem.AppendChild(TDElem);

                XmlElement TemplateElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_DESIGNTEMPLATE);
                TDElem.AppendChild(TemplateElem);

                // Set template to initial animated html template.
                _AddUserOutputElement(xmlDoc, TemplateElem, PropertyConstants.OUT_DATA_TOUCHDISPLAY_URI, _biobaseDevice.TouchDisplayStandardTemplate);


                foreach (KeyValuePair<string, string> Ex in _biobaseDevice.TouchDisplayStandardExternalParameters)
                {
                    XmlElement EpElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER);
                    EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_KEY, Ex.Key);
                    EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_VALUE, Ex.Value);
                    TemplateElem.AppendChild(EpElem);
                }

                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
            }
        }

        /*!
         * \fn private void _SetTouchStatus()
         * \brief The TouchDisplayStandardTemplate template and ExternalParameters are already set in _SetTouchGuidance!
         * \param DataStatus final image status from _biobaseDevice_DataAvailable
         * global inputs: _biobaseDevice.mostResentKey 
         * 
         * Typical XML output for successful flat capture of right thumb:
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<TouchDisplay>
         *    -<DesignTemplate>
         *     <URI>file:///E:/temp/Templates/index_standard_thumbs.html</URI>
         *     <ExternalParameter Value="2" Key="ButtonRetry"/>
         *     <ExternalParameter Value="2" Key="ButtonConfirm"/>
         *     <ExternalParameter Value="1" Key="FP1"/>
         *     <ExternalParameter Value="0" Key="FP6"/>
         *     <ExternalParameter Value="1" Key="Result"/>
         *    </DesignTemplate>
         *   </TouchDisplay>
         *  </OutputData>
         * </BioBase>
         * 
         * Catch BioBaseException and Exception and log errors
         */
        private void _SetTouchStatus(BioBReturnCode DataStatus)
        {
            try
            {
                // Update TouchDisplayStandardExternalParameters with new _biobaseDevice.mostResentKey
                string Retry = "2"; //hidden
                string Confirm = "2"; //hidden
                switch (_biobaseDevice.mostResentKey)
                {
                    case (ActiveKeys.KEYS_NONE):
                        Retry = "2"; //hidden
                        Confirm = "2"; //hidden
                        break;
                    case (ActiveKeys.KEYS_OK_CONTRAST):
                        Retry = "2"; //hidden
                        Confirm = "1"; //active  - enable AdjustAcquisitionProcess
                        break;
                    case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
                        Retry = "1";  // button active  - enable RescanImage
                        Confirm = "1";  // button active - enable accept image captured with warning
                        break;
                }
                _biobaseDevice.TouchDisplayStandardExternalParameters["ButtonRetry"] = Retry;  // update button 
                _biobaseDevice.TouchDisplayStandardExternalParameters["ButtonConfirm"] = Confirm;  // update button 


                string status = "1";

                // check status 
                switch ((BioBReturnCode)DataStatus)
                {
                    case BioBReturnCode.BIOB_SUCCESS:
                        status = "1";
                        break;

                    case BioBReturnCode.BIOB_OPTICS_SURFACE_DIRTY:
                        status = "3";
                        break;

                    case BioBReturnCode.BIOB_REPLACE_PAD:
                        status = "9";
                        break;

                    case BioBReturnCode.BIOB_BAD_SCAN:
                    case BioBReturnCode.BIOB_NO_CAPTURE_ACTIVE:
                        status = "4";
                        break;

                    case BioBReturnCode.BIOB_AUTOCAPTURE_SEGMENTATION:
                        status = "6";
                        break;

                    case BioBReturnCode.BIOB_NO_OBJECT:
                    case BioBReturnCode.BIOB_SPOOF_DETECTED:
                    case BioBReturnCode.BIOB_SPOOF_DETECTOR_FAIL:
                        status = "2";
                        break;

                    case BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY:
                    case BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY:
                    case BioBReturnCode.BIOB_ROLL_LIFTED_TIP:
                    case BioBReturnCode.BIOB_ROLL_ON_BORDER:
                    case BioBReturnCode.BIOB_ROLL_PAUSED:
                    case BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW:
                        status = "5";
                        break;

                    default:
                        if ((DataStatus & BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY) == BioBReturnCode.BIOB_ROLL_SHIFTED_HORIZONTALLY ||
                        (DataStatus & BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY) == BioBReturnCode.BIOB_ROLL_SHIFTED_VERTICALLY ||
                        (DataStatus & BioBReturnCode.BIOB_ROLL_LIFTED_TIP) == BioBReturnCode.BIOB_ROLL_LIFTED_TIP ||
                        (DataStatus & BioBReturnCode.BIOB_ROLL_ON_BORDER) == BioBReturnCode.BIOB_ROLL_ON_BORDER ||
                        (DataStatus & BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW) == BioBReturnCode.BIOB_ROLL_FINGER_TOO_NARROW ||
                        (DataStatus & BioBReturnCode.BIOB_ROLL_PAUSED) == BioBReturnCode.BIOB_ROLL_PAUSED)
                        {
                            status = "5";
                        }
                        else
                        {
                            status = "2";
                        }
                        break;
                }
                _biobaseDevice.TouchDisplayStandardExternalParameters["Result"] = status;   // Update status


                // create xml tree
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);

                XmlElement TDElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY);
                outputDataElem.AppendChild(TDElem);

                XmlElement TemplateElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_DESIGNTEMPLATE);
                TDElem.AppendChild(TemplateElem);

                // Set template to initial animated html template.
                _AddUserOutputElement(xmlDoc, TemplateElem, PropertyConstants.OUT_DATA_TOUCHDISPLAY_URI, _biobaseDevice.TouchDisplayStandardTemplate);


                foreach (KeyValuePair<string, string> Ex in _biobaseDevice.TouchDisplayStandardExternalParameters)
                {
                    XmlElement EpElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER);
                    EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_KEY, Ex.Key);
                    EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_VALUE, Ex.Value);
                    TemplateElem.AppendChild(EpElem);
                }

                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
            }
        }

        /*!
         * \fn private void _SetTouchGuidance()
         * \brief use position, impression and keys to format output for device's touch and LED display
         * global output:TouchDisplayInitialTemplate HTML template for initial prompt
         * global output:TouchDisplayInitialExternalParameters HTML template parameters for initial prompt
         * global output:TouchDisplayStandardTemplate HTML template for prompt after figners detected on the platen
         * global output:TouchDisplayStandardExternalParameters HTML template parameters for prompt after figners detected on the platen
         * global inputs: _biobaseDevice.mostRecentPosition and _biobaseDevice.mostRecentImpression
         * global inputs: _biobaseDevice.mostResentKey 
         * 
         * Note: "ButtonRetry", "ButtonConfirm", "Result" and "FPx" are defined in the Templates files
         * NOTE: When application adds support for finger annotation, this methods logic must add support for annotated fingers.
         * 
         * Typical XML output for flat capture of right thumb:
         * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
         * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
         *  -<OutputData>
         *   -<TouchDisplay>
         *    -<DesignTemplate>
         *     <URI>file:///E:/temp/Templates/index_initial_thumbs.html</URI>
         *     <ExternalParameter Value="2" Key="ButtonRetry"/>
         *     <ExternalParameter Value="1" Key="ButtonConfirm"/>
         *     <ExternalParameter Value="1" Key="FP1"/>
         *     <ExternalParameter Value="0" Key="FP6"/>
         *    </DesignTemplate>
         *   </TouchDisplay>
         *  </OutputData>
         * </BioBase>
         * 
         * Catch BioBaseException and Exception and log errors
         */
        private void _SetTouchGuidance()
        {
            try
            {
                _biobaseDevice.TouchDisplayStandardTemplate = _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_standby.html";
                _biobaseDevice.TouchDisplayInitialExternalParameters.Clear();
                _biobaseDevice.TouchDisplayStandardExternalParameters.Clear();

                string Retry = "2"; //hidden
                string Confirm = "2"; //hidden
                switch (_biobaseDevice.mostResentKey)
                {
                    case (ActiveKeys.KEYS_NONE):
                        Retry = "2"; //hidden
                        Confirm = "2"; //hidden
                        break;
                    case (ActiveKeys.KEYS_OK_CONTRAST):
                        Retry = "2"; //hidden
                        Confirm = "1"; //active  - enable AdjustAcquisitionProcess
                        break;
                    case (ActiveKeys.KEYS_ACCEPT_RECAPTURE):
                        Retry = "1";  // button active  - enable RescanImage
                        Confirm = "1";  // button active - enable accept image captured with warning
                        break;
                }
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("ButtonRetry", Retry);  // button 
                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("ButtonConfirm", Confirm);  // button 

                switch (_biobaseDevice.mostRecentImpression)
                {
                    case (PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_FLAT):
                        switch (_biobaseDevice.mostRecentPosition)
                        {
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_thumbs.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "1");  // show right thumb
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "0");  // hide left thumb

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_thumbs.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "1");  // show right index finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "0");  // hide right middle finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "0");  // hide right ring finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "0");  // hide right little finger

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "0");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "0");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "0");

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "0");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "0");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "0");

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "0");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "0");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "0");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "1");

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "1");  //show index
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "1");  //show middle
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "1");  //show ring
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "1");  //show little

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "1");  //show index
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "1");  //show middle
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "0");  //hide ring
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "0");  //hide little

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_right.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "0");  //hide index
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "0");  //hide middle
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "1");  //show ring
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "1");  //show little

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_right.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS):
                                // Generic two fingers so in this application it will assume right and left index 
                                //TODO: Fix  html for two index fingers. index_initial_four.html currently always displays 4 fingers.
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_four.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "1");  //show right index
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "0");  //hide right middle
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "1");  //show left index
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "0");  //hide left middle

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_four.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;


                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_thumbs.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "0");  // hide right thumb
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "1");  // show left thumb

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_thumbs.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "1");  // show left index finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "0");  // hide left middle finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "0");  // hide left ring finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "0"); // hide left little finger

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "0");  // hide left index finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "1");  // show left middle finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "0");  // hide left ring finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "0"); // hide left little finger

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "0");  // hide left index finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "0");  // hide left middle finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "1");  // show left ring finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "0"); // hide left little finger

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "0");  // hide left index finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "0");  // hide left middle finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "0");  // hide left ring finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "1"); // show left little finger

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "1");  // show left index finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "1");  // show left middle finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "1");  // show left ring finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "1"); // show left little finger

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "1");  // show left index finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "1");  // show left middle finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "0");  // hide left ring finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "0"); // hide left little finger

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_left.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "0");  // hide left index finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "0");  // hide left middle finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "1");  // show left ring finger
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "1"); // show left little finger

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_left.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS):
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_thumbs.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "1");  // show right thumb
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "1");  // show left thumb

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_thumbs.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES):
                                //PreTriggerMessage = "Place Flat 4: Left Middle + Left Index + Right Index + Right Middle!";
                                _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_initial_four.html";
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "1");  // show right thumb
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "1");  // show left thumb
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "1");  // show right thumb
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "1");  // show left thumb

                                _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_standard_four.html";
                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                break;
                        }
                        break;


                    case (PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL):
                        _biobaseDevice.TouchDisplayInitialExternalParameters.Clear();
                        _biobaseDevice.TouchDisplayInitialTemplate = m_TouchDisplayTemplatePath + "index_roll.html";
                        _biobaseDevice.TouchDisplayStandardTemplate = m_TouchDisplayTemplatePath + "index_roll.html";
                        switch (_biobaseDevice.mostRecentPosition)
                        {
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB):
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP1", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "2");   // Prompt to Roll right thumb.... Green

                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                _biobaseDevice.TouchDisplayStandardExternalParameters["FP1"] = "2";     // Capturing Roll right thumb.... Yellow
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX):
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP1", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "2");   // Prompt to Roll right index....

                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                _biobaseDevice.TouchDisplayStandardExternalParameters["FP2"] = "2";     // Capturing Roll index.... Yellow
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE):
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP1", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "2");

                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                _biobaseDevice.TouchDisplayStandardExternalParameters["FP3"] = "2";
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING):
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP1", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "2");

                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                _biobaseDevice.TouchDisplayStandardExternalParameters["FP4"] = "2";     // Capturing Roll - Yellow
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE):
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP1", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "2");

                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                _biobaseDevice.TouchDisplayStandardExternalParameters["FP5"] = "2";     // Capturing Roll - Yellow
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB):
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP2", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "2");

                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                _biobaseDevice.TouchDisplayStandardExternalParameters["FP6"] = "2";     // Capturing Roll - Yellow
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX):
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP2", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "2");

                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                _biobaseDevice.TouchDisplayStandardExternalParameters["FP7"] = "2";     // Capturing Roll - Yellow
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE):
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP2", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "2");

                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                _biobaseDevice.TouchDisplayStandardExternalParameters["FP8"] = "2";     // Capturing Roll - Yellow
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING):
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP2", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "2");

                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                _biobaseDevice.TouchDisplayStandardExternalParameters["FP9"] = "2";     // Capturing Roll - Yellow
                                break;
                            case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE):
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("HP2", "1");
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP10", "2");

                                //Optional example of how to update of the Touch Display to show other finger rolls are complete...
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP1", "3");  // mark red - error on right thumb
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP2", "4");  // mark light blue - successful capture on right index
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP3", "4");  // mark light blue - successful capture
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP4", "4");  // mark light blue - successful capture
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP5", "4");  // mark light blue - successful capture
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP6", "4");  // mark light blue - successful capture
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP7", "4");  // mark light blue - successful capture
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP8", "4");  // mark light blue - successful capture
                                _biobaseDevice.TouchDisplayInitialExternalParameters.Add("FP9", "0");  // mark light blue - successful capture

                                _biobaseDevice.TouchDisplayStandardExternalParameters = new Dictionary<string, string>(_biobaseDevice.TouchDisplayInitialExternalParameters); // templates use the same parameters
                                _biobaseDevice.TouchDisplayStandardExternalParameters["FP10"] = "2";     // Capturing Roll - Yellow
                                break;
                        }
                        break;
                    case (PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL_VERTICAL):
                        //TODO: Create and setup animated html templates for vertical roll
                        break;
                }


                // create xml tree
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);

                XmlElement TDElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY);
                outputDataElem.AppendChild(TDElem);

                XmlElement TemplateElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_DESIGNTEMPLATE);
                TDElem.AppendChild(TemplateElem);

                // Set template to initial animated html template.
                _AddUserOutputElement(xmlDoc, TemplateElem, PropertyConstants.OUT_DATA_TOUCHDISPLAY_URI, _biobaseDevice.TouchDisplayInitialTemplate);

                foreach (KeyValuePair<string, string> Ex in _biobaseDevice.TouchDisplayInitialExternalParameters)
                {
                    XmlElement EpElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER);
                    EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_KEY, Ex.Key);
                    EpElem.SetAttribute(PropertyConstants.OUT_DATA_TOUCHDISPLAY_EXTERNAL_PARAMETER_VALUE, Ex.Value);
                    TemplateElem.AppendChild(EpElem);
                }

                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
            }
        }

        /*!
         * \fn private void _StopTouchUpdates()
         * \brief stops alll touch screen updated by sending null url
         * Catch BioBaseException and Exception and log errors
         */
        private void _StopTouchUpdates()
        {
            try
            {
                // Stop display on Guardian 300 - UserOutput with no URL....
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);
                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);
                XmlElement statusDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_TOUCHDISPLAY);
                outputDataElem.AppendChild(statusDataElem);
                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_TOUCHDISPLAY, ex.Message));
            }
        }


        #endregion

        /*!
        * \fn private void _SetImageText()
        * \brief place holder for applications that would use the Visualization Window() that has LSE draw to the image box.
        *  Becasue this application uses the preview event to draw to the image box, this is not implemented.
        *  this would send text to the Visualization Window() that is draw to the image box by LSE
        *  
        * //Typical visualization text overlay format:
        * <?xml version="1.0" encoding="UTF-8" standalone="true"?>
        * -<BioBase xsi:noNamespaceSchemaLocation="BioBase.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="4.0">
        *  -<OutputData>
        *   -<VisualizationOverlay>
        *     <Text Value="Place fingers on platen." BelongsToImage="FALSE" FontSize="10" FontName="Arial" Color="0 0 255" PosX="10" PosY="10
        *    </VisualizationOverlay>
        *   </OutputData>
        *  </BioBase>
        *
        * Catch BioBaseException and Exception and log errors
        */
        private void _SetImageText(string msg)
        {
            try
            {
                // Display text on visualization window
                XmlDocument xmlDoc = new XmlDocument();
                _SetupXMLOutputHeader(xmlDoc);

                XmlElement outputDataElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OUTPUTDATA);
                xmlDoc.DocumentElement.AppendChild(outputDataElem);

                XmlElement OverlayElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OVERLAY);
                outputDataElem.AppendChild(OverlayElem);

                XmlElement TextElem = xmlDoc.CreateElement(PropertyConstants.OUT_DATA_OVERLAY_TEXT);
                TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_POSY, "10");
                TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_POSX, "10");
                TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_COLOR, "0 0 255");
                TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_FONT_NAME, "Arial");
                TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_FONT_SIZE, "10");
                TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_BELONGS_TO_IMAGE, PropertyConstants.DEV_PROP_FALSE);
                TextElem.SetAttribute(PropertyConstants.OUT_DATA_OVERLAY_TEXT_VALUE, msg);
                OverlayElem.AppendChild(TextElem);

                _PerformUserOutput(xmlDoc.OuterXml);    //Send XML to device
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioBase {0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_OVERLAY_TEXT, ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("{0} with {1} BioBase error {2}", PropertyConstants.OUT_DATA_OUTPUTDATA, PropertyConstants.OUT_DATA_OVERLAY_TEXT, ex.Message));
            }

        }

        /*!
         * \fn private void _SetupXMLOutputHeader()
         * \brief Format generic XML header for all SetOutputData 
         * No try/catch as the BioBaseException and Exception are passed to calling functions
         */
        private void _SetupXMLOutputHeader(XmlDocument xmlDoc)
        {
            XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDoc.AppendChild(declaration);

            XmlElement rootBiobaseElem = xmlDoc.CreateElement(PropertyConstants.XML_ROOT);
            rootBiobaseElem.SetAttribute(PropertyConstants.XML_ROOT_INTERFACE_VERSION, PropertyConstants.XML_ROOT_INTERFACE_VERSION_NUMBER);
            rootBiobaseElem.SetAttribute(PropertyConstants.XMLNS, PropertyConstants.XMLNS_URL);

            XmlAttribute attributeNode = xmlDoc.CreateAttribute("xsi", "noNamespaceSchemaLocation", PropertyConstants.XMLNS_URL);
            attributeNode.Value = PropertyConstants.XML_SCHEMA_NAME;
            rootBiobaseElem.SetAttributeNode(attributeNode);
            xmlDoc.AppendChild(rootBiobaseElem);    // add root
        }

        /*!
         * \fn private void _AddUserOutputElement()
         * \brief Add XML element for SetOutputData
         * No try/catch as the BioBaseException and Exception are passed to calling functions
         */
        private void _AddUserOutputElement(
                                 XmlDocument xmlDoc,  ///< [in]  top level XML document for elements
                             XmlElement Parent,   ///< [in]  XML element to add element to
                             string element,      ///< [in]  new XML element
                             string text)         ///< [in]  text for XML element
        {
            XmlNode StatusElement = xmlDoc.CreateElement(element);
            StatusElement.InnerText = text;
            Parent.AppendChild(StatusElement);
        }

        /*!
         * \fn private void _PerformUserOutput()
         * \brief Format generic XML header for all SetOutputData 
         * No try/catch as the BioBaseException and Exception are passed to calling functions
         */
        private void _PerformUserOutput(string xml)
        {
            // Get IntPtr to unmanaged copy of xml string
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] outData = encoding.GetBytes(xml);
            int size = Marshal.SizeOf(outData[0]) * outData.Length;
            IntPtr imagePtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(outData, 0, imagePtr, outData.Length);

            // Format unmanaged structure for SetOutputData
            BioBSetOutputData outputData;
            outputData.Buffer = imagePtr;
            outputData.BufferSize = size;
            outputData.FormatType = BioBOutputDataFormat.BIOB_OUT_XML;
            outputData.pExtStruct = IntPtr.Zero;
            outputData.pStructName = null;
            outputData.TransactionID = 0;

            // Output sent to the open device referenced in the _biobaseDevice object
            _biobaseDevice.SetOutputData(outputData);
            Marshal.FreeHGlobal(imagePtr);
        }

        void RescanImage()
        {
            ImageBox.Image = null;
            ImageBox.Update();
            if (this.InvokeRequired)
                this.Invoke(new Action(() => RescanImage()));
            else
                StartAcquire();
        }

        private void StartAcquire()
        {

            if (_biobaseDevice == null)
            {
                MessageBox.Show("Open device first");
                return;
            }
            try
            {
                if (_biobaseDevice.IsDeviceReady() == false)
                {
                    MessageBox.Show("Device is not Opened.");
                    return;
                }
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("Start Acquire BioBase error {0}", ex.Message));
                return;
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("Start Acquire error {0}", ex.Message));
                return;
            }

            try
            {
                // Must have try/catch to pick up warning returned by low level BioB_BeginAcquisitionProcess
                _biobaseDevice.BeginAcquisitionProcess();
            }
            catch (BioBaseException ex)
            {
                if (ex.ReturnCode == BioBReturnCode.BIOB_REPLACE_PAD)
                {
                    // Optional check of Replace silicone membrane here. Code could be modified to always ignore warning.
                    // with BIOB_REPLACE_PAD return code, there are two options. We won't prompt to replace silicone membrane here.
                    // Device should be closed and re-opened when silicone membrane is replaced.
                    // 1. Ignore the warning to replace silicone membrane; continue with Acquire (break)
                    // 2. Cancel Acquire without changing device state (return)
                    string emsg = string.Format("BioB_BeginAcquisitionProcess warning {0}.   Ignore replace silicone membrane warning?", ex.Message);
                    DialogResult result = MessageBox.Show(emsg, "BioB_OpenDevice", MessageBoxButtons.YesNo);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        _biobaseDevice.CancelAcquisition();
                        return; // 2. Cancel Acquire without changing UI device state (return)
                    }
                }
                else
                {
                    throw new BioBaseException(ex.ReturnCode);
                }
            }

            SetDeviceState(DeviceState.device_opened_and_live);
            m_bAskRecapture = false;     // Used to confirm button is for contrast adjustment
            _biobaseDevice.mostResentKey = ActiveKeys.KEYS_OK_CONTRAST;
            _SetGuidanceElements();

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //NOTE: This would be a good spot to add an UI Prompt for user on place the correct position and impression on the platen
            // For application using the visualizer logic this can be done by calling the _SetImageText() method.
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (checkBoxVisualization.Checked == true)
            {
                if (_biobaseDevice.mostRecentImpression == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL)
                    _SetImageText("Roll finger horizontally!");
                else if (_biobaseDevice.mostRecentImpression == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL_VERTICAL)
                    _SetImageText("Roll finger vertically!");
                else if (_biobaseDevice.mostRecentImpression == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_FLAT)
                {  //"Place position!"
                    string msg = "Place " + _biobaseDevice.mostRecentPosition + "!";
                    _SetImageText(msg);
                }
                else
                    _SetImageText("Place fingers on platen.");
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseDevice();
        }

        private void btnAcquire_Click(object sender, EventArgs e)
        {
            try
            {
                if (_biobaseDevice.IsDeviceReady() == false)
                {
                    AddMessage("Device is not ready to capture.");
                    return;
                }

                // Remove any _biobaseDevice_DataAvailable image. 1. won't conflict with visualization image. 2. ensure security of personal data (GDPR)!!!!
                ImageBox.Image = null;
                ImageBox.Update();

                bool bAutoCaptureSupported = false;

                bool bFlexRollSupported = false;
                bool bFlexFlatSupported = false;

                if (_deviceType == null)
                {
                    bFlexRollSupported = false;
                    bFlexFlatSupported = false;
                }
                else if ((_deviceType == LSEConst.DEVICE_TYPE_LSCAN_500P)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_500PJ)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_500)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_1000PX)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_1000P)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_1000)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_1000T))
                {
                    bFlexFlatSupported = true;
                    bFlexRollSupported = false;
                    AddMessage(string.Format("Opening device type {0}, FlexFlat = true, FlexRoll=false", _deviceType));
                }
                else if ((_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN_MODULE)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_GUARDIAN_FW)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_GUARDIAN_USB)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_GUARDIAN_T)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_GUARDIAN_R2)
                       || (_deviceType == LSEConst.DEVICE_TYPE_LSCAN_GUARDIAN_L)
                       || (_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN)
                       || (_deviceType == LSEConst.DEVICE_TYPE_PATROL)
                       || (_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN_200)
                       || (_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN_300))
                {
                    bFlexFlatSupported = true;
                    bFlexRollSupported = true;
                    AddMessage(string.Format("Opening device type {0}, FlexFlat = true, FlexRoll=true", _deviceType));
                }
                else if (_deviceType == LSEConst.DEVICE_TYPE_GUARDIAN_45)
                {
                    bFlexFlatSupported = false;
                    bFlexRollSupported = false;
                    AddMessage(string.Format("Opening device type {0}, FlexFlat = false, FlexRoll=false", _deviceType));
                }

                // Enable auto capture.
                // Code should first check if auto capture is supported else SetProperty will throw exception
                string test = _biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_AUTOCAPTURE_SUPPORTED);
                if (_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_DEVICE_AUTOCAPTURE_SUPPORTED) == PropertyConstants.DEV_PROP_TRUE)
                {
                    bAutoCaptureSupported = true;
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_ON, PropertyConstants.DEV_PROP_TRUE);
                    AddMessage(string.Format("InFunc [btnAcquire_Click] DEV_PROP_DEVICE_AUTOCAPTURE_SUPPORTED  = true, set DEV_PROP_AUTOCAPTURE_ON =true"));
                }
                else
                {
                    string strTF = "false";
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_ON, PropertyConstants.DEV_PROP_FALSE);
                    if (_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_ON) == PropertyConstants.DEV_PROP_TRUE)
                    {
                        strTF = "true";
                    }
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_ON, PropertyConstants.DEV_PROP_FALSE);
                    AddMessage(string.Format("InFunc [btnAcquire_Click] DEV_PROP_DEVICE_AUTOCAPTURE_SUPPORTED  = false,  DEV_PROP_AUTOCAPTURE_ON ={0}", strTF));

                }

                //Allow Autocontrast for flat if checkbox is selected but no Autocontrast for rolls
                if ((comboBox_Impression.Text == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_FLAT) && (checkBoxAutocontrast.Checked))
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCONTRAST_ON, PropertyConstants.DEV_PROP_TRUE);
                else
                {
                    checkBoxAutocontrast.Checked = false; // Autocontrast should not be used for rolls - Rolled image can have uneven contrast
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCONTRAST_ON, PropertyConstants.DEV_PROP_FALSE);
                }

                //Check UI option for capture override options
                if (checkBoxAltTrigger.Checked && bAutoCaptureSupported)
                {
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_OVERRIDE_ON, PropertyConstants.DEV_PROP_TRUE);
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_OVERRIDE_TIME, "4000");
                    if (radioButtonInsufficientObjectCount.Checked)
                        _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_OVERRIDE_MODE, PropertyConstants.DEV_PROP_ON_INSUFFICIENT_COUNT);
                    else
                        _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_OVERRIDE_MODE, PropertyConstants.DEV_PROP_ON_INSUFFICIENT_QUALITY);
                }
                else
                {
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_OVERRIDE_ON, PropertyConstants.DEV_PROP_FALSE);
                    checkBoxVisualization.Checked = true;
                }


                //Set option to check for spoof detection AKA presentation attack detection (PAD) 
                //Only set option if supported by device else BeginAcquire will return an error
                _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_SPOOF_DETECTION_ON, (checkBoxPAD.Checked) ? PropertyConstants.DEV_PROP_TRUE : PropertyConstants.DEV_PROP_FALSE);

                // Set number of fingers (objects) being captured based on Position
                // This allows the application to annotate a finger (i.e. acquire "RightFour" with only 3 fingers
                _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_NUM_RQD_OBJECTS, comboBox_NumObjCapture.Text);
                //Int16 fingerCount = Int16.Parse(comboBox_NumObjCapture.Text);
                SetUILedColors(4); // Display all four UI status LEDs for LSE because they correspond to location finger is detected on platen.

                //Set option to allow the image resolution to be changed
                // Checked if device supports setting else the SetProperty will throw exception
                // Must be done before checking Flex Capture so it knows the resolution
                _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_IMAGE_RESOLUTION, (checkBox1000dpi.Checked) ? PropertyConstants.DEV_PROP_RESOLUTION_1000 : PropertyConstants.DEV_PROP_RESOLUTION_500);


                //Check option for Flex Roll Area capture
                // Flex roll capture supported on Guardian, Guardian 300, Guardian 200, Guardian Module
                if (comboBox_Impression.Text == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL)
                {
                    m_ImpressionModeRoll = true;
                    if (checkBoxFlexRollCapture.Checked)
                    {
                        if ((_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_ROLL_FLEXIBLE) != PropertyConstants.DEV_PROP_NOT_SET) && bFlexRollSupported)
                        { // Flex roll property is settable...
                            _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ROLL_FLEXIBLE, PropertyConstants.DEV_PROP_TRUE);
                            _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "-1 -1 800 748");
                        }
                        else
                        {
                            // checkBoxFlexRollCapture checked but flex rolls is NOT VALID for this device.
                            checkBoxFlexRollCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                                                                      // traditional (non-flex) capture area.
                            _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "0 0 0 0");
                        }
                    }
                    else
                    {
                        // DEV_PROP_ROLL_FLEXIBLE property is persistant so it must be turned off if settable
                        if (_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_ROLL_FLEXIBLE) != PropertyConstants.DEV_PROP_NOT_SET)
                            _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ROLL_FLEXIBLE, PropertyConstants.DEV_PROP_FALSE);
                        // traditional (non-flex) capture area
                        _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "0 0 0 0");
                    }

                    // Must use use visualization window for flex roll capture
                    if (checkBoxFlexRollCapture.Checked == true)
                        checkBoxVisualization.Checked = true;
                }


                //Check option for Flex Flast Area capture
                // Flex flat capture supported on LScan 500P, LScan 500PJ, LScan 500, and LScan 1000PX, LScan 1000, Guardian, Guardian 300, Guardian 200, Guardian 100, Guardian Module
                else if (comboBox_Impression.Text == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_FLAT)
                {
                    m_ImpressionModeRoll = false;
                    if (checkBoxFlexFlatCapture.Checked)
                    {
                        string FlexArea = "-1 -1 800 748";

                        if (_biobaseDevice.GetProperty(PropertyConstants.DEV_PROP_IMAGE_RESOLUTION) == PropertyConstants.DEV_PROP_RESOLUTION_1000)
                        { // 1000 dpi palm scanner
                            switch (comboBox_Position.Text)
                            {
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FULL_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_WRITERS_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LOWER_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_UPPER_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FULL_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_WRITERS_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LOWER_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_UPPER_PALM):
                                    // checkBoxFlexFlatCapture checked but flex flat is NOT VALID for this device.
                                    checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                                                                              // traditional (non-flex) capture area.
                                    FlexArea = "0 0 0 0";
                                    break;

                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES):
                                    FlexArea = "-1 -1 3200 3000";   //flex flat capture area for 4 fingers or 2 thumbs at 1000dpi
                                    if (!bFlexFlatSupported)
                                    {
                                        FlexArea = "0 0 0 0";
                                        checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                                    }
                                    break;

                                case (PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE):
                                    checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option
                                    FlexArea = "0 0 0 0";     // traditional (non-flex) capture area
                                    break;
                                default:  // And all the single finger positions
                                    FlexArea = "-1 -1 3200 3000";   //flex flat capture area for 4 fingers or 2 thumbs at 1000dpi
                                    if (!bFlexFlatSupported)
                                    {
                                        FlexArea = "0 0 0 0";
                                        checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                                    }
                                    //checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.

                                    /// The single finger postion must be changed to DEV_PROP_POS_TYPE_BOTH_THUMBS for flex flat work!
                                    /// BUT this hack will cause the FIR record to return DEV_PROP_POS_TYPE_BOTH_THUMBS instead of the requested position!!!
                                    //FlexArea = "-1 -1 1600 1496";  //flex flat capture area for 2 fingers (and 1 finger) at 1000dpi
                                    //comboBox_Position.Text = PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS;
                                    break;
                            }
                        }
                        else
                        {   // 500dpi - Guardian and LScan palm
                            switch (comboBox_Position.Text)
                            {
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FULL_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_WRITERS_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LOWER_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_UPPER_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FULL_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_WRITERS_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LOWER_PALM):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_UPPER_PALM):
                                    // checkBoxFlexFlatCapture checked but flex flat is NOT VALID for this postion.
                                    checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                                                                              // traditional (non-flex) capture area.
                                    FlexArea = "0 0 0 0";
                                    break;

                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES):
                                    FlexArea = "-1 -1 1600 1496";   //flex flat capture area for 4 fingers or 2 thumbs at 500dpi
                                    if (!bFlexFlatSupported)
                                    {
                                        FlexArea = "0 0 0 0";
                                        checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                                    }
                                    break;

                                case (PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE):
                                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE):
                                    FlexArea = "0 0 0 0";     // traditional (non-flex) capture area.
                                    checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                                    break;
                                default:  // And all the single finger positions
                                    FlexArea = "-1 -1 1600 1496";   //flex flat capture area for 4 fingers or 2 thumbs at 500dpi
                                    if (!bFlexFlatSupported)
                                    {
                                        FlexArea = "0 0 0 0";  // traditional (non-flex) capture area.
                                        checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.
                                    }
                                    //checkBoxFlexFlatCapture.Checked = false;  // Uncheck UI option to warn operator of invalid option.

                                    /// The single finger postion must be changed to DEV_PROP_POS_TYPE_BOTH_THUMBS for flex flat work!
                                    /// BUT this hack will cause the FIR record to return DEV_PROP_POS_TYPE_BOTH_THUMBS instead of the requested position!!!
                                    //FlexArea = "-1 -1 800 748";   //flex flat capture area for 2 fingers (and 1 finger) at 500dpi
                                    //comboBox_Position.Text = PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS;
                                    break;
                            }
                        }
                        _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, FlexArea);

                        // Must use use visualization window for flex flat capture
                        if (checkBoxFlexFlatCapture.Checked == true)
                            checkBoxVisualization.Checked = true;
                    }// checkBoxFlexFlatCapture.Checked
                    else
                    {
                        // traditional (non-flex) flat capture area
                        _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "0 0 0 0");
                    }
                }//DEV_PROP_IMPR_TYPE_FINGERPRINT_FLAT
                else if (comboBox_Impression.Text == PropertyConstants.DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL_VERTICAL)
                {// DEV_PROP_IMPR_TYPE_FINGERPRINT_ROLL_VERTICAL, DEV_PROP_IMPR_TYPE_FINGERPRINT_UNKNOWN, etc.
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "0 0 0 0");
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_AUTOCAPTURE_ON, PropertyConstants.DEV_PROP_FALSE);
                }
                else
                {  //DEV_PROP_IMPR_TYPE_FINGERPRINT_UNKNOWN, etc.
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_ACTIVE_AREA, "0 0 0 0");
                }


                if (checkBoxVisualization.Checked == true)
                {
                    // Setup visualization window...
                    _biobaseDevice.SetVisualizationWindow(ImageBox.Handle, PropertyConstants.DEV_ID_VIS_FINGER_WND, BioBOsType.BIOB_WIN32OS);

                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_VISUALIZATION_MODE, PropertyConstants.DEV_PROP_VISMODE_PREVIEW_ONLY);
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_VISUALIZATION_FULLIMAGE_ON, PropertyConstants.DEV_PROP_FALSE);
                    _biobaseDevice.SetProperty(PropertyConstants.DEV_PROP_VISUALIZATION_BK_COLOR, PropertyConstants.DEV_PROP_DEFAULT_BK_COLOR);
                }


                // Reset LEDs in GUI on start of capture
                SetUILedColors(ActiveColor.gray, ActiveColor.gray, ActiveColor.gray, ActiveColor.gray, ActiveColor.gray);

                _biobaseDevice.mostRecentPosition = comboBox_Position.Text;        //Save position for StartAcquire and in case we need to re-capture. also used in SetOutputData
                _biobaseDevice.mostRecentImpression = comboBox_Impression.Text;    //Save impression for StartAcquire and in case we need to re-capture. also used in SetOutputData
                StartAcquire();

            }
            catch (BioBaseException ex)
            {
                MessageBox.Show(string.Format("Setting up device for Acquire BioBase error {0}", ex.Message));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Setting up device for Acquire error {0}", ex.Message));
            }
        }

        private void btnCloseDevice_Click(object sender, EventArgs e)
        {
            try
            {
                if (_biobaseDevice != null)
                    CloseDevice();
                m_scannerOpen = false;

                _biobaseDevices = new BioBaseDeviceInfo[0];
                FillDeviceListBox();
                SetDeviceState(DeviceState.device_not_connected);

                _biobase.Close();
                AddMessage("Close BioBase API. Must call Open BioBase API before continuing.");
            }
            catch (BioBaseException ex)
            {
                AddMessage(string.Format("BioB_Close BioBase error {0}", ex.Message));
            }
            catch (Exception ex)
            {
                AddMessage(string.Format("BioB_Close error {0}", ex.Message));
            }

            // Reset LEDs in GUI on clsoedevice
            SetUILedColors(ActiveColor.gray, ActiveColor.gray, ActiveColor.gray, ActiveColor.gray, ActiveColor.gray);

            CloseDevice();
        }

        private void btnOpenDevice_Click(object sender, EventArgs e)
        {
            OpenAPI();
            OpenDevice();
        }

        private void btnCancelAcquire_Click(object sender, EventArgs e)
        {
            try
            {
                AddMessage("Cancel capture.");
                _biobaseDevice.CancelAcquisition();
                SetDeviceState(DeviceState.device_opened_and_capture_cancelled);

                //Reset device's LEDs, TFT display or Touch display here
                _biobaseDevice.mostResentKey = ActiveKeys.KEYS_NONE;
                _ResetStatusElements();
                _ResetGuidanceElements();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Cancel capture error {0}", ex.Message));
            }
        }
        //-------------------------------------------------------------//



        private void ConvertBmpToWsq(Bitmap bmpIn)
        {
            string fpName = "";
            comboBox_Position.Invoke(new Action(() => fpName = comboBox_Position.Text));

            string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

            string tempPath = AppDomain.CurrentDomain.BaseDirectory + @"output\" + fileName + "_" + fpName + ".bmp";
            bmpIn.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);
            bmpIn.Dispose();

            //string outputPath = AppDomain.CurrentDomain.BaseDirectory + @"output\" + fileName + "_" + fpName + ".wsq";

            //Wsqm.WSQ dec = new Wsqm.WSQ();

            //String[] comentario = new String[2];
            //comentario[0] = "wacinfotech";
            //comentario[1] = "wacinfotech";

            //dec.EnconderFile(tempPath, outputPath, comentario, 0.75f);  // image to wsq
            //                                                            //dec.DecoderFile(@"output.wsq", @"output.bmp"); // wsq to image

            //dec = null;
        }

        private void comboBox_Position_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_Position.Text)
            {
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_THUMB):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_MIDDLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LITTLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_THUMB):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_MIDDLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LITTLE):

                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_WRITERS_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_LOWER_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_WRITERS_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_LOWER_PALM):
                    comboBox_NumObjCapture.Text = "1";
                    // Default to capture 1 object.
                    break;
                case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_THUMBS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_TWO_FINGERS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_INDEX_AND_MIDDLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_RING_AND_LITTLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_INDEX_AND_MIDDLE):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_RING_AND_LITTLE):
                    // Default to capture 2 objects.
                    comboBox_NumObjCapture.Text = "2";
                    break;
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FOUR_FINGERS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FOUR_FINGERS):
                case (PropertyConstants.DEV_PROP_POS_TYPE_BOTH_INDEXES_AND_MIDDLES):
                    // Default to capture 4  objects. In case of annotation, this will need to edit
                    comboBox_NumObjCapture.Text = "4";
                    break;

                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_FULL_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_RIGHT_UPPER_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_FULL_PALM):
                case (PropertyConstants.DEV_PROP_POS_TYPE_LEFT_UPPER_PALM):
                    // Default to capture 5 objects. In case of annotation, this will need to edit
                    comboBox_NumObjCapture.Text = "5";
                    break;
            }
        }

        private void checkBoxFlexFlatCapture_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxFlexFlatCapture.Checked == true)
            {
                checkBoxVisualization.Enabled = false;
                checkBoxVisualization.Checked = true;
            }

            if ((checkBoxFlexFlatCapture.Checked == false) && (checkBoxFlexRollCapture.Checked == false))
            {
                checkBoxVisualization.Enabled = true;
            }
        }

        private void checkBoxFlexRollCapture_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxFlexRollCapture.Checked == true)
            {
                checkBoxVisualization.Enabled = false;
                checkBoxVisualization.Checked = true;
            }

            if ((checkBoxFlexFlatCapture.Checked == false) && (checkBoxFlexRollCapture.Checked == false))
            {
                checkBoxVisualization.Enabled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Save final captured image
            try
            {
                if (_imageCaptured == false)
                {
                    AddMessage("There is no image to save.");
                    return;
                }
                Bitmap ImageData = (Bitmap)ImageBox.Image;
                ConvertBmpToWsq(ImageData);

                MessageBox.Show(string.Format("Successed"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Problem saving image to file. Error:{0}", ex.Message));
            }
        }

        //********************************//

        public PointF endPos;
        public PointF beginPos;
        public PointF frontPos;
        private int xypointcount = 0;
        private int[] lastpointx = new int[3];
        private int[] lastpointy = new int[3];

        Point mPoint = new Point(-1, -1);


        private System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
        private Graphics g;
        //private Graphics mGraphicsBuffer;
        private Pen pen = new Pen(Color.Black); //

        //*** กดปุ่ม save เพื่อกำหนดชื่อไฟล์ก่อน ค่อยกด Submit จากเครื่อง!!!! ***//

        private Bitmap MakeTransparent(Bitmap image, int threshold)
        {
            Bitmap newImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    if (pixel.A < threshold)
                    {
                        newImage.SetPixel(x, y, Color.Transparent);
                    }
                    else
                    {
                        newImage.SetPixel(x, y, pixel);
                    }
                }
            }

            return newImage;
        }
        public void GetTouchNumber(int number)
        {
            Console.WriteLine("GetTouchNumber：" + number);
            if (number == 1)  //1 ok!
            {
                int ret = FiveInchDll.ComSignOK();
                Console.WriteLine("ComSignOK：" + ret);
                g.Clear(System.Drawing.Color.White);


                Bitmap originalImage = new Bitmap(System.IO.Directory.GetCurrentDirectory() + "\\5inch_signok.png");
                Bitmap transparentImage = MakeTransparent(originalImage, 128); // Example threshold value
                originalImage.Dispose();

                transparentImage.Save(System.IO.Directory.GetCurrentDirectory() + "\\5inch_signok.png");
                transparentImage.Dispose();

                Bitmap bitmap;
                using (System.IO.Stream bmpStream = System.IO.File.Open(System.IO.Directory.GetCurrentDirectory() + "\\5inch_signok.png", System.IO.FileMode.Open))
                {
                    Image image = Image.FromStream(bmpStream);
                    bitmap = new Bitmap(image);
                    pictureBox2.Image = bitmap;
                }
            }
            else
            {
                try
                {
                    g.Clear(System.Drawing.Color.White);
                }
                catch
                {
                    //
                }
                lastpointx[0] = -1;
                lastpointy[0] = -1;
                lastpointx[1] = -1;
                lastpointy[1] = -1;
                lastpointx[2] = -1;
                lastpointy[2] = -1;
            }
        }


        //0816加报点
        public void GetTouchPoint(TOUCH_INFO[] info1)
        {


            int x = 0, y = 0;
            int pressurevl;
            int dx = 0, dy = 0;


            for (int k = 0; k < 80; k++)
            {

                x = info1[k].X;
                y = info1[k].Y;

                //落笔
                if (info1[k].Pressure > 0)
                {
                    if (info1[k].Pressure > 0 && info1[k].Pressure < 500)
                    {
                        pressurevl = 1;
                        pen.Width = 1;
                    }
                    else if (info1[k].Pressure >= 500 && info1[k].Pressure < 1000)
                    {
                        pressurevl = 2;
                        pen.Width = 2;
                    }
                    else if (info1[k].Pressure >= 1000 && info1[k].Pressure < 1500)
                    {
                        pressurevl = 3;
                        pen.Width = 3;
                    }
                    else if (info1[k].Pressure >= 1500 && info1[k].Pressure < 2048)
                    {
                        pressurevl = 4;
                        pen.Width = 4;
                    }
                    else
                    {
                        pressurevl = 0;
                        pen.Width = 1;
                    }
                }
                else
                {
                    //抬笔
                    pressurevl = 0;

                    lastpointx[0] = -1;
                    lastpointy[0] = -1;
                    lastpointx[1] = -1;
                    lastpointy[1] = -1;
                    lastpointx[2] = -1;
                    lastpointy[2] = -1;
                    continue;
                }

                if (info1[k].Pressure > 10)   //有画线宽度
                {
                    lastpointx[2] = x;
                    lastpointy[2] = y;


                    if (lastpointx[2] != -1)
                    {
                        if (lastpointx[1] != -1 && lastpointx[0] != -1)
                        {
                            //float dx = Math.Abs(lastpointx[2] - beginPos.X);
                            //float dy = Math.Abs(endPos.Y - beginPos.Y);

                            dx = Math.Abs(lastpointx[2] - lastpointx[1]);
                            dy = Math.Abs(lastpointy[2] - lastpointy[1]);
                            if ((dx != 0) && (dy != 0))
                            {

                                if (lastpointy[1] != -1 && lastpointy[2] != -1)  //y轴相同的点不画,直接跳过
                                {
                                    if (lastpointx[1] != -1 && lastpointx[2] != -1)  //第3个点和第二个点比较是否x坐标在同一个位置,不是就执行画第一个点到第二个点的线
                                    {
                                        //painter->drawLine(frontPos, beginPos);   //画线
                                        g.DrawLine(pen, lastpointx[0], lastpointy[0], lastpointx[1], lastpointy[1]);
                                        //painter->drawPoint(beginPos);         //画点
                                        //frontPos = beginPos;
                                        //beginPos = endPos;
                                        lastpointx[0] = lastpointx[1];
                                        lastpointy[0] = lastpointy[1];
                                        lastpointx[1] = lastpointx[2];
                                        lastpointy[1] = lastpointy[2];
                                    }
                                    else
                                    {
                                        //是就执行画第一个点到第三个点的线
                                        //painter->drawLine(frontPos, endPos);
                                        g.DrawLine(pen, lastpointx[0], lastpointy[0], lastpointx[2], lastpointy[2]);
                                        //frontPos = endPos;                      //第三个点赋值第一个点
                                        //beginPos = QPointF(0, 0);                //第二个点置空
                                        //beginPos.X = -1;
                                        //beginPos.Y = -1;
                                        lastpointx[0] = lastpointx[2];
                                        lastpointy[0] = lastpointy[2];
                                        lastpointx[1] = -1;
                                        lastpointy[1] = -1;
                                    }
                                }
                            }
                        }//
                        else
                        {
                            if (lastpointx[1] != -1)  //不为空在赋值,防止丢弃点时赋空值
                            {
                                lastpointx[0] = lastpointx[1];
                                lastpointy[0] = lastpointy[1];
                            }
                            lastpointx[1] = lastpointx[2];
                            lastpointy[1] = lastpointy[2];
                        }
                    }

                }
                else
                {
                    dx = dy = 0;
                    lastpointx[0] = -1;
                    lastpointy[0] = -1;
                    lastpointx[1] = -1;
                    lastpointy[1] = -1;
                    lastpointx[2] = -1;
                    lastpointy[2] = -1;

                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int ret = FiveInchDll.OpenComDevice(GetTouchNumber);
            Console.WriteLine("OpenComDevice：" + ret);

            if (ret == 0)
            {
                //2022-08-16 加实时报点
                ret = FiveInchDll.ComSendPoint(1, GetTouchPoint);
                Console.WriteLine("ComSendPoint：" + ret);
            }

            //++++++ wittaya add
            string SignFile = System.IO.Directory.GetCurrentDirectory() + "\\5inch_signok.png";
            ret = FiveInchDll.ComSetPictureSavePath(SignFile, 255);
            Console.WriteLine("ComSetPictureSavePath：" + ret);
            //------
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int ret = FiveInchDll.CloseComDevice();
            Console.WriteLine("CloseComDevice：" + ret);
        }
        
    }
}

//0816加实时报点
[StructLayout(LayoutKind.Sequential)]
public struct TOUCH_INFO
{
    public int X;
    public int Y;
    public int Pressure;
    public int SN;
    public int btnID;//5寸 确定 重签 取消按钮
}
public class FiveInchDll
{
    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
    public delegate void GetTouchNumber(int number);

    //0816加报点
    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
    public delegate void TOUCH_INFO_FUNC([MarshalAs(UnmanagedType.LPArray, SizeConst = 160)] TOUCH_INFO[] info);

    [DllImport("XTJZFiveInch.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int ComSendPoint(int nState, [MarshalAs(UnmanagedType.FunctionPtr)] TOUCH_INFO_FUNC callback); //0816加报点

    [DllImport("XTJZFiveInch.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int OpenComDevice([MarshalAs(UnmanagedType.FunctionPtr)] GetTouchNumber callback);

    [DllImport("XTJZFiveInch.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int CloseComDevice();

    [DllImport("XTJZFiveInch.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int ComSetSignBackgroundImage(string UIFile);

    [DllImport("XTJZFiveInch.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int ComSetPictureSavePath(string PicturePath, int PicturePathLen);

    [DllImport("XTJZFiveInch.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    public static extern int ComSignOK();

    //internal static int ComSendPoint(int v, Action<Form1.TOUCH_INFO> getTouchPoint)
    //{
    //    throw new NotImplementedException();
    //}
}