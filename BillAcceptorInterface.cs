using System;
using System.IO.Ports;
using System.Threading;

class Program
{
    static int minwidth = PULSE_WIDTH - (PULSE_WIDTH * PULSE_DELTA / 100); //minimum pulse width
    static int maxwidth = PULSE_WIDTH + (PULSE_WIDTH * PULSE_DELTA / 100); // maximum pulse width

    static int BILL_ENABLE_PIN = A1; // To bill acceptor enable input
    static int BILL_PULSE_PIN = A0; // To bill acceptor pulse output
    static int rs = 12, en = 11, d4 = 5, d5 = 4, d6 = 3, d7 = 2;// not needed if you don't use LCD

    static unsigned long duration = 0, pulses = 0, ppulses = 0;
    static bool busy = false;

    static SerialPort arduino = new SerialPort("COM3", 9600);
    static Timer timer = new Timer(1000);

    static void Main(string[] args)
    {
        //Open the serial port
        arduino.Open();

        //Initialize timer
        timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        timer.Enabled = true;

        //Continuously read data from the serial port
        while (true)
        {
            int impulseDuration = arduino.ReadByte();

            if (impulseDuration > minwidth && impulseDuration < maxwidth)
            {
                lock (pulses)
                {
                    pulses++;
                }
            }
        }
    }

    private static void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        if (digitalRead(BILL_PULSE_PIN) == HIGH)
        {
            duration++;
            if (duration > 1000) duration = 1000;
        }
        else
        {
            duration = 0;
        }
    }

    //Thread safe read and clear pulse count
    //If a timer interrupt occurs while busy the pulse will be stored in ppulses and added at the next interrupt.
    //Don't call this function in a fast loop, leave some time for timer interrupt to add stored pulses to the count.
    static unsigned long readpulses()
    {
        lock (pulses)
        {
            return pulses;
        }
    }
    static void clearpulses()
    {
        lock (pulses)
        {
            pulses = 0;
        }
    }

    // Macros to enable and disable the bill acceptor through bill acceptor "Enable" input
    static void BILL_ACCEPTOR_ENABLE()
    {
        arduino.Write(new byte[] { (byte)BILL_ENABLE_PIN, 0 }, 0, 2);
    }
    static void BILL_ACCEPTOR_DISABLE()
    {
        arduino.Write(new byte[] { (byte)BILL_ENABLE_PIN, 1 }, 0, 2);
    }
}
