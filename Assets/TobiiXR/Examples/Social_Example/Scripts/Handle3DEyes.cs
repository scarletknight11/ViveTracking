// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR;
using UnityEngine;

/// <summary>
/// Monobehaviour which handles the eye direction for 3D eyes.
/// </summary>
namespace Photon.Pun.Demo.PunBasics
{
    public class Handle3DEyes : MonoBehaviourPunCallbacks, IPunObservable
    {
#pragma warning disable 649
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        //public bool avertEyes = false; // Used for previous Charles or Ihshan's code
        //public bool directEyes = false; // Used for previous Charles or Ihshan's code

        public enum Orientation { avertedEyes, directedEyes, naturalEyes };
        public Orientation orientation;
        //private Orientation eyeDirectionMaster;



        [Header("Eye Transforms")]
        [SerializeField]
        private Transform _leftEye;

        [SerializeField]
        private Transform _rightEye;

        [Header("Eye Behaviour values")]
        [SerializeField, Tooltip("Cross eye correction for models that look cross eyed. +/-20 degrees.")]
        [Range(-20.0f, 20.0f)]
        private float _crossEyeCorrection;

        [SerializeField, Tooltip("Reduce gaze direction jitters at the cost of responsiveness.")]
        private float _gazeDirectionSmoothTime = 0.03f;

        [SerializeField, Tooltip("Maximum eye vertical angle.")]
        private float _verticalGazeAngleUpperLimitInDegrees = 30;

        [SerializeField, Tooltip("Minimum eye vertical angle.")]
        private float _verticalGazeAngleLowerLimitInDegrees = 30;

        [SerializeField, Tooltip("Maximum eye horizontal angle.")]
        private float _horizontalGazeAngleLimitInDegrees = 35;

#pragma warning restore 649

        private static ExponentialSmoothing _smoothing = new ExponentialSmoothing();

        // Expose eye direction for deriving facial expressions etc. in other components.
        public Vector3 LeftEyeDirection
        {
            get { return _lastGoodDirection; }
        }

        public Vector3 RightEyeDirection
        {
            get { return _lastGoodDirection; }
        }

        private const float CrossEyedCorrectionFactor = 100;
        private const float AngularNoiseCompensationFactor = 800;
        private Transform _middleOfTheEyes;

        private Vector3 _lastGoodDirection = Vector3.forward;
        private Vector3 _previousSmoothedDirectionL = Vector3.zero;
        private Vector3 _previousSmoothedDirectionR = Vector3.zero;
        private Vector3 _smoothDampVelocityL;
        private Vector3 _smoothDampVelocityR;

        private TobiiXR_EyeTrackingData eyeData;

        private void Awake()
        {
            var middlePoint = new GameObject();
            middlePoint.transform.parent = _leftEye.parent;
            middlePoint.name = "middleOfTheEyes";
            _middleOfTheEyes = middlePoint.transform;
            _middleOfTheEyes.localRotation = Quaternion.Euler(Vector3.zero);
            _middleOfTheEyes.localScale = Vector3.one;
            _middleOfTheEyes.position = (_leftEye.position + _rightEye.position) / 2;

            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
                //this.GetComponentInChildren<Camera>().enabled = true;
            }
            else
            {
                this.GetComponent<FollowCameraHeight>().enabled = false;
                this.GetComponentInChildren<RotateWithHMD>().enabled = false;
                this.GetComponentInChildren<HandleHands>().isMine = false;
            }

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            //DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                this.ProcessInputs();
                
            }

            // To activate or deactivate Eye Lids
            if (photonView.IsMine && orientation == Orientation.avertedEyes)
            {
                var handle3DEyeLids = GetComponent<Handle3DEyelids>();
                handle3DEyeLids.enabled = false;
            }
            else if(!photonView.IsMine && orientation == Orientation.avertedEyes)
            {
                var handle3DEyeLids = GetComponent<Handle3DEyelids>();
                handle3DEyeLids.enabled = false;
            }
            else if (photonView.IsMine && orientation == Orientation.directedEyes)
            {
                this.ProcessInputs();
                var handle3DEyeLids = GetComponent<Handle3DEyelids>();
                handle3DEyeLids.enabled = false;
            }
            else if (!photonView.IsMine && orientation == Orientation.directedEyes)
            {
                this.ProcessInputs();
                var handle3DEyeLids = GetComponent<Handle3DEyelids>();
                handle3DEyeLids.enabled = false;
            }
            else if (photonView.IsMine && orientation == Orientation.naturalEyes)
            {
                var handle3DEyeLids = GetComponent<Handle3DEyelids>();
                handle3DEyeLids.enabled = true;
            }
            else if (!photonView.IsMine && orientation == Orientation.naturalEyes)
            {
                var handle3DEyeLids = GetComponent<Handle3DEyelids>();
                handle3DEyeLids.enabled = false;
            }
            // ************ WORK ON THIS LATER ****************
            //// For changing eye directions of Local player
            //var master = PhotonNetwork.MasterClient;

            //if (master.IsMasterClient)
            //{
            //    var eyeDirectionMaster = GetComponent<EyeDirectionMaster>();
            //    eyeDirectionMaster.currentEyeDirection = orientation;
            //    Debug.Log(eyeDirectionMaster.currentEyeDirection);
            //}
            //else if (master.IsLocal)
            //{
            //    var eyeDirectionMaster = GetComponent<EyeDirectionMaster>();
            //    orientation = eyeDirectionMaster.currentEyeDirection;
            //    Debug.Log(orientation);

            //}


            // Explore this to set eye direction in master that changes also in client
            //PhotonNetwork.MasterClient.SetCustomProperties();


            //if (orientation == Orientation.directedEyes)
            //{
            //    photonView.RPC("RPC_EyeDirectionSync", RpcTarget.All, orientation);
            //}
            //***********************************************************************
            if (eyeData == null)
                return;

            // Get local transform direction.
            var gazeDirection = eyeData.GazeRay.Direction;

            // If direction data is invalid use other eye's data or if that's invalid use last good data.
            gazeDirection = eyeData.GazeRay.IsValid ? gazeDirection : _lastGoodDirection;

            // Save last good data.
            _lastGoodDirection = gazeDirection;

            // Correct how some avatar models look cross eyed.
            gazeDirection.x += (_crossEyeCorrection / CrossEyedCorrectionFactor);

            // Clamp the gaze angles within model's thresholds.
            gazeDirection = ClampVerticalGazeAngles(gazeDirection, _verticalGazeAngleLowerLimitInDegrees, _verticalGazeAngleUpperLimitInDegrees);
            gazeDirection = ClampHorizontalGazeAngles(gazeDirection, _horizontalGazeAngleLimitInDegrees);

            _smoothing.Process(eyeData.ConvergenceDistanceIsValid ? eyeData.ConvergenceDistance : 2.0f); // Smooth towards 2 meters if data is missing

            var convergencePoint = _middleOfTheEyes.position + (_middleOfTheEyes.TransformDirection(gazeDirection) * (_smoothing.CurrentData));

            var directionR = convergencePoint - _rightEye.position;
            var directionL = convergencePoint - _leftEye.position;

            // Increase smoothing for noisier higher angles
            var angle = Vector3.Angle(gazeDirection, Vector3.forward);
            var compensatedSmoothTime = _gazeDirectionSmoothTime + angle / AngularNoiseCompensationFactor;

            var smoothedDirectionLeftEye = Vector3.SmoothDamp(_previousSmoothedDirectionL, directionL, ref _smoothDampVelocityL, compensatedSmoothTime);
            var smoothedDirectionRightEye = Vector3.SmoothDamp(_previousSmoothedDirectionR, directionR, ref _smoothDampVelocityR, compensatedSmoothTime);
            _previousSmoothedDirectionL = smoothedDirectionLeftEye;
            _previousSmoothedDirectionR = smoothedDirectionRightEye;

            // Call different eye gaze directions by using a drop down menu
            if (orientation == Orientation.naturalEyes)
            {
                // Rotate the eye transforms to match the eye direction.
                var leftRotation = Quaternion.LookRotation(smoothedDirectionLeftEye);
                var rightRotation = Quaternion.LookRotation(smoothedDirectionRightEye);

                _leftEye.rotation = leftRotation;
                _rightEye.rotation = rightRotation;

            }
            else if (orientation == Orientation.avertedEyes)
            {
                _leftEye.localEulerAngles = new Vector3(0, -25, 0);
                _rightEye.localEulerAngles = new Vector3(0, -25, 0);
                //_leftEye.localEulerAngles = new Vector3(-5, 30, 0);

            }
            else // Directed gaze
            {
                _leftEye.localEulerAngles = new Vector3(0, 0, 0);
                _rightEye.localEulerAngles = new Vector3(0, 0, 0);
            }

            // Charles' original code

            //if(!avertEyes){
            //  // Rotate the eye transforms to match the eye direction.
            //  var leftRotation = Quaternion.LookRotation(smoothedDirectionLeftEye);
            //  var rightRotation = Quaternion.LookRotation(smoothedDirectionRightEye);

            //  _leftEye.rotation = leftRotation;
            //  _rightEye.rotation = rightRotation;
            //}
            //else{
            //  _leftEye.localEulerAngles = new Vector3(-16, -21, 0);
            //  _rightEye.localEulerAngles = new Vector3(-16, -21, 0);
            //}


            // Ihshan' edited code for averted eye direction

            // NOTE for adjusting Vector3 related to the Iris (a bit weird for the coordinates !!)
            // X-axis = (+ve) moving UP the Iris, (-ve) moving DOWN the Iris
            // Y-axis = (+ve) moving RIGHT the Iris, (-ve) moving LEFT the Iris

            //if (avertEyes)
            //{
            //    // Averted eye gaze
            //    _leftEye.localEulerAngles = new Vector3(-5, 30, 0);
            //    _rightEye.localEulerAngles = new Vector3(-5, 30, 0);
            //}
            //else if (directEyes)
            //{
            //    // Direct eye gaze (look at straight forward)
            //    _leftEye.localEulerAngles = new Vector3(-5, 6, 0);
            //    _rightEye.localEulerAngles = new Vector3(-5, 6, 0);
            //}
            //else
            //{
            //    // Rotate the eye transforms to match the eye direction.
            //    var leftRotation = Quaternion.LookRotation(smoothedDirectionLeftEye);
            //    var rightRotation = Quaternion.LookRotation(smoothedDirectionRightEye);

            //    _leftEye.rotation = leftRotation;
            //    _rightEye.rotation = rightRotation;
            //}
        }
       
               
        public void ProcessInputs()
        {
            // Get local copies.
            eyeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);
        }

        /// <summary>
        /// Clamp vertical gaze angles - needs to be done in degrees.
        /// </summary>
        /// <param name="gazeDirection">Direction vector of the gaze.</param>
        /// <param name="lowerLimit">The lower clamp limit in degrees.</param>
        /// <param name="upperLimit">The upper clamp limit  in degrees.</param>
        /// <returns>The gaze direction clamped between the two degree limits.</returns>
        private static Vector3 ClampVerticalGazeAngles(Vector3 gazeDirection, float lowerLimit, float upperLimit)
        {
            var angleRad = Mathf.Atan(gazeDirection.y / gazeDirection.z);
            var angleDeg = angleRad * Mathf.Rad2Deg;

            var y = Mathf.Tan(upperLimit * Mathf.Deg2Rad) * gazeDirection.z;
            if (angleDeg > upperLimit)
            {
                gazeDirection = new Vector3(gazeDirection.x, y, gazeDirection.z);
            }

            y = Mathf.Tan(-lowerLimit * Mathf.Deg2Rad) * gazeDirection.z;
            if (angleDeg < -lowerLimit)
            {
                gazeDirection = new Vector3(gazeDirection.x, y, gazeDirection.z);
            }

            return gazeDirection;
        }

        /// <summary>
        /// Clamp horizontal gaze angles - needs to be done in degrees.
        /// </summary>
        /// <param name="gazeDirection">Direction vector of the gaze.</param>
        /// <param name="limit">The limit to clamp to in degrees.</param>
        /// <returns>The clamped gaze direction.</returns>
        private static Vector3 ClampHorizontalGazeAngles(Vector3 gazeDirection, float limit)
        {
            var angleRad = Mathf.Atan(gazeDirection.x / gazeDirection.z);
            var angleDeg = angleRad * Mathf.Rad2Deg;

            var x = Mathf.Tan(limit * Mathf.Deg2Rad) * gazeDirection.z;
            if (angleDeg > limit)
            {
                gazeDirection = new Vector3(x, gazeDirection.y, gazeDirection.z);
            }

            if (angleDeg < -limit)
            {
                gazeDirection = new Vector3(-x, gazeDirection.y, gazeDirection.z);
            }

            return gazeDirection;
        }

        private class ExponentialSmoothing
        {
            public float CurrentData;
            private float _alpha = 0.25f;
            private float _previousData;

            public ExponentialSmoothing()
            {
                Init(2f, 0.2f);
            }

            public float Alpha { get { return _alpha; } set { _alpha = Mathf.Clamp(value, 0, 1); } }

            public void Init(float startValue, float alpha = 0.25f)
            {
                _previousData = startValue;
                CurrentData = startValue;
                Alpha = alpha;
            }

            public void Process(float pos)
            {
                CurrentData = Alpha * pos + (1 - Alpha) * _previousData;
                _previousData = CurrentData;
            }
        }

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                print("We own this player: send the others our data");
                stream.SendNext(this.eyeData.IsLeftEyeBlinking);
                stream.SendNext(orientation);
            }
            else
            {
                bool status = (bool)stream.ReceiveNext();
                print("Network player, receive data" + status);
                
                orientation = (Orientation)stream.ReceiveNext();
                //this.eyeData = (TobiiXR_EyeTrackingData)stream.ReceiveNext();

            }

        }

        #endregion

        //[PunRPC]

        //private void RPC_updateEyeDirection(Orientation remoteOrientation)
        //{
        //    var handle3Deyes = GetComponent<Handle3DEyes>();
        //    handle3Deyes.orientation = remoteOrientation;
        //}

        [PunRPC]
        private void RPC_EyeDirectionSync(Orientation remoteOrientation)
        {
            var handle3Deyes = GetComponent<Handle3DEyes>();
            handle3Deyes.orientation = remoteOrientation;
        }
    }
}
