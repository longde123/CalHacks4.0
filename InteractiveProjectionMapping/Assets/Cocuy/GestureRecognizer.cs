using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class GestureRecognizer : MonoBehaviour
{

    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    float horizontal;
    float vertical;
    float anglex;
    float angley;
    float anglez;
    float deep = 1;
    float firstdeep = 1;

    //Debugging shit
    [SerializeField]
    GameObject rightHand;
    [SerializeField]
    GameObject leftHand;
    [SerializeField]
    GameObject rightProjection;
    [SerializeField]
    GameObject leftProjection;

    [SerializeField]
    bool inverted; //if on a projector, the controls are inverted

    //canvas renderingStuff
    [SerializeField]
    Canvas canvas;
    [SerializeField]
    GameObject rightButton;
    [SerializeField]
    GameObject leftButton;

    //Screen distance stuff
    [SerializeField]
    float distToScreen;
    [SerializeField]
    GameObject fakeScreen;
    [SerializeField]
    Vector3 fakeScreenPosition;
    [SerializeField]
    float fakeScreenWidth;
    [SerializeField]
    float fakeScreenHeight;
    //save main cam position
    private Vector3 initialCamPos;

    //trippy shit
    [SerializeField]
    GameObject fluidWrapper;

    private MouseManipulator manipulator;
    void Start()
    {
        manipulator = fluidWrapper.GetComponent<MouseManipulator>();
        initialCamPos = Camera.main.transform.position;
        _Sensor = KinectSensor.GetDefault();
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    public Vector3 projectOntoScreen(Vector3 worldPoint, float screenDist)
    {
        float xDist = initialCamPos.x - worldPoint.x;
        float yDist = initialCamPos.y - worldPoint.y;
        float zDist = initialCamPos.z - worldPoint.z;
        float ratio = screenDist / zDist;

        float newX = xDist * ratio + Camera.main.transform.position.x ;
        float newY = yDist * ratio + Camera.main.transform.position.y ;
        float newZ = zDist * ratio + Camera.main.transform.position.z ;
        return new Vector3(newX, newY, newZ);
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            _Sensor = null;
        }
    }


    void Update()
    {
        if (_Reader == null)
        {
            Debug.Log("null reader");
        }
        if (_Reader != null)
        {
           // Debug.Log("attempting to read");
            var frame = _Reader.AcquireLatestFrame();

            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(_Data);

                frame.Dispose();
                frame = null;

                int idx = -1;
                for (int i = 0; i < _Sensor.BodyFrameSource.BodyCount; i++)
                {
                    if (_Data[i].IsTracked)
                    {
                        idx = i;
                    }
                }
                if (idx > -1)
                {
                    KinectJointFilter filter = new KinectJointFilter();
                    filter.Init();
                    Body body = _Data[idx];
                    filter.UpdateFilter(body);

                    CameraSpacePoint[] filteredJoints = new CameraSpacePoint[Body.JointCount];
                    filteredJoints = filter.GetFilteredJoints();

                    CameraSpacePoint handRightPos = body.Joints[JointType.HandRight].Position;
                    CameraSpacePoint handLeftPos = body.Joints[JointType.HandLeft].Position;

                    //CameraSpacePoint handRightPos = filteredJoints[11]; //handright = 11
                    //CameraSpacePoint handLeftPos = filteredJoints[7]; //hand left = 7


                    Vector3 handRight;
                    Vector3 handLeft;
                    if (inverted)
                    {
                        handRight = (new Vector3(-handRightPos.X * 10f, handRightPos.Y * 10f, handRightPos.Z));
                        handLeft = (new Vector3(-handLeftPos.X * 10f, handLeftPos.Y * 10f, handLeftPos.Z));
                    }
                    else
                    {
                        handRight = (new Vector3(handRightPos.X * 10f, handRightPos.Y * 10f, handLeftPos.Z));
                        handLeft = (new Vector3(handLeftPos.X * 10f, handLeftPos.Y * 10f, handLeftPos.Z));

                    }
                    rightHand.transform.position = handRight;
                    leftHand.transform.position = handLeft;
                    leftProjection.transform.position = projectOntoScreen(handLeft, this.distToScreen);
                    rightProjection.transform.position = projectOntoScreen(handRight, this.distToScreen);

                    Vector3 projectionPointLeft = fakeScreen.transform.InverseTransformPoint(leftProjection.transform.position);
                    float xRatioLeft = (projectionPointLeft.x) / fakeScreenWidth;
                    float yRatioLeft = (projectionPointLeft.y) / fakeScreenHeight;

                    RectTransform r = canvas.GetComponent<RectTransform>();
                    //leftButton.transform.localPosition = new Vector3(xRatioLeft * r.rect.width, yRatioLeft * r.rect.height, 0);

                    Vector3 projectionPointRight = fakeScreen.transform.InverseTransformPoint(rightProjection.transform.position);
                    float xRatioRight = (projectionPointRight.x) / fakeScreenWidth;
                    float yRatioRight = (projectionPointRight.y) / fakeScreenWidth;

                    RectTransform r2 = canvas.GetComponent<RectTransform>();
                    //rightButton.transform.localPosition = new Vector3(xRatioRight * r2.rect.width, yRatioRight * r2.rect.height, 0);


                    Vector3 camSpaceLeft = new Vector3(Screen.width * (xRatioLeft+.5f), Screen.height * (yRatioLeft), 0);

                    Vector3 camSpaceRight = new Vector3(Screen.width * (xRatioRight+ .5f), Screen.height * (yRatioRight), 0);

                    manipulator.trippyUpdate(camSpaceRight);
                    manipulator.trippyUpdate(camSpaceLeft);
                }
            }
        }
    }
}