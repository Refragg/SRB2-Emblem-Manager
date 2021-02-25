using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BC = System.BitConverter;

namespace SRB2_Emblem_Manager
{
    public class EmblemsChangedEventArgs : EventArgs
    {
        public static readonly EmblemsChangedEventArgs FullInfo = new EmblemsChangedEventArgs(true);
        public static readonly EmblemsChangedEventArgs NonFullInfo = new EmblemsChangedEventArgs(false);

        //when IsFullInfo == false it means that only the emblem count changed
        public bool IsFullInfo;
        
        private EmblemsChangedEventArgs(bool isfullinfo)
        {
            IsFullInfo = isfullinfo;
        }
    }

    public class Memory
    {
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess,
            int lpBaseAddress, byte[] lpBuffer, int dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess,
            int lpBaseAddress, byte[] lpBuffer, int dwSize, IntPtr lpNumberOfBytesWritten);

        /* 2.2.8 addresses */
        private const int MAX_EXTRA_EMBLEMS_ADDRESS = 0x0082E0E0; //addresses to get the number of emblems and extra emblems currently loaded in the game so we don't have to check for all 16 and 512 possible emblems
        private const int MAX_EMBLEMS_ADDRESS = MAX_EXTRA_EMBLEMS_ADDRESS + 4;

        private const int EXTRA_EMBLEMS_ADDRESS = 0x059F42C0; //address that stores the first extra emblem object in memory
        private const int EMBLEMS_ADDRESS = EXTRA_EMBLEMS_ADDRESS + (68 * 16); //+ 0x440 (length of 1 extra emblem object in bytes * number of the maximum amount of extra emblems allowed by the game)

        private const int EXTRA_EMBLEMS_COLLECTED_ADDRESS = EXTRA_EMBLEMS_ADDRESS + 66; //addresses to get only the collected byte in the extra emblem / emblem object
        private const int EMBLEMS_COLLECTED_ADDRESS = EMBLEMS_ADDRESS + 126;
        //

        public bool isGameHooked = false;

        public Process gameProc;

        public Emblem[] previousEmblems = new Emblem[0]; //assigning a dummy value so that it doesn't complain about it being null
        public ExtraEmblem[] previousExtraEmblems = new ExtraEmblem[0];

        public Emblem[] Emblems = new Emblem[0];
        public ExtraEmblem[] ExtraEmblems = new ExtraEmblem[0];

        private int lastEmblemCount;

        public event EventHandler<EmblemsChangedEventArgs> EmblemsChangedEvent;

        Thread memoryChecking;

        public Memory()
        {
            memoryChecking = new Thread(() => { while (true) MemoryCheckLoop(); });
            memoryChecking.Start();
        }

        //methods to be accessed by other classes that executes the actual method after some check
        public int GetEmblemCount()
        {
            if (isGameHooked)
            {
                _GetEmblemCount();
                return lastEmblemCount;
            }
            return -1;
        }
        
        public void ReadEmblemFullInfo()
        {
            if(isGameHooked)
            {
                _ReadEmblemFullInfo();
            }
            return;
        }

        private bool Hook()
        {
            try
            {
                gameProc = Process.GetProcessesByName("srb2win").First();
                gameProc.Exited += GameProc_Exited;
                gameProc.EnableRaisingEvents = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void GameProc_Exited(object sender, EventArgs e)
        {
            gameProc.Exited -= GameProc_Exited;
            isGameHooked = false;
        }

        private void MemoryCheckLoop()
        {
            if (!isGameHooked)
            {
                if (Hook())
                {
                    isGameHooked = true;
                    _ReadEmblemFullInfo();
                    _GetEmblemCount();
                    return;
                }

                Thread.Sleep(1000); 
                return;
            }

            _GetEmblemCount();
            Thread.Sleep(2000);
        }

        //methods to be accessed by this class because it already knows whether or not the game is hooked
        private void _GetEmblemCount()
        {
            byte[] currentEmblem = new byte[1];

            int emblemCount = 0;
            int address = EMBLEMS_COLLECTED_ADDRESS;
            for (int i = 0; i < Emblems.Length; i++)
            {
                ReadProcessMemory(gameProc.Handle, address, currentEmblem, 1, IntPtr.Zero);
                if (currentEmblem[0] == 1)
                {
                    emblemCount++;
                    Emblems[i].collected = 1;
                }
                address += 128;
            }

            int extraEmblemsCount = 0;
            address = EXTRA_EMBLEMS_COLLECTED_ADDRESS;
            for (int i = 0; i < ExtraEmblems.Length; i++)
            {
                ReadProcessMemory(gameProc.Handle, address, currentEmblem, 1, IntPtr.Zero);
                if (currentEmblem[0] == 1)
                {
                    extraEmblemsCount++;
                    ExtraEmblems[i].collected = 1;
                }
                address += 68;
            }

            int total = emblemCount + extraEmblemsCount;

            bool countChanged = lastEmblemCount != total ? true : false;

            lastEmblemCount = total;
            if (countChanged)
            {
                EmblemsChangedEvent(this, EmblemsChangedEventArgs.NonFullInfo);
            }
        }

        private void _ReadEmblemFullInfo()
        {
            //loop here in case it runs this too early and reads 0 max emblems because it's not initialized in the game yet
            int tries = 0, maxEmblems = 0, maxExtra = 0;
            do
            {
                //read the number of emblems loaded into the game
                byte[] maxEmblemsBuffer = new byte[4];
                byte[] maxExtraBuffer = new byte[4];

                ReadProcessMemory(gameProc.Handle, MAX_EMBLEMS_ADDRESS, maxEmblemsBuffer, 4, IntPtr.Zero);
                ReadProcessMemory(gameProc.Handle, MAX_EXTRA_EMBLEMS_ADDRESS, maxExtraBuffer, 4, IntPtr.Zero);

                maxEmblems = BC.ToInt32(maxEmblemsBuffer, 0);
                maxExtra = BC.ToInt32(maxExtraBuffer, 0);

                if(maxEmblems + maxExtra != 0)
                {
                    break;
                }

                Thread.Sleep(2000);
                tries++;
            }
            while (tries < 5);

            Emblems = new Emblem[maxEmblems];
            ExtraEmblems = new ExtraEmblem[maxExtra];

            byte[] currentEmblem = new byte[128]; //size of 1 emblem object in memory

            int address = EMBLEMS_ADDRESS;
            for (int i = 0; i < Emblems.Length; i++)
            {
                ReadProcessMemory(gameProc.Handle, address, currentEmblem, currentEmblem.Length, IntPtr.Zero);
                //WriteProcessMemory(gameProc.Handle, address + 4, new byte[1] { 1 }, 1, IntPtr.Zero); //for the fun stuff
                Emblems[i] = new Emblem()
                {
                    type = currentEmblem[0],
                    tag = BC.ToInt16(currentEmblem, 2),
                    level = BC.ToInt16(currentEmblem, 4),
                    sprite = Convert.ToChar(currentEmblem[6]),
                    color = BC.ToUInt16(currentEmblem, 8),
                    var = BC.ToInt32(currentEmblem, 12),
                    hint = Encoding.ASCII.GetString(currentEmblem, 16, 110),
                    collected = currentEmblem[126]
                };
                
                address += 128;
            }

            currentEmblem = new byte[68]; //size of 1 extra emblem object in memory

            address = EXTRA_EMBLEMS_ADDRESS;
            for (int i = 0; i < ExtraEmblems.Length; i++)
            {
                ReadProcessMemory(gameProc.Handle, address, currentEmblem, currentEmblem.Length, IntPtr.Zero);

                ExtraEmblems[i] = new ExtraEmblem()
                {
                    name = Encoding.ASCII.GetString(currentEmblem, 0, 20),
                    description = Encoding.ASCII.GetString(currentEmblem, 20, 40),
                    conditionset = currentEmblem[60],
                    showconditionset = currentEmblem[61],
                    sprite = Convert.ToChar(currentEmblem[62]),
                    color = BC.ToUInt16(currentEmblem, 64),
                    collected = currentEmblem[66],
                };

                address += 68;
            }

            if (!(previousEmblems.SequenceEqual(Emblems) && previousExtraEmblems.SequenceEqual(ExtraEmblems)))
            {
                EmblemsChangedEvent(this, EmblemsChangedEventArgs.FullInfo);
            }

            previousEmblems = (Emblem[])Emblems.Clone();
            previousExtraEmblems = (ExtraEmblem[])ExtraEmblems.Clone();
        }
    }
}