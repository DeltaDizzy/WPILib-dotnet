﻿using System;
using System.Collections.Generic;
using HAL.Simulator;
using HAL.Simulator.Data;
using NetworkTables.Tables;
using NUnit.Framework;
using WPILib.Exceptions;
using static WPILib.DoubleSolenoid;
// ReSharper disable UnusedVariable

namespace WPILib.Tests
{
    [TestFixture(0)]
    [TestFixture(7)]
    [TestFixture(59)]
    public class TestDoubleSolenoid : TestBase
    {
        private readonly int m_module;

        public TestDoubleSolenoid(int module)
        {
            m_module = module;
        }

        public DoubleSolenoid NewDoubleSolenoid()
        {
            return new DoubleSolenoid(m_module, 0, 1);
        }

        private HALSimPCMData GetSolenoids()
        {
            return SimData.PCM[m_module];
        }

        [Test]
        public void TestDoubleSolenoidCreateDefaultModule()
        {
            using (DoubleSolenoid s = new DoubleSolenoid(0, 1))
            {
                Assert.IsTrue(SimData.PCM[0].GetSolenoidInitialized(0));
                Assert.IsTrue(SimData.PCM[0].GetSolenoidInitialized(1));
            }
            Assert.That(SimData.PCM[0].GetSolenoidInitialized(0), Is.False);
            Assert.That(SimData.PCM[0].GetSolenoidInitialized(1), Is.False);
        }

        [Test]
        public void TestDoubleSolenoidModuleUnderLimit()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var s = new DoubleSolenoid(-1, 0, 1);
            });
        }

        [Test]
        public void TestDoubleSolenoidModuleOverLimit()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var s = new DoubleSolenoid(SolenoidModules, 0, 1);
            });
        }

        [Test]
        public void TestDoubleSolenoidCreate()
        {
            using (DoubleSolenoid s = NewDoubleSolenoid())
            {
                Assert.IsTrue(GetSolenoids().GetSolenoidInitialized(0));
                Assert.IsTrue(GetSolenoids().GetSolenoidInitialized(1));
            }
        }

        [Test]
        public void TestMultipleAllocation()
        {
            using (DoubleSolenoid ds = NewDoubleSolenoid())
            {
                Assert.Throws(typeof(AllocationException), () =>
                {
                    var p = NewDoubleSolenoid();
                });
            }
        }

        [Test]
        public void TestDoubleSolenoidCreateAll()
        {
            List<DoubleSolenoid> solenoids = new List<DoubleSolenoid>();
            for (int i = 0; i < SolenoidChannels; i++)
            {
                solenoids.Add(new DoubleSolenoid(m_module, i, i + 1));
                i++;
            }

            foreach (var ds in solenoids)
            {
                ds.Dispose();
            }
        }

        [Test]
        public void TestCreateLowerLimit()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var p = new DoubleSolenoid(m_module, -2, -1);
            });
        }

        [Test]
        public void TestCreateUpperLimit()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var p = new DoubleSolenoid(m_module, SolenoidChannels, SolenoidChannels + 1);
            });
        }

        [Test]
        public void TestSolenoidSetForward()
        {
            using (DoubleSolenoid ds = NewDoubleSolenoid())
            {
                ds.Set(DoubleSolenoid.Value.Forward);
                Assert.IsTrue(GetSolenoids().GetSolenoidOutput(0));
                Assert.IsFalse(GetSolenoids().GetSolenoidOutput(1));
            }
        }

        [Test]
        public void TestSolenoidSetReverse()
        {
            using (DoubleSolenoid ds = NewDoubleSolenoid())
            {
                ds.Set(DoubleSolenoid.Value.Reverse);
                Assert.IsTrue(GetSolenoids().GetSolenoidOutput(1));
                Assert.IsFalse(GetSolenoids().GetSolenoidOutput(0));
            }
        }

        [Test]
        public void TestSolenoidSetOff()
        {
            using (DoubleSolenoid ds = NewDoubleSolenoid())
            {
                ds.Set(DoubleSolenoid.Value.Off);
                Assert.IsFalse(GetSolenoids().GetSolenoidOutput(1));
                Assert.IsFalse(GetSolenoids().GetSolenoidOutput(0));
            }
        }

        [Test]
        public void TestSolenoidGetForward()
        {
            using (DoubleSolenoid ds = NewDoubleSolenoid())
            {
                GetSolenoids().SetSolenoidOutput(0, true);
                GetSolenoids().SetSolenoidOutput(1, false);

                Assert.AreEqual(DoubleSolenoid.Value.Forward, ds.Get());
            }
        }

        [Test]
        public void TestSolenoidGetReverse()
        {
            using (DoubleSolenoid ds = NewDoubleSolenoid())
            {
                GetSolenoids().SetSolenoidOutput(1, true);
                GetSolenoids().SetSolenoidOutput(0, false);

                Assert.AreEqual(DoubleSolenoid.Value.Reverse, ds.Get());
            }
        }

        [Test]
        public void TestSolenoidGetOff()
        {
            using (DoubleSolenoid ds = NewDoubleSolenoid())
            {
                GetSolenoids().SetSolenoidOutput(0, false);
                GetSolenoids().SetSolenoidOutput(1, false);

                Assert.AreEqual(DoubleSolenoid.Value.Off, ds.Get());
            }
        }

        [Test]
        public void TestBlackList()
        {
            using (DoubleSolenoid ds = NewDoubleSolenoid())
            {
                Assert.IsFalse(ds.FwdSolenoidBlackListed);
                Assert.IsFalse(ds.RevSolenoidBlackListed);
            }
        }

        [Test]
        public void TestSmartDashboardType()
        {
            using (DoubleSolenoid s = NewDoubleSolenoid())
            {
                Assert.That(s.SmartDashboardType, Is.EqualTo("Double Solenoid"));
            }
        }

        [Test]
        public void TestUpdateTableNull()
        {
            using (DoubleSolenoid s = NewDoubleSolenoid())
            {
                Assert.DoesNotThrow(() =>
                {
                    s.UpdateTable();
                });
            }
        }

        [Test]
        public void TestInitTable()
        {
            using (DoubleSolenoid s = NewDoubleSolenoid())
            {
                ITable table = new MockNetworkTable();
                Assert.DoesNotThrow(() =>
                {
                    s.InitTable(table);
                });
                Assert.That(s.Table, Is.EqualTo(table));
            }

        }

        [Test]
        public void TestStartLiveWindowMode()
        {
            using (DoubleSolenoid s = NewDoubleSolenoid())
            {
                Assert.DoesNotThrow(() =>
                {
                    s.StartLiveWindowMode();
                });
            }
        }

        [Test]
        public void TestStopLiveWindowMode()
        {
            using (DoubleSolenoid s = NewDoubleSolenoid())
            {
                Assert.DoesNotThrow(() =>
                {
                    s.StopLiveWindowMode();
                });
            }
        }


        [Test]
        public void TestValueChanged()
        {
            using (DoubleSolenoid s = NewDoubleSolenoid())
            {
                s.Set(DoubleSolenoid.Value.Forward);
                Assert.That(s.Get(), Is.EqualTo(Value.Forward));
                s.ValueChanged(null, null, NetworkTables.Value.MakeString("Reverse"), NetworkTables.NotifyFlags.NotifyLocal);
                Assert.That(s.Get, Is.EqualTo(Value.Reverse));
                s.ValueChanged(null, null, NetworkTables.Value.MakeString("Garbage"), NetworkTables.NotifyFlags.NotifyLocal);
                Assert.That(s.Get, Is.EqualTo(Value.Off));
                s.ValueChanged(null, null, NetworkTables.Value.MakeString("Forward"), NetworkTables.NotifyFlags.NotifyLocal);
                Assert.That(s.Get, Is.EqualTo(Value.Forward));
            }
        }
    }
}
