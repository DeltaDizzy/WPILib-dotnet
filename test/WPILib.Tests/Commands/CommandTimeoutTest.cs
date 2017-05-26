﻿using System.Threading;
using NUnit.Framework;
using WPILib.Commands;

namespace WPILib.Tests.Commands
{
    [TestFixture]
    public class CommandTimeoutTest : AbstractCommandTest
    {
        [Test, Ignore("Waiting for Timestamps in sim to work")]
        public void TestTwoSecondTimeout()
        {
            ASubsystem subsystem = new ASubsystem();

            MockCommand command = new TimedMockCommand();
            command.AddRequires(subsystem);
            command.MockSetTimeout(2);

            command.Start();

            AssertCommandState(command, 0, 0, 0, 0, 0);
            Scheduler.Instance.Run();
            AssertCommandState(command, 0, 0, 0, 0, 0);
            Scheduler.Instance.Run();
            AssertCommandState(command, 1, 1, 1, 0, 0);
            Scheduler.Instance.Run();
            AssertCommandState(command, 1, 2, 2, 0, 0);
            Scheduler.Instance.Run();
            AssertCommandState(command, 1, 3, 3, 0, 0);
            Thread.Sleep(2000);
            Scheduler.Instance.Run();
            AssertCommandState(command, 1, 4, 4, 1, 0);
            Scheduler.Instance.Run();
            AssertCommandState(command, 1, 4, 4, 1, 0);
        }
    }

    internal class TimedMockCommand : MockCommand
    {
        protected override bool IsFinished()
        {
            return base.IsFinished() || IsTimedOut();
        }
    }
}
