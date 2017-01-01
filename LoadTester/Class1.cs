﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HAL.Base;
using HAL.Simulator;
using WPILib;
using WPILib.SmartDashboard;
using HAL = HAL.Base.HAL;
using OpenCvSharp;
using CSCore;
using System.IO;
using CTRE;

namespace LoadTester
{
    public class MyRobot : IterativeRobot
    {
        Joystick joystick;
        Servo servo;
        AnalogGyro gyro;
        private CANTalon talon;

        public override void RobotInit()
        {
            talon = new CANTalon(1);
            talon.Set(1.0);
            Console.WriteLine("Successfully Created Talon SRX");
            Thread thread = new Thread(() =>
            {
                UsbCamera camera = CameraServer.Instance.StartAutomaticCapture();
                camera.SetResolution(320, 240);


                CvSink cvSink = CameraServer.Instance.GetVideo();
                CvSource outputStream = CameraServer.Instance.PutVideo("Flip", 320, 240);

                Mat source = new Mat();
                Mat flip = new Mat();
                Mat output = new Mat();

                while (true)
                {
                    cvSink.GrabFrame(source);
                    Cv2.CvtColor(source, flip, ColorConversionCodes.BGR2GRAY);
                    Cv2.Flip(flip, output, FlipMode.X);
                    outputStream.PutFrame(output);
                }
            });

            thread.Start();

            servo = new Servo(0);

            joystick = new Joystick(0);
            gyro = new AnalogGyro(0);
        }

        int count = 0;

        public override void DisabledInit()
        {
            
        }
        public override void DisabledPeriodic()
        {
            SmartDashboard.PutNumber("DisabledCount", count++);
        }

        public override void TeleopPeriodic()
        {
            if (joystick.GetRawButton(1))
            {
                servo.Set(0.0);
            }
            else if (joystick.GetRawButton(2))
            {
                servo.Set(1.0);
            }

            else
            {
                servo.Set(0.5);
            }
        }

        public override void RobotPeriodic()
        {
            //Console.WriteLine("Getting Gyro");
            SmartDashboard.PutNumber("Gyro", gyro.GetAngle());
        }

        public static void Main(string[] args)
        {
            RobotBase.Main(null, typeof(MyRobot));
            /*
            GripPipeline pipeline;
            pipeline = new GripPipeline((v) =>
            {
                Mat last = v.lastImage;
                Cv2.DrawContours(last, v.filterContoursOutput, -1, Scalar.Green, 3);
                Cv2.ImShow("Hello!", last);
                Cv2.WaitKey(1);
                ;
            });

            VideoCapture cap = new VideoCapture(0);
            Mat mat = new Mat();
            while (true)
            {
                cap.Read(mat);
                pipeline.process(mat);

                
            }
            */
        }
    }
}
