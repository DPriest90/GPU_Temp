using System;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Hardware.CPU;

/// <summary>
/// A small always running program that gets and displays the current temperaure of the GPU. Uses the
/// OpenHardwareMonitor library which I got with NuGet package manager.
/// </summary>
public class Program
{
    #region Properties

        private static int _currentGpuTemp = 0;
        private static bool _tempFound = false;

        private static readonly int MinimumTempAlert = 75;

    /// <summary>
    /// This controls the 
    /// </summary>
        public static bool TempFound
        {
            get { return _tempFound; }
            set { _tempFound = value; }
        }

        /// <summary>
        /// The up-to-date Graphics Card temperature
        /// </summary>
        public static int CurrentGpuTemp
        {
            get { return _currentGpuTemp; }
            set { _currentGpuTemp = value; }
        }

    #endregion

    /// <summary>
    /// Program entry point
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        int minimumHighTempWarning = 75;

        // Initialize the computer hardware sensor manager
        Computer computer = new Computer()
        {
            GPUEnabled = true,  // Enable GPU monitoring
            MainboardEnabled = true // Optionally enable other hardware monitoring
        };

        // Start monitoring the hardware
        computer.Open();
        
        // If this bool becomes false, exit the loop (stop the program).
        bool continueProgram = true;

        do
        {
            GetGpuTemp(computer);

            if (_tempFound == false)
            {
                Console.Clear();
                Console.WriteLine("No GPU temperature sensor found.");
            }

            // Wait for a few seconds before updating again
            Thread.Sleep(2000); // Update every 2 seconds (2000 milliseconds)
        }
        while (continueProgram == true);

        // Close the hardware monitoring (though it will never reach here unless the program ends)
        computer.Close();

        // Begin loop. Only stop when the above bool is changed to false (this is how we monitor continuously).
        //while (continueProgram == true)
        //{
        //    GetGpuTemp(computer);            

        //    if (!_tempFound)
        //    {
        //        Console.Clear();
        //        Console.WriteLine("No GPU temperature sensor found.");                
        //    }

        //    // Wait for a few seconds before updating again
        //    Thread.Sleep(2000); // Update every 2 seconds (2000 milliseconds)
        //}

        // Close the hardware monitoring (though it will never reach here unless the program ends)
        //computer.Close();
    }

    /// <summary>
    /// Isolate the GPU Temperature sensor and output the current temperature to screen
    /// </summary>
    /// <param name="comp">The computer we are running this program on (OpenHardwareMonitor.Computer object)</param>
    private static void GetGpuTemp(Computer comp)
    {   
        // Loop through all the hardware to find the GPU sensors
        foreach (IHardware hardware in comp.Hardware)
        {
            // Only work with the GPU and ignore all other hardware components of the computer
            if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAti)
            {
                // Ensure we have the latest possible data available
                hardware.Update(); 

                // Get any sensor whose type is that of "Temperature"
                var tempSensor = hardware.Sensors.
                    Where(x => x.SensorType == SensorType.Temperature).
                    FirstOrDefault();

                // Update our Temperature property, so that we have access to the temperature globally within
                // this file program file. Convert from a float value to an integer value by cast.
                _currentGpuTemp = (int)tempSensor.Value.GetValueOrDefault();

                // Make sure our sensor object is actually populated with data and then output information to screen
                if (tempSensor != null)
                {
                    _tempFound = true;
                    // Clear the console so we only ever show the latest temperature
                    Console.Clear();                    
                    Console.WriteLine(@"GPU Temperature: {0}", _currentGpuTemp);
                    //if (_currentGpuTemp > MinimumTempAlert)
                    //{
                    //    Console.WriteLine(@"Current GPU Temp Is Starting To Get Hot!");
                    //}                    
                }
                else
                    _tempFound = false;
            }
        }
    }

}