﻿//using System;
//using System.Collections.Generic;
//using HAL;
//using HAL.Base;
//using HAL.Simulator;
//using HAL.Simulator.Data;
//using NetworkTables.Tables;
//using NUnit.Framework;
//using WPILib.Exceptions;
//using WPILib.Interfaces;
//// ReSharper disable UnusedVariable

//namespace WPILib.Tests
//{
//    [TestFixture]
//    public class TestCounter : TestBase
//    {
//        private List<CounterData> GetCounterData()
//        {
//            return SimData.Counter;
//        }


//        public void TestInit(Mode mode, uint up, uint down, bool upRising, bool upFalling, bool downRising,
//            bool downFalling, bool triggerUp = false, bool triggerDown = false)
//        {
//            Assert.IsTrue(GetCounterData()[0].Initialized);
//            Assert.AreEqual(mode, GetCounterData()[0].Mode);
//            Assert.AreEqual(up, GetCounterData()[0].UpSourceChannel);
//            Assert.AreEqual(down, GetCounterData()[0].DownSourceChannel);
//            Assert.AreEqual(upRising, GetCounterData()[0].UpRisingEdge);
//            Assert.AreEqual(upFalling, GetCounterData()[0].UpFallingEdge);
//            Assert.AreEqual(downRising, GetCounterData()[0].DownRisingEdge);
//            Assert.AreEqual(downFalling, GetCounterData()[0].DownFallingEdge);
//            Assert.AreEqual(triggerUp, GetCounterData()[0].UpSourceTrigger);
//            Assert.AreEqual(triggerDown, GetCounterData()[0].DownSourceTrigger);
//        }

//        [Test]
//        public void TestCounterAllocateAll()
//        {
//            List<Counter> counters = new List<Counter>();
//            Assert.DoesNotThrow(() =>
//            {
//                for (int i = 0; i < NumCounters; i++)
//                {
//                    counters.Add(new Counter(i));
//                }
//            });
//            foreach (var counter in counters)
//            {
//                counter?.Dispose();
//            }
//        }

//        [Test]
//        public void TestCounterOverAllocate()
//        {
//            List<Counter> counters = new List<Counter>();
//            for (int i = 0; i < NumCounters; i++)
//            {
//                counters.Add(new Counter(i));
//            }

//            Counter counter = null;
//            Assert.Throws<UncleanStatusException>(() =>
//            {
//                counter = new Counter(NumCounters);
//            });
//            counter?.Dispose();

//            foreach (var c in counters)
//            {
//                c?.Dispose();
//            }

//        }

//        [Test]
//        public void TestCounterInvalidEncodingType()
//        {
//            Assert.Throws<ArgumentOutOfRangeException>(() =>
//            {
//                Counter cnt = new Counter(EncodingType.K4X, null, null, false);
//            });
//        }

//        [Test]
//        public void TestCounterDigitalUpSourceNull()
//        {
//            using (DigitalInput validSource = new DigitalInput(0))
//            {
//                Assert.Throws<ArgumentNullException>(() =>
//                {
//                    Counter cnt = new Counter(EncodingType.K2X, null, validSource, false);
//                });
//            }
//        }

//        [Test]
//        public void TestCounterDigitalDownSourceNull()
//        {
//            using (DigitalInput validSource = new DigitalInput(0))
//            {
//                Assert.Throws<ArgumentNullException>(() =>
//                {
//                    Counter cnt = new Counter(EncodingType.K2X, validSource, null, false);
//                });
//            }
//        }

//        [Test]
//        public void TestCounterSourceNull()
//        {
//            Assert.Throws<ArgumentNullException>(() =>
//            {
//                Counter cnt = new Counter((DigitalSource)null);
//            });
//        }

//        [Test]
//        public void TestCounterTriggerNull()
//        {
//            Assert.Throws<ArgumentNullException>(() =>
//            {
//                Counter cnt = new Counter((AnalogTrigger)null);
//            });
//        }

//        [Test]
//        public void TestCounterSetUpSourceWithExisitingUpSource()
//        {
//            using (Counter ctr = new Counter(6))
//            {
//                TestInit(0, 6, 0, true, false, false, false);
//                Assert.That(SimData.DIO[6].Initialized, Is.True);
//                using (DigitalInput input = new DigitalInput(5))
//                {
//                    ctr.SetUpSource(input);
//                    Assert.That(SimData.DIO[6].Initialized, Is.False);
//                    Assert.That(SimData.DIO[5].Initialized, Is.True);
//                    TestInit(0, 5, 0, true, false, false, false);
//                }
//                Assert.That(SimData.DIO[5].Initialized, Is.False);
//            }
//        }

//        [Test]
//        public void TestCounterSetUpAnalogSource()
//        {
//            using (Counter c = new Counter())
//            {
//                TestInit(0, 0, 0, false, false, false, false);
//                using (AnalogTrigger at = new AnalogTrigger(2))
//                {
//                    c.SetUpSource(at, AnalogTriggerType.State);
//                    TestInit(0, 2, 0, true, false, false, false, true);
//                }
//            }
//        }

//        [Test]
//        public void TestCounterSetUpAnalogSourceNull()
//        {
//            using (Counter c = new Counter())
//            {
//                TestInit(0, 0, 0, false, false, false, false);
//                Assert.Throws<ArgumentNullException>(() =>
//                {
//                    c.SetUpSource(null, AnalogTriggerType.State);
//                });
//            }
//        }

//        [Test]
//        public void TestSetUpSourceEdgeWithNullSource()
//        {
//            using (Counter c = new Counter())
//            {
//                TestInit(0, 0, 0, false, false, false, false);
//                Assert.Throws<Exception>(() =>
//                {
//                    c.SetUpSourceEdge(true, true);
//                });
//            }
//        }

//        [Test]
//        public void TestCounterSetDownSourceWithExisitingDownSource()
//        {
//            using (Counter ctr = new Counter())
//            {
//                TestInit(0, 0, 0, false, false, false, false);
//                ctr.SetDownSource(6);
//                TestInit(0, 0, 6, false, false, false, false);
//                Assert.That(SimData.DIO[6].Initialized, Is.True);
//                using (DigitalInput input = new DigitalInput(5))
//                {
//                    ctr.SetDownSource(input);
//                    Assert.That(SimData.DIO[6].Initialized, Is.False);
//                    Assert.That(SimData.DIO[5].Initialized, Is.True);
//                    TestInit(0, 0, 5, false, false, false, false);
//                }
//                Assert.That(SimData.DIO[5].Initialized, Is.False);
//            }
//        }

//        [Test]
//        public void TestCounterSetDownAnalogSource()
//        {
//            using (Counter c = new Counter())
//            {
//                TestInit(0, 0, 0, false, false, false, false);
//                using (AnalogTrigger at = new AnalogTrigger(2))
//                {
//                    c.SetDownSource(at, AnalogTriggerType.State);
//                    TestInit(0, 0, 2, false, false, false, false, false, true);
//                }
//            }
//        }

//        [Test]
//        public void TestCounterSetDownAnalogSourceNull()
//        {
//            using (Counter c = new Counter())
//            {
//                TestInit(0, 0, 0, false, false, false, false);
//                Assert.Throws<ArgumentNullException>(() =>
//                {
//                    c.SetDownSource(null, AnalogTriggerType.State);
//                });
//            }
//        }

//        [Test]
//        public void TestSetDownSourceEdgeWithNullSource()
//        {
//            using (Counter c = new Counter())
//            {
//                TestInit(0, 0, 0, false, false, false, false);
//                Assert.Throws<Exception>(() =>
//                {
//                    c.SetDownSourceEdge(true, true);
//                });
//            }
//        }

//        [Test]
//        public void TestCounterInit1()
//        {
//            using (Counter ctr = new Counter())
//            {
//                TestInit(0, 0, 0, false, false, false, false);
//            }
//        }

//        [Test]
//        public void TestCounterInit2()
//        {
//            using (DigitalInput di = new DigitalInput(5))
//            {
//                using (Counter ctr = new Counter(di))
//                {
//                    TestInit(0, 5, 0, true, false, false, false);
//                }
//            }
//        }

//        [Test]
//        public void TestCounterInit3()
//        {
//            using (Counter ctr = new Counter(6))
//            {
//                TestInit(0, 6, 0, true, false, false, false);
//            }
//        }

//        [Test]
//        public void TestCounterInit4()
//        {
//            using (DigitalInput us = new DigitalInput(3))
//            {
//                using (DigitalInput ds = new DigitalInput(4))
//                {
//                    using (Counter ctr = new Counter(EncodingType.K1X, us, ds, true))
//                    {
//                        TestInit(Mode.ExternalDirection, 3, 4, true, false, true, true);
//                    }
//                }
//            }
//        }

//        [Test]
//        public void TestCounterInit5()
//        {
//            using (AnalogTrigger at = new AnalogTrigger(2))
//            {
//                using (Counter ctr = new Counter(at))
//                {
//                    TestInit(0, 2, 0, true, false, false, false, true);
//                }
//            }
//        }

//        [Test]
//        public void TestCounterSetUpChannel()
//        {
//            using (Counter ctr = new Counter())
//            {
//                ctr.SetUpSource(2);
//                TestInit(0, 2, 0, true, false, false, false);
//            }
//        }

//        [Test]
//        public void TestCounterSetUpSource()
//        {
//            using (DigitalInput src = new DigitalInput(3))
//            {
//                using (Counter ctr = new Counter())
//                {
//                    ctr.SetUpSource(src);
//                    TestInit(0, 3, 0, true, false, false, false);
//                }
//            }
//        }

//        [Test]
//        public void TestCounterFree()
//        {


//            Assert.IsFalse(GetCounterData()[0].Initialized);
//            Assert.IsFalse(SimData.DIO[0].Initialized);
//            Assert.IsFalse(SimData.DIO[1].Initialized);
//            Counter ctr = null;
//            try
//            {
//                ctr = new Counter();
//                ctr.SetUpSource(0);
//                ctr.SetDownSource(1);

//                Assert.IsTrue(GetCounterData()[0].Initialized);
//                Assert.IsTrue(SimData.DIO[0].Initialized);
//                Assert.IsTrue(SimData.DIO[1].Initialized);
//            }
//            finally
//            {
//                ctr?.Dispose();
//            }
//            Assert.IsFalse(GetCounterData()[0].Initialized);
//            Assert.IsFalse(SimData.DIO[0].Initialized);
//            Assert.IsFalse(SimData.DIO[1].Initialized);
//        }

//        [TestCase(false, false)]
//        [TestCase(false, true)]
//        [TestCase(true, false)]
//        [TestCase(true, true)]
//        public void TestCountUpSourceEdge(bool rising, bool falling)
//        {
//            using (Counter c = new Counter())
//            {
//                c.SetUpSource(2);
//                c.SetUpSourceEdge(rising, falling);
//                Assert.AreEqual(rising, GetCounterData()[0].UpRisingEdge);
//                Assert.AreEqual(falling, GetCounterData()[0].UpFallingEdge);
//            }
//        }

//        [TestCase(false, false)]
//        [TestCase(false, true)]
//        [TestCase(true, false)]
//        [TestCase(true, true)]
//        public void TestCountDownSourceEdge(bool rising, bool falling)
//        {
//            using (Counter c = new Counter())
//            {
//                c.SetDownSource(2);
//                c.SetDownSourceEdge(rising, falling);
//                Assert.AreEqual(rising, GetCounterData()[0].DownRisingEdge);
//                Assert.AreEqual(falling, GetCounterData()[0].DownFallingEdge);
//            }
//        }

//        [Test]
//        public void TestCounterSetUpDownMode()
//        {
//            using (Counter c = new Counter())
//            {
//                c.SetUpDownCounterMode();
//                Assert.AreEqual(Mode.TwoPulse, GetCounterData()[0].Mode);
//            }
//        }

//        [Test]
//        public void TestCounterSetExtDirMode()
//        {
//            using (Counter c = new Counter())
//            {
//                c.SetExternalDirectionMode();
//                Assert.AreEqual(Mode.ExternalDirection, GetCounterData()[0].Mode);
//            }
//        }

//        [TestCase(false)]
//        [TestCase(true)]
//        public void TestCounterSetSemiPeriodMode(bool high)
//        {
//            using (Counter c = new Counter())
//            {
//                c.SetSemiPeriodMode(high);
//                Assert.AreEqual(Mode.Semiperiod, GetCounterData()[0].Mode);
//                Assert.AreEqual(high, GetCounterData()[0].UpRisingEdge);
//                Assert.IsFalse(GetCounterData()[0].UpdateWhenEmpty);
//            }
//        }

//        [TestCase(1.0)]
//        [TestCase(4.5)]
//        [TestCase(1.5)]
//        public void TestCounterSetPlMode(double thresh)
//        {
//            using (Counter c = new Counter())
//            {
//                c.SetPulseLengthMode(thresh);
//                Assert.AreEqual(Mode.PulseLength, GetCounterData()[0].Mode);
//                Assert.AreEqual(thresh, GetCounterData()[0].PulseLengthThreshold);
//            }
//        }

//        [Test]
//        public void TestCounterGet()
//        {
//            using (Counter c = new Counter())
//            {
//                GetCounterData()[0].Count = 5;
//                Assert.AreEqual(5, c.Get());

//                GetCounterData()[0].Count = 258;
//                Assert.AreEqual(258, c.Get());
//            }
//        }

//        [Test]
//        public void TestCounterGetDistance()
//        {
//            using (Counter c = new Counter())
//            {
//                c.DistancePerPulse = 2;

//                GetCounterData()[0].Count = 5;
//                Assert.AreEqual(5 * 2, c.GetDistance());

//                c.DistancePerPulse = 5;
//                GetCounterData()[0].Count = 258;
//                Assert.AreEqual(258 * 5, c.GetDistance());
//            }
//        }

//        [Test]
//        public void TestCounterReset()
//        {
//            using (Counter c = new Counter())
//            {
//                GetCounterData()[0].Count = 5;
//                Assert.AreEqual(5, c.Get());
//                Assert.IsFalse(GetCounterData()[0].Reset);

//                c.Reset();

//                Assert.AreEqual(0, GetCounterData()[0].Count);
//                Assert.AreEqual(0, c.Get());
//                Assert.IsTrue(GetCounterData()[0].Reset);
//            }
//        }

//        [TestCase(1.0)]
//        [TestCase(4.5)]
//        [TestCase(1.5)]
//        public void TestCounterSetMaxPeriod(double period)
//        {
//            using (Counter c = new Counter())
//            {
//                Assert.AreEqual(0.5, GetCounterData()[0].MaxPeriod, 0.0001);
//                c.MaxPeriod = period;
//                Assert.AreEqual(period, GetCounterData()[0].MaxPeriod, 0.0001);
//            }
//        }

//        [TestCase(true)]
//        [TestCase(false)]
//        public void TestCounterSetUpdateEmpty(bool enabled)
//        {
//            using (Counter c = new Counter())
//            {
//                Assert.IsFalse(GetCounterData()[0].UpdateWhenEmpty);
//                c.UpdateWhenEmpty = enabled;
//                Assert.AreEqual(enabled, GetCounterData()[0].UpdateWhenEmpty);
//            }
//        }

//        [Test]
//        public void TestCounterGetStopped()
//        {
//            using (Counter c = new Counter())
//            {
//                GetCounterData()[0].Period = 6;
//                GetCounterData()[0].MaxPeriod = 7;
//                Assert.IsFalse(c.GetStopped());
//                GetCounterData()[0].Period = 7;
//                GetCounterData()[0].MaxPeriod = 3;
//                Assert.IsTrue(c.GetStopped());
//            }
//        }

//        [TestCase(true)]
//        [TestCase(false)]
//        public void TestCounterGetDirection(bool dir)
//        {
//            using (Counter c = new Counter())
//            {
//                GetCounterData()[0].Direction = dir;
//                Assert.AreEqual(dir, c.GetDirection());
//            }
//        }

//        [TestCase(true)]
//        [TestCase(false)]
//        public void TestCounterSetReverseDirection(bool dir)
//        {
//            using (Counter c = new Counter())
//            {
//                c.SetReverseDirection(dir);
//                Assert.AreEqual(dir, GetCounterData()[0].ReverseDirection);
//            }
//        }

//        [TestCase(1.0)]
//        [TestCase(5.76)]
//        [TestCase(2.222)]
//        public void TestCounterGetPeriod(double period)
//        {
//            using (Counter c = new Counter())
//            {
//                GetCounterData()[0].Period = period;
//                Assert.AreEqual(period, c.GetPeriod(), 0.00001);
//            }
//        }

//        [TestCase(1)]
//        [TestCase(3)]
//        [TestCase(10)]
//        public void TestCounterSetGetSamples(int samples)
//        {
//            using (Counter c = new Counter())
//            {
//                c.SamplesToAverage = samples;
//                Assert.AreEqual(samples, GetCounterData()[0].SamplesToAverage);
//                Assert.AreEqual(samples, c.SamplesToAverage);
//            }
//        }

//        [Test]
//        public void TestPidSourceTypeGetSet()
//        {
//            using (Counter c = new Counter())
//            {
//                Assert.That(c.PIDSourceType, Is.EqualTo(PIDSourceType.Displacement));
//                c.PIDSourceType = PIDSourceType.Rate;
//                Assert.That(c.PIDSourceType, Is.EqualTo(PIDSourceType.Rate));
//            }
//        }

//        [Test]
//        public void TestPidSourceTypeGetSetInterfacee()
//        {
//            using (Counter c = new Counter())
//            {
//                IPIDSource pidSource = c;
//                Assert.That(pidSource.PIDSourceType, Is.EqualTo(PIDSourceType.Displacement));
//                pidSource.PIDSourceType = PIDSourceType.Rate;
//                Assert.That(pidSource.PIDSourceType, Is.EqualTo(PIDSourceType.Rate));
//            }
//        }

//        [Test]
//        public void TestPidGetDisplacement()
//        {
//            using (Counter c = new Counter())
//            {
//                c.PIDSourceType = PIDSourceType.Displacement;
//                GetCounterData()[0].Count = 50;
//                Assert.That(c.PidGet(), Is.EqualTo(50));
//            }
//        }

//        [Test]
//        public void TestPidGetRate()
//        {
//            using (Counter c = new Counter())
//            {
//                c.PIDSourceType = PIDSourceType.Rate;
//                GetCounterData()[0].Period = 1.0 / 50;
//                Assert.That(c.PidGet(), Is.EqualTo(50));
//            }
//        }

//        [Test]
//        public void TestSmartDashboardType()
//        {
//            using (Counter s = new Counter())
//            {
//                Assert.That(s.SmartDashboardType, Is.EqualTo("Counter"));
//            }
//        }

//        [Test]
//        public void TestUpdateTableNull()
//        {
//            using (Counter s = new Counter())
//            {
//                Assert.DoesNotThrow(() =>
//                {
//                    s.UpdateTable();
//                });
//            }
//        }

//        [Test]
//        public void TestInitTable()
//        {
//            using (Counter s = new Counter())
//            {
//                ITable table = new MockNetworkTable();
//                Assert.DoesNotThrow(() =>
//                {
//                    s.InitTable(table);
//                });
//                Assert.That(s.Table, Is.EqualTo(table));
//            }

//        }

//        [Test]
//        public void TestStartLiveWindowMode()
//        {
//            using (Counter s = new Counter())
//            {
//                Assert.DoesNotThrow(() =>
//                {
//                    s.StartLiveWindowMode();
//                });
//            }
//        }

//        [Test]
//        public void TestStopLiveWindowMode()
//        {
//            using (Counter s = new Counter())
//            {
//                Assert.DoesNotThrow(() =>
//                {
//                    s.StopLiveWindowMode();
//                });
//            }
//        }
//    }
//}
