using System;
using System.Text;

public class Utils
{
    public static int GetLength(float data)
    {
        return BitConverter.GetBytes(data).Length;
    }
    public static int GetLength(int data)
    {
        return BitConverter.GetBytes(data).Length;
    }
    public static int GetLength(string data)
    {
        return Encoding.UTF8.GetBytes(data).Length;
    }
    public static int GetLength(long data)
    {
        return BitConverter.GetBytes(data).Length;
    }
    public static int GetLength(bool data)
    {
        return BitConverter.GetBytes(data).Length;
    }
    public static int GetLength(byte data)
    {
        return 1;
    }
}
